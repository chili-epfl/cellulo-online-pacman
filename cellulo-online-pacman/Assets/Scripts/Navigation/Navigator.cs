using System;
using System.Collections.Generic;
using System.Linq;
using Navigation.Algorithm;
using UnityEngine;

namespace Navigation
{
    /// <summary>
    /// Class acts as the interface for all navigation calculations of the Game.
    /// It interfaces with the general C# implementation of the shortest path algorithm.
    /// </summary>
    public static class Navigator
    {
        /// <summary>
        /// Finds the Closest GameNode to a given position.
        /// </summary>
        /// <param name="gameNodes">List of all GameNodes on active map.</param>
        /// <param name="pos"></param>
        /// <returns>The closest game mode and the distance to it.</returns>
        public static Tuple<GameNode, float> FindClosestNode(List<GameNode> gameNodes, Vector2 pos)
        {
            List<Tuple<GameNode, float>> gameNodeDistTuple =
                gameNodes.Select(x => new Tuple<GameNode, float>(x, (pos - x.Position).magnitude)).ToList();

            var closestNodeDistTuple = gameNodeDistTuple.OrderBy(x => x.Item2).ToList()[0];

            // Debug.Log("closestNodeDistTuple : " + closestNodeDistTuple.Item1 + "\t" + closestNodeDistTuple.Item2);

            return closestNodeDistTuple;
        }

        /// <summary>
        /// Finds the shortest path between 2 game nodes.
        /// </summary>
        /// <param name="gameNodes">List of all GameNodes on active map.</param>
        /// <param name="start">Path start.</param>
        /// <param name="end">Path end.</param>
        /// <returns>The shortest path</returns>
        public static List<Vector2> FindShortestPath(List<GameNode> gameNodes, GameNode start, GameNode end)
        {
            var map = GenerateMap(gameNodes, start, end);
            var search = new SearchEngine(map);

            var shortestPath = search.GetShortestPathDijikstra();

            return shortestPath.Select(x => x.Original.Position).ToList();
        }

        /// <summary>
        /// Calculates the total length of a path
        /// </summary>
        /// <param name="path">Path as an ordered list of positions.</param>
        /// <returns></returns>
        public static float PathLength(List<Vector2> path)
        {
            var length = 0f;
            for (int i = 0; i < path.Count - 1; i++)
            {
                length += (path[i] - path[i + 1]).magnitude;
            }

            return length;
        }

        private static Map GenerateMap(List<GameNode> gameNodes, GameNode start, GameNode end)
        {
            // Regenerate AlgoNodes since these get modified by the algorithm
            // and contain information such as "visited" by graph traversal and
            // etc.
            gameNodes.ForEach(x => x.GenerateAlgoNode());

            var nodes = gameNodes.Select(x => x.AlgoNode).ToList();

            foreach (var gameNode in gameNodes)
            {
                foreach (var neighbour in gameNode.Neighbours)
                {
                    ConnectNodes(gameNode.AlgoNode, neighbour.AlgoNode, (gameNode.Position - neighbour.Position).magnitude);
                }
            }

            return new Map {StartNode = start.AlgoNode, Nodes = nodes, EndNode = end.AlgoNode};
        }

        private static void ConnectNodes(Node a, Node b, float dist)
        {
            a.Connections.Add(new Edge(){ConnectedNode = b, Cost = dist, Length = dist});
        }
    }
}
