using System.Collections.Generic;
using Dora.ExplorationAlgorithm;
using Dora.ExplorationAlgorithm.TheNextFrontier;
using Dora.MapGeneration;

namespace Dora {
    public class ScenarioGenerator {
        public static Queue<SimulationScenario> GenerateBallisticScenarios() {
            Queue<SimulationScenario> scenarios = new Queue<SimulationScenario>();

            for (int i = 0; i < 5; i++) {
                int randomSeed = i + 4 + 1;
                int minute = 60;
                var mapConfig = new CaveMapConfig(
                    60,
                    60,
                    randomSeed,
                    4,
                    2,
                    48,
                    10,
                    1,
                    1,
                    1f);

                var officeConfig = new OfficeMapConfig(
                    60,
                    60,
                    randomSeed,
                    58,
                    4,
                    5,
                    2,
                    1,
                    75,
                    1,
                    1f);


                var robotConstraints = new RobotConstraints(
                    broadcastRange: 15.0f,
                    broadcastBlockedByWalls: true,
                    senseNearbyRobotRange: 5f,
                    senseNearbyRobotBlockedByWalls: true,
                    automaticallyUpdateSlam: true,
                    slamUpdateIntervalInTicks: 10,
                    slamSynchronizeIntervalInTicks: 10,
                    slamPositionInaccuracy: 0.5f,
                    distributeSlam: false,
                    environmentTagReadRange: 4.0f
                );

                if (true) {
                    scenarios.Enqueue(new SimulationScenario(
                        seed: randomSeed,
                        hasFinishedSim: (simulation) => simulation.SimulateTimeSeconds >= 20 * minute,
                        mapSpawner: (mapGenerator) => mapGenerator.GenerateOfficeMap(officeConfig, 2.0f),
                        robotSpawner: (map, robotSpawner) => robotSpawner.SpawnAtHallWayEnds(
                            map,
                            randomSeed,
                            40,
                            0.6f,
                            (seed) => new RandomExplorationAlgorithm(seed)),
                        robotConstraints: robotConstraints
                    ));
                }
                else {
                    scenarios.Enqueue(new SimulationScenario(
                        seed: randomSeed,
                        hasFinishedSim: (simulation) => simulation.SimulateTimeSeconds >= 20 * minute,
                        mapSpawner: (mapGenerator) => mapGenerator.GenerateCaveMap(mapConfig, 2.0f),
                        robotSpawner: (map, robotSpawner) => robotSpawner.SpawnRobotsTogether(
                            map,
                            randomSeed,
                            20,
                            0.6f,
                            new Coord(20, 20),
                            (seed) => new RandomExplorationAlgorithm(seed)),
                        robotConstraints: robotConstraints
                    ));
                }
            }

            return scenarios;
        }

        public static Queue<SimulationScenario> GenerateTnfScenarios() {
            var scenarios = new Queue<SimulationScenario>();

            int randomSeed = 4 + 1;
            int minute = 60;

            var mapConfig = new CaveMapConfig(
                60,
                60,
                randomSeed,
                4,
                2,
                48,
                10,
                1,
                1,
                1f);

            var officeConfig = new OfficeMapConfig(
                60,
                60,
                randomSeed,
                58,
                4,
                5,
                2,
                1,
                75,
                1,
                1f);

            var robotConstraints = new RobotConstraints(
                broadcastRange: 15.0f,
                broadcastBlockedByWalls: true,
                senseNearbyRobotRange: 5f,
                senseNearbyRobotBlockedByWalls: true,
                automaticallyUpdateSlam: true,
                slamUpdateIntervalInTicks: 10,
                slamSynchronizeIntervalInTicks: 10,
                slamPositionInaccuracy: 0.5f,
                distributeSlam: false,
                environmentTagReadRange: 4.0f
            );

            scenarios.Enqueue(new SimulationScenario(
                seed: randomSeed, 
                hasFinishedSim: simulation => simulation.SimulateTimeSeconds >= 20 * minute,
                mapSpawner: generator => generator.GenerateOfficeMap(officeConfig, 2.0f),
                robotSpawner: (map, robotSpawner) => robotSpawner.SpawnRobotsTogether(
                    map,
                    randomSeed,
                    1,
                    0.6f,
                    new Coord(25,10),
                    (seed) => new TnfExplorationAlgorithm()),
                robotConstraints: robotConstraints
            ));

            return scenarios;
        }
    }
}