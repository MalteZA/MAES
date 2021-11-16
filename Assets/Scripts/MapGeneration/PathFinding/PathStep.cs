using System.Collections.Generic;
using UnityEngine;

namespace Dora.MapGeneration.PathFinding {
    // Represents a single line in a path. Used for creating tile reservations when traversing the path
    public class PathStep {
        public readonly Vector2Int Start;
        public readonly Vector2Int End;
        public readonly List<Vector2Int> CrossedTiles;

        public PathStep(Vector2Int start, Vector2Int end, List<Vector2Int> crossedTiles) {
            Start = start;
            End = end;
            CrossedTiles = crossedTiles;
        }
    }
}