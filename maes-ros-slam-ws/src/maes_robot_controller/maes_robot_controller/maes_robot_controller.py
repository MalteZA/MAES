# Copyright 2022 MAES
# 
# This file is part of MAES
# 
# MAES is free software: you can redistribute it and/or modify it under
# the terms of the GNU General Public License as published by the
# Free Software Foundation, either version 3 of the License, or (at your option)
# any later version.
# 
# MAES is distributed in the hope that it will be useful, but WITHOUT
# ANY WARRANTY; without even the implied warranty of MERCHANTABILITY
# or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General
# Public License for more details.
# 
# You should have received a copy of the GNU General Public License along
# with MAES. If not, see http://www.gnu.org/licenses/.
# 
# Contributors: Malte Z. Andreasen, Philip I. Holler and Magnus K. Jensen
# 
# Original repository: https://github.com/MalteZA/MAES
import math
import random
from dataclasses import dataclass
from typing import Callable

import geometry_msgs.msg
from maes_msgs.msg import State
from maes_msgs.srv import BroadcastToAll, DepositTag

import time

from action_msgs.msg import GoalStatus
from geometry_msgs.msg import Pose, PoseStamped, TransformStamped
from geometry_msgs.msg import PoseWithCovarianceStamped
from lifecycle_msgs.srv import GetState
from nav2_msgs.action import NavigateThroughPoses, NavigateToPose
from nav2_msgs.action._navigate_to_pose import NavigateToPose_Feedback
from nav2_msgs.msg import *
from nav_msgs.msg import OccupancyGrid
from rclpy import Future
from rclpy.impl.rcutils_logger import RcutilsLogger
from tf2_msgs.msg import TFMessage

import rclpy

from rclpy.action import ActionClient
from rclpy.node import Node
from rclpy.qos import QoSDurabilityPolicy, QoSHistoryPolicy, QoSReliabilityPolicy, ReliabilityPolicy, HistoryPolicy
from rclpy.qos import QoSProfile


