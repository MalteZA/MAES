﻿using System;
using Dora.Robot.ExplorationAlgorithm;
using UnityEngine;

namespace Dora.Robot
{
    public class Robot: MonoBehaviour, ISimulationUnit
    {
        
        public Transform leftWheelTransform;
        public Transform rightWheelTransform;

        // The controller that provides an interface for moving the robot
        public IRobotController movementController { get; private set; }

        // The algorithm that controls the logic of the robot
        public IExplorationAlgorithm ExplorationAlgorithm { get; set; }

        private void Start()
        {
            var rigidBody = GetComponent<Rigidbody2D>();
            movementController = new Robot2DController(rigidBody, transform, leftWheelTransform, rightWheelTransform);
        }

        public void LogicUpdate(SimulationConfiguration config)
        {
            ExplorationAlgorithm.UpdateLogic(config);
        }

        public void PhysicsUpdate(SimulationConfiguration config)
        {
            movementController.UpdateMotorPhysics(config);
        }

        public object SaveState()
        {
            throw new System.NotImplementedException();
        }

        public void RestoreState(object stateInfo)
        {
            throw new System.NotImplementedException();
        }

    }
}