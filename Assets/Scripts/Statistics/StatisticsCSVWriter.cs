using System.Collections.Generic;
using System.IO;
using System.Text;
using static Maes.Statistics.ExplorationTracker;

namespace Maes.Statistics {
    public class StatisticsCSVWriter {
        private readonly Simulation _simulation;
        private readonly List<SnapShot<float>> _coverSnapShots;
        private readonly List<SnapShot<float>> _exploreSnapshots;
        private readonly Dictionary<int, SnapShot<bool>> _allAgentsConnectedSnapShots;
        public readonly Dictionary<int, SnapShot<float>> BiggestClusterPercentageSnapShots;
        private string path;
        

        public StatisticsCSVWriter(Simulation simulation, string fileNameWithoutExtension) {
            _coverSnapShots = simulation.ExplorationTracker._coverSnapshots;
            _exploreSnapshots = simulation.ExplorationTracker._exploreSnapshots;
            _allAgentsConnectedSnapShots = simulation._communicationManager.CommunicationTracker.InterconnectionSnapShot;
            BiggestClusterPercentageSnapShots = simulation._communicationManager.CommunicationTracker.BiggestClusterPercentageSnapshots;
            
            _simulation = simulation;
            var resultForFileName =
                $"e{(int)_exploreSnapshots[_exploreSnapshots.Count - 1].Value}-c{(int)_coverSnapShots[_coverSnapShots.Count - 1].Value}";
            Directory.CreateDirectory(GlobalSettings.StatisticsOutPutPath);
            path = GlobalSettings.StatisticsOutPutPath + fileNameWithoutExtension + "-" + resultForFileName + ".csv";
        }

        public void CreateCSVFile(string separator) {
            var csv = new StringBuilder();
            csv.AppendLine("Tick,Covered,Explored,Agents Interconnected, Biggest Cluster %");
            for (int i = 0; i < _coverSnapShots.Count; i++) {
                var tick = "" + _coverSnapShots[i].Tick;
                var coverage = "" + _coverSnapShots[i].Value;
                var explore = "" +_exploreSnapshots[i].Value;

                StringBuilder line = new StringBuilder();
                line.Append($"{tick}{separator}{coverage}{separator}{explore}{separator}");
                if (_allAgentsConnectedSnapShots.ContainsKey(i)) {
                    var allAgentsInterconnectedString = _allAgentsConnectedSnapShots[i].Value ? "" + 1 : "" + 0;
                    line.Append($"{allAgentsInterconnectedString}");
                }
                line.Append($"{separator}");
                if (BiggestClusterPercentageSnapShots.ContainsKey(i))
                    line.Append($"{BiggestClusterPercentageSnapShots[i].Value}");

                csv.AppendLine(line.ToString());
            }
            
            File.WriteAllText(path, csv.ToString());
        }
    }
}