class RobotController(Node):

    def __init__(self):
        # The name and namespace is usually overridden be the launch file
        super().__init__(node_name="maes_robot_controller")

        # All topics have prefixed with the namespace of the node e.g. /robot0
        self._topic_namespace_prefix = self.get_namespace()

        # Used to print to console running ros2
        self.logger = MaesLogger(logger=self.get_logger())

        # Register fields for navigation
        self.nav_goal_handle = None
        self.nav_result_future: Future = None
        self.nav_feedback: NavigateToPose_Feedback = None
        self.nav_status = None

        # Interface to use global costmap for nagivation
        self.global_costmap: MaesCostmap = MaesCostmap(self.logger)

        # Robot position set by a callback function from the tf topic
        self.robot_position: TransformStamped = None

        # Registers subscribers, services and actions and assigns to variables below.
        # Subscribers
        self._state_sub = None
        self._global_costmap_sub = None
        self._tf_sub = None
        # Service clients
        self._broadcast_srv = None
        self._deposit_env_tag_srv = None
        # Navigation action client
        self._nav_to_pose_client: ActionClient = None
        self._register_subs_srvs_actions()  # Assign to variables

        self.state: State = None


    def wait_for_maes_to_start_simulation(self):
        while self.state is None or self.global_costmap.costmap is None:
            self.logger.log_info("Waiting for MAES simulation to start...")
            time.sleep(0.1) # A tick in MAES is 0.1 seconds
            rclpy.spin_once(self)

    def nav_to_pos(self, pose_x, pose_y):
        goal_pose = PoseStamped()
        goal_pose.header.frame_id = 'map'
        goal_pose.header.stamp = self.get_clock().now().to_msg()
        goal_pose.pose.position.x = float(pose_x)
        goal_pose.pose.position.y = float(pose_y)
        goal_pose.pose.position.z = 0.0
        goal_pose.pose.orientation.x = 0.0
        goal_pose.pose.orientation.y = 0.0
        goal_pose.pose.orientation.z = 0.0
        goal_pose.pose.orientation.w = 1.0

        self.nav_to_pose_with_orientation(goal_pose)

    def deposit_tag(self, tag_msg):
        request = DepositTag.Request()
        request.msg = tag_msg
        self._deposit_env_tag_srv.call_async(request=request)

    def broadcast_msg(self, msg):
        request = BroadcastToAll.Request()
        request.msg = msg
        self._broadcast_srv.call_async(request=request)

    def nav_to_pose_with_orientation(self, pose: PoseStamped):
        # Sends a `NavToPose` action request and waits for completion
        self.logger.log_debug("Waiting for 'NavigateToPose' action server")
        while not self._nav_to_pose_client.wait_for_server(timeout_sec=1.0):
            self.logger.log_info("'NavigateToPose' action server not available, waiting...")

        goal_msg = NavigateToPose.Goal()
        goal_msg.pose = pose

        self.logger.log_info('Navigating to goal: ' + str(pose.pose.position.x) + ' ' +
                             str(pose.pose.position.y) + '...')

        def _feedback_callback(msg):
            self.nav_feedback = msg.feedback

        send_goal_future = self._nav_to_pose_client.send_goal_async(goal_msg, _feedback_callback)

        rclpy.spin_until_future_complete(self, send_goal_future)
        self.nav_goal_handle = send_goal_future.result()

        if not self.nav_goal_handle.accepted:
            self.logger.log_error('Goal to ' + str(pose.pose.position.x) + ' ' +str(pose.pose.position.y) + 'was '
                                                                                                            'rejected!')
            return False

        self.nav_result_future = self.nav_goal_handle.get_result_async()
        return True



    def cancel_nav(self):
        if self.nav_result_future is None:
            self.logger.log_debug("Tried to cancel nav, but nav_result_future is None")

        if self.nav_result_future:
            self.logger.log_info('Canceling current goal.')
            future = self.nav_goal_handle.cancel_goal_async()
            rclpy.spin_until_future_complete(self, future)
        return

    def is_nav_complete(self):
        if not self.nav_result_future:
            # task was cancelled or completed
            self.logger.log_info('Nav result future is None,. Task was cancelled or completed')
            return True


        rclpy.spin_until_future_complete(self, self.nav_result_future, timeout_sec=0.1)
        if self.nav_result_future.result():
            self.nav_status = self.nav_result_future.result().status
            if self.nav_status != GoalStatus.STATUS_SUCCEEDED:
                goal_status_dict = {
                    0: 'UNKNOWN',
                    1: 'ACCEPTED',
                    2: 'EXECUTING',
                    3: 'CANCELING',
                    4: 'SUCCEEDED',
                    5: 'CANCELED',
                    6: 'ABORTED',
                }
                GoalStatus.get_fields_and_field_types()
                self.logger.log_info('Goal failed with status with reason: {0}'.format(goal_status_dict[self.nav_status]))
                return True
        else:
            # Timed out, still processing, not complete yet
            return False

        return True

    def _register_subs_srvs_actions(self):
        # Declare topics
        state_topic = self._topic_namespace_prefix + "/maes_state"
        broadcast_srv_topic = self._topic_namespace_prefix + "/maes_broadcast"
        deposit_env_tag_srv_topic = self._topic_namespace_prefix + "/maes_deposit_tag"
        nav_to_pose_topic = self._topic_namespace_prefix + "/navigate_to_pose"
        global_costmap_2d_topic = self._topic_namespace_prefix + "/global_costmap/costmap"
        tf2_topic = self._topic_namespace_prefix + "/tf"

        # Register service clients
        self._broadcast_srv = self.create_client(srv_type=BroadcastToAll, srv_name=broadcast_srv_topic)
        self._deposit_env_tag_srv = self.create_client(srv_type=DepositTag, srv_name=deposit_env_tag_srv_topic)

        # Wait for services to be active
        while not self._broadcast_srv.wait_for_service(timeout_sec=1) or not self._deposit_env_tag_srv.wait_for_service(timeout_sec=1):
            self.logger.log_info("{0} waiting for either broadcast or deposit tag services".format(self._topic_namespace_prefix))

        # Create navigation action client
        self._nav_to_pose_client = ActionClient(self, NavigateToPose, nav_to_pose_topic)

        # Wait for action service to be active
        while not self._nav_to_pose_client.wait_for_server(timeout_sec=1):
            self.logger.log_info("{0} waiting for either nav action client to start".format(self._topic_namespace_prefix))


        # Quality of service profile for subscriptions
        qos_profile = QoSProfile(
            reliability=ReliabilityPolicy.RELIABLE,
            history=HistoryPolicy.KEEP_LAST,
            depth=1
        )

        def save_robot_state_callback(state: State):
            self.state = state

        # Create subscribers
        self._state_sub = self.create_subscription(msg_type=State,
                                                   topic=state_topic,
                                                   callback=save_robot_state_callback,
                                                   qos_profile=qos_profile)
        self._global_costmap_sub = self.create_subscription(msg_type=OccupancyGrid,
                                                            topic=global_costmap_2d_topic,
                                                            callback=self.global_costmap.update_costmap,
                                                            qos_profile=qos_profile)

        def save_robot_position_callback(msg: TFMessage):
            odom = list(filter(lambda e: e.header.frame_id == "odom", msg.transforms))
            if len(odom) == 1:
                self.robot_position = odom[0]


        self._tf_sub = self.create_subscription(msg_type=TFMessage,
                                                topic=tf2_topic,
                                                callback=save_robot_position_callback,
                                                qos_profile=qos_profile)


def main(args=None):
    rclpy.init(args=args)

    # Initialise controller
    robot = RobotController()
    robot.wait_for_maes_to_start_simulation()
    '''
    INSTRUCTIONS:
    Print to ROS Terminal (there exists info, error, warn, debug tags):
    robot.logger.log_info("From maes_robot_controller.py")

    Movement using Nav2:
    robot.nav_to_pos(0,0)
    robot.cancel_nav()
    robot.is_nav_complete() -> Bool
    robot.nav_feedback // Contains feedback from current navigation
    robot.robot_position.transform.translation.x // Get position x
    robot.robot_position.transform.translation.y // Get position y

    Use services:
    robot.broadcast_msg(msg="Testing broadcasting")
    robot.deposit_tag(tag_msg="Content of env_tag")

    Use robot state from MAES:
    robot.state.tick // Get current logic tick from MAES. With default settings a tick lasts for 0.1 seconds

    If you want a loop that runs until killed by the terminal, use the following
    while rclpy.ok():
        rclpy.spin_once(robot)

    Below is an example of a simple frontier algorithm. Feel free to delete
    '''

    # Declaration of logic variables
    next_goal: Coord2D = None # Used for frontier example
    next_goal_costmap_index: int = None # Used for frontier example algorithm

    # This method returns true if the tile is not itself unknown, but has 2 neighbors, that are unknown
    def is_frontier(map_index: int, costmap: MaesCostmap):
        # -1 = unknown, 0 = certain to be open, 100 = certain to be obstacle
        # It is itself unknown
        if costmap.costmap.data[map_index] == -1:
            return False
        # It is itself a wall
        if costmap.costmap.data[map_index] >= 65:
            return False

        return costmap.has_at_least_n_unknown_neighbors(index=map_index, n=2)

    while rclpy.ok():
        rclpy.spin_once(robot)

        # If no target found
        if next_goal is None or robot.is_nav_complete():
            # Find index of first tile in costmap that is a frontier
            goal_frontier_tile_index = next((index for index, value in enumerate(robot.global_costmap.costmap.data) if is_frontier(index, robot.global_costmap)), None)

            # No more frontiers found, loop again
            if goal_frontier_tile_index is None:
                robot.logger.log_info("Robot with namespace {0} is has found no more frontiers".format(robot._topic_namespace_prefix))
                continue

            next_goal = robot.global_costmap.costmap_index_to_pos(goal_frontier_tile_index)
            next_goal_costmap_index = goal_frontier_tile_index
            robot.deposit_tag("From tick {0}".format(robot.state.tick)) # Deposit tag every time a new target/goal is found
            robot.nav_to_pos(next_goal.x, next_goal.y)
        # If target found but not yet reached, i.e. it is still a frontier
        elif is_frontier(next_goal_costmap_index, robot.global_costmap):
            # This section allows for logging feedback etc. e.g.
            # self.logger.log_info("Frontier value: {0}".format(self.global_costmap.costmap.data[self.next_target_costmap_index]))
            continue
        # If target is explored, i.e. next_target not None and not frontier
        else:
            robot.logger.log_info("Robot with namespace {0} explored its target at ({1},{2})".format(robot._topic_namespace_prefix,
                                                                                                          next_goal.x,
                                                                                                          next_goal.y))
            next_goal_costmap_index = None
            next_goal = None
            robot.cancel_nav()


if __name__ == '__main__':
    main()

"""
Helper classes from here
"""
class MaesLogger:
    """
    This class was made to pass a simple logging interface to both the costmap and the controller
    """
    def __init__(self, logger) -> None:
        super().__init__()
        self._logger: RcutilsLogger = logger

    def log_info(self, msg):
        self._logger.info(msg)

    def log_warn(self, msg):
        self._logger.warn(msg)

    def log_error(self, msg):
        self._logger.error(msg)

    def log_debug(self, msg):
        self._logger.debug(msg)

@dataclass
class Coord2D:
    x: float
    y: float

    def distance_to(self, pose: TransformStamped):
        pose_point = [pose.transform.translation.x, pose.transform.translation.y]
        return math.dist([self.x, self.y], pose_point)


class MaesCostmap:
    def __init__(self, logger: MaesLogger) -> None:
        super().__init__()
        self.logger = logger
        # The map data, in row-major order, starting with (0, 0).  Occupancy
        # probabilities are in the range [0,100].  Unknown is -1.
        self.costmap: OccupancyGrid = None


    def update_costmap(self, costmap: OccupancyGrid):
        self.costmap = costmap

    def distance_to_costmap_index(self, from_coord: Coord2D, index: int) -> float:
        index_pos = self.costmap_index_to_pos(index)
        return math.sqrt(math.pow(from_coord.x - index_pos.x, 2) +
                         math.pow(from_coord.y - index_pos.y, 2))


    def costmap_index_to_pos(self, index: int) -> Coord2D:
        y_tile: int = int(index / self.costmap.info.width)
        x_tile: int = index % self.costmap.info.width
        return self.cost_map_tiles_to_pos(x_tile=x_tile, y_tile=y_tile)

    def cost_map_tiles_to_pos(self, x_tile: int, y_tile: int) -> Coord2D:
        x = (x_tile - (self.costmap.info.width / 2)) * self.costmap.info.resolution
        y = (y_tile - (self.costmap.info.height / 2)) * self.costmap.info.resolution
        return Coord2D(x, y)

    def pos_to_costmap_index(self, pos: Coord2D) -> int:
        x_tile, y_tile = self.pos_to_costmap_tile(pos)
        return y_tile * self.costmap.info.width + x_tile

    def pos_to_costmap_tile(self, pos: Coord2D) -> (int, int):
        x_tile = int((pos.x - self.costmap.info.origin.position.x) / self.costmap.info.resolution)
        y_tile = int((pos.y - self.costmap.info.origin.position.y) / self.costmap.info.resolution)
        return x_tile, y_tile

    def get_costmap_coord_status(self, x: int, y: int) -> float:
        index = y * self.costmap.info.width + x
        return self.costmap.data[index]

    def get_number_of_unknown_neighbors(self, index: int):
        width = self.costmap.info.width
        height = self.costmap.info.height
        up_left = index + width - 1
        up = index + width
        up_right = index + width + 1
        left = index - 1
        right = index + 1
        down_left = index - width - 1
        down = index - width
        down_right = index - width + 1
        neighbors = [up_left, up, up_right, left, right, down_left, down, down_right]
        neighbors = list(filter(lambda tile_index: 0 <= tile_index <= width * height - 1, neighbors))

        unknown_neighbors = 0
        for neighbor_index in neighbors:
            # If neighbor is unknown, return index of that neighbor
            if self.costmap.data[neighbor_index] == -1:
                unknown_neighbors += 1

        return unknown_neighbors

    def has_at_least_n_unknown_neighbors(self, index: int, n: int):
        return self.get_number_of_unknown_neighbors(index) >= n