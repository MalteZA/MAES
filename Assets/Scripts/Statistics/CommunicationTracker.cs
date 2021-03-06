// Copyright 2022 MAES
// 
// This file is part of MAES
// 
// MAES is free software: you can redistribute it and/or modify it under
// the terms of the GNU General Public License as published by the
// Free Software Foundation, either version 3 of the License, or (at your option)
// any later version.
// 
// MAES is distributed in the hope that it will be useful, but WITHOUT
// ANY WARRANTY; without even the implied warranty of MERCHANTABILITY
// or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General
// Public License for more details.
// 
// You should have received a copy of the GNU General Public License along
// with MAES. If not, see http://www.gnu.org/licenses/.
// 
// Contributors: Malte Z. Andreasen, Philip I. Holler and Magnus K. Jensen
// 
// Original repository: https://github.com/MalteZA/MAES

using System;
using System.Collections.Generic;
using System.Linq;
using Maes.Robot;
using UnityEngine;
using static Maes.Robot.CommunicationManager;
using static Maes.Statistics.ExplorationTracker;

namespace Maes.Statistics {
    internal class CommunicationTracker {
        public readonly Dictionary<int, SnapShot<bool>> InterconnectionSnapShot = new Dictionary<int, SnapShot<bool>>();
        public readonly Dictionary<int, SnapShot<float>> BiggestClusterPercentageSnapshots = new Dictionary<int, SnapShot<float>>();
        public Dictionary<(int, int), CommunicationInfo> AdjacencyMatrixRef;
        public List<HashSet<int>> CommunicationGroups = null; 
        private RobotConstraints _robotConstraints;

        public CommunicationTracker(RobotConstraints constraints) {
            _robotConstraints = constraints;
        }

        public void CreateSnapshot(int tick) {
            if (tick == 0) return;
            CreateInterconnectedSnapShot(tick);
            CreateClusterSizeSnapShot(tick);
        }

        private void CreateClusterSizeSnapShot(int tick) {
            if (CommunicationGroups != null) {
                // if we have exactly one group, then every agent must be in it!
                if (CommunicationGroups.Count == 1) {
                    BiggestClusterPercentageSnapshots[tick] = new SnapShot<float>(tick, 100.0f);
                }
                else {
                    // Supposed to sort descending
                    CommunicationGroups.Sort((e1, e2) => {
                        return e2.Count.CompareTo(e1.Count);
                    });
                    var totalRobots = CommunicationGroups.Aggregate(0, (sum, e1) => {
                        return sum + e1.Count;
                    });
                    float percentage = (float)CommunicationGroups[0].Count / (float)totalRobots * (float)100;
                    BiggestClusterPercentageSnapshots[tick] = new SnapShot<float>(tick, percentage);
                }
            }
        }

        private void CreateInterconnectedSnapShot(int tick) {
            if (AdjacencyMatrixRef != null && CommunicationGroups != null) {
                if (AreAllAgentsConnected(tick))
                    InterconnectionSnapShot[tick] = new SnapShot<bool>(tick, true);
                else
                    InterconnectionSnapShot[tick] = new SnapShot<bool>(tick, false);
            }
        }

        private bool AreAllAgentsConnected(int tick) {
            if (CommunicationGroups.Count == 1) {
                return true;
            }

            return false;
        }
    }
}