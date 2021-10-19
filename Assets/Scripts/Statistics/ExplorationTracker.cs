using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Dora.MapGeneration;
using Dora.Robot;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Dora.Statistics
{
    public class ExplorationTracker
    {
        // The low-resolution collision map used to create the smoothed map that robots are navigating 
        private SimulationMap<bool> _collisionMap;
        private ExplorationVisualizer _explorationVisualizer;
        
        private SimulationMap<ExplorationCell> _explorationMap;
        private RayTracingMap<ExplorationCell> _rayTracingMap;
        private readonly int _explorationMapWidth;
        private readonly int _explorationMapHeight;

        private const int UpperLeftTriangle = 0;
        private const int LowerRightTriangle = 1;

        public ExplorationTracker(SimulationMap<bool> collisionMap, ExplorationVisualizer explorationVisualizer)
        {
            _collisionMap = collisionMap;
            _explorationVisualizer = explorationVisualizer;
            _explorationMap = collisionMap.FMap(isCellSolid => new ExplorationCell(!isCellSolid));
            _explorationVisualizer.SetMap(_explorationMap, collisionMap.Scale, collisionMap.ScaledOffset);
            _rayTracingMap = new RayTracingMap<ExplorationCell>(_explorationMap);
        }
        
        public void LogicUpdate(SimulationConfiguration config, List<MonaRobot> robots)
        {
            List<int> newlyExploredTriangles = new List<int>();
            float visibilityRange = 15.0f;

            foreach (var robot in robots)
            {
                for (int i = 0; i < 90; i++)
                {
                    var angle = i * 4;
                    if (i * 2 % 45 == 0) continue;
                    
                    _rayTracingMap.Raytrace(robot.transform.position, angle, visibilityRange, (index, cell) =>
                    {
                        if (cell.isExplorable && !cell.IsExplored)
                        {
                            cell.IsExplored = true;
                            newlyExploredTriangles.Add(index);
                        }
                        return cell.isExplorable;
                    });
                }
            }

            _explorationVisualizer.SetExplored(newlyExploredTriangles);
        }
    }
}