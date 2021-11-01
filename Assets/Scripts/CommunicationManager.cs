using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using Dora.MapGeneration;
using Dora.Robot;
using UnityEngine;
using static Dora.MapGeneration.EnvironmentTaggingMap;

namespace Dora {
    // Messages sent through this class will be subject to communication range and line of sight.
    // Communication is non-instantaneous. Messages will be received by other robots after one logic tick. 
    public class CommunicationManager : ISimulationUnit {
        private RobotConstraints _robotConstraints;
        private DebuggingVisualizer _visualizer;

        // Messages that will sent during the next logic update
        private List<Message> _queuedMessages = new List<Message>();
        
        // Messages that were sent last tick and can now be read 
        private List<Message> _readableMessages = new List<Message>();

        private readonly RayTracingMap<bool> _rayTracingMap;
        private List<MonaRobot> _robots;

        // Map for storing and retrieving all tags deposited by robots
        private readonly EnvironmentTaggingMap _environmentTaggingMap;
        
        private int _localTickCounter = 0;
        
        private Dictionary<(int, int), CommunicationInfo> _adjacencyMatrix = null;

        private readonly struct Message {
            public readonly object Contents;
            public readonly MonaRobot Sender;
            public readonly Vector2 broadcastCenter;

            public Message(object contents, MonaRobot sender, Vector2 broadcastCenter) {
                Contents = contents;
                Sender = sender;
                this.broadcastCenter = broadcastCenter;
            }
        }

        private readonly struct CommunicationInfo {
            public readonly float Distance;
            public readonly float Angle;
            public readonly int WallsCellsPassedThrough;

            public CommunicationInfo(float distance, float angle, int wallsCellsPassedThrough) {
                Distance = distance;
                Angle = angle;
                WallsCellsPassedThrough = wallsCellsPassedThrough;
            }
        }

        public readonly struct SensedObject<T> {
            public readonly float Distance;
            public readonly float Angle;
            public readonly T item;

            public SensedObject(float distance, float angle, T t) {
                Distance = distance;
                Angle = angle;
                this.item = t;
            }
        }

        public CommunicationManager(SimulationMap<bool> collisionMap, RobotConstraints robotConstraints,
            DebuggingVisualizer visualizer) {
            _robotConstraints = robotConstraints;
            _visualizer = visualizer;
            _rayTracingMap = new RayTracingMap<bool>(collisionMap);
            _environmentTaggingMap = new EnvironmentTaggingMap(collisionMap);
        }

        // Adds a message to the broadcast queue
        public void BroadcastMessage(MonaRobot sender, in object messageContents) {
            _queuedMessages.Add(new Message(messageContents, sender, sender.transform.position));
        }

        // Returns a list of messages sent by other robots
        public List<object> ReadMessages(MonaRobot receiver) {
            this.PopulateAdjacencyMatrix();
            List<object> messages = new List<object>();
            Vector2 receiverPosition = receiver.transform.position;
            foreach (var message in _readableMessages) {
                // The robot will not receive its own messages
                if (message.Sender.id == receiver.id) continue;

                var communicationTrace = _adjacencyMatrix[(message.Sender.id, receiver.id)];
                if (communicationTrace.Distance <= _robotConstraints.BroadcastRange) {
                    if(_robotConstraints.BroadcastBlockedByWalls && communicationTrace.WallsCellsPassedThrough != 0)
                        continue;
                    
                    messages.Add(message.Contents);
                    if (GlobalSettings.DrawCommunication)
                        _visualizer.AddCommunicationTrail(message.Sender, receiver);
                }
            }

            return messages;
        }

        private CommunicationInfo RayTraceCommunication(Vector2 pos1, Vector2 pos2) {
            var distance = Vector2.Distance(pos1, pos2);
            var angle = Vector2.Angle(Vector2.right, pos2 - pos1);
            // If p1.y > p2.y then angle should be 360 minus the angle difference between the vectors
            // to make the angle relative to the x axis. (Moving from oregon along the x axis is 0 degrees in out system)
            if (pos1.y > pos2.y) angle = 360f - angle;

            if (distance > _robotConstraints.MaxRayCastRange)
                return new CommunicationInfo(distance, angle, -1);
            
            var wallsTravelledThrough = 0;
            _rayTracingMap.Raytrace(pos1, angle, distance,
                (_, cellIsSolid) => {
                    if (cellIsSolid) {
                        wallsTravelledThrough++;
                    }
                    return true;
                });
            return new CommunicationInfo(distance, angle, wallsTravelledThrough);
        }

        public void LogicUpdate() {
            // Move messages sent last tick into readable messages
            _readableMessages.Clear();
            _readableMessages.AddRange(_queuedMessages);
            _queuedMessages.Clear();
            _localTickCounter++;
            if (_robotConstraints.ShouldAutomaticallyUpdateSlam 
                && _localTickCounter % _robotConstraints.SlamUpdateIntervalInTicks == 0) {
                SynchronizeSlamMaps();
            }

            _adjacencyMatrix = null;
        }

        private void SynchronizeSlamMaps() {
            var slamGroups = GetCommunicationGroups();

            foreach (var group in slamGroups) {
                var slamMaps = group
                    .Select(id => _robots.Find(r => r.id == id))
                    .Select(r => r.Controller.SlamMap)
                    .ToList();
                
                SlamMap.Combine(slamMaps);
            }
        }

        public void PhysicsUpdate() {
            // No physics update needed
        }

        public object SaveState() {
            throw new System.NotImplementedException();
        }

        public void RestoreState(object stateInfo) {
            throw new System.NotImplementedException();
        }

        private void PopulateAdjacencyMatrix() {
            if (_adjacencyMatrix != null)
                return;
            
            _adjacencyMatrix = new Dictionary<(int, int), CommunicationInfo>();
            
            foreach (var r1 in _robots) {
                foreach (var r2 in _robots) {
                    if (r1.id != r2.id) {
                        var r1Position = r1.transform.position;
                        var r2Position = r2.transform.position;
                        var r1Vector2 = new Vector2(r1Position.x, r1Position.y);
                        var r2Vector2 = new Vector2(r2Position.x, r2Position.y);
                        _adjacencyMatrix[(r1.id, r2.id)] = RayTraceCommunication(r1Vector2, r2Vector2);
                    }
                }
            }
        }

        public List<HashSet<int>> GetCommunicationGroups() {
            PopulateAdjacencyMatrix();

            List<HashSet<int>> groups = new List<HashSet<int>>();
            foreach (var r1 in _robots) {
                if(!groups.Exists(g => g.Contains(r1.id))); {
                    groups.Add(GetCommunicationGroup(r1.id));
                }
            }

            return groups;
        }

        private HashSet<int> GetCommunicationGroup(int robotId) {
            var keys = new Queue<int>();
            keys.Enqueue(robotId);
            var resultSet = new HashSet<int>(){robotId};

            while (keys.Count > 0) {
                var currentKey = keys.Dequeue();

                var inRange = _adjacencyMatrix
                    .Where((kv) => kv.Key.Item1 == currentKey && kv.Value.Distance < _robotConstraints.BroadcastRange && kv.Value.WallsCellsPassedThrough == 0)
                    .Select((e) => e.Key.Item2);

                foreach (var rInRange in inRange) {
                    if (!resultSet.Contains(rInRange)) {
                        keys.Enqueue(rInRange);
                        resultSet.Add(rInRange);
                    }
                }
            }

            return resultSet;
        }

        public void DepositTag(MonaRobot robot, object data) {
            if (GlobalSettings.ShowEnvironmentTags)
                _visualizer.AddEnvironmentTag(robot.transform.position);
            
            _environmentTaggingMap.AddTag(robot.transform.position, data);
        }

        public List<EnvironmentTag> ReadNearbyTags(MonaRobot robot) {
            var tags = _environmentTaggingMap.GetTagsNear(robot.transform.position,
                _robotConstraints.EnvironmentTagReadRange);

            // Debugging visualize tags that are readable by robots
            if (GlobalSettings.ShowEnvironmentTags){
                foreach (var tag in tags) {
                    _visualizer.AddReadableTag(tag.WorldPosition);
                }
            }

            return tags;
        }

        public List<SensedObject<int>> SenseNearbyRobots(int id) {
            PopulateAdjacencyMatrix();
            var sensedObjects = new List<SensedObject<int>>();

            foreach (var robot in _robots) {
                if(robot.id == id) continue;

                
                var comInfo = _adjacencyMatrix[(id, robot.id)];
                if(comInfo.Distance > _robotConstraints.SenseNearbyRobotRange 
                   || (comInfo.WallsCellsPassedThrough > 0 && _robotConstraints.SenseNearbyRobotBlockedByWalls))
                    continue;

                sensedObjects.Add(new SensedObject<int>(comInfo.Distance, comInfo.Angle, robot.id));
                
            }
            
            return sensedObjects;
        } 

        public void SetRobotReferences(List<MonaRobot> robots) {
            this._robots = robots;
        }

    }
}