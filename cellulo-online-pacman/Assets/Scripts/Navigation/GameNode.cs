using System;
using System.Collections.Generic;
using Navigation.Algorithm;
using UnityEngine;

namespace Navigation
{
    /// <summary>
    /// To be attached to GameObjects in Unity and allow designing navigation graphs through Unity.
    /// The neighbours should be set accordingly in Unity. This pretty much represents a
    /// vertex and its adjacency list.
    ///
    /// The navigation algorithm later converts these infos of all GameNodes into a
    /// list of Vertices and Edges. G = (V,E) which is used by the shortest path algorithm.
    /// </summary>
    public class GameNode : MonoBehaviour
    {
        public Vector2 Position => gameObject.transform.position;

        public GameNode upNeighbour;
        public GameNode leftNeighbour;
        public GameNode rightNeighbour;
        public GameNode downNeighbour;

        private List<GameNode> _neighbours;

        /// The adjacency list of this GameNode/(vertex).
        public List<GameNode> Neighbours
        {
            get
            {
                if (_neighbours == null)
                {
                    _neighbours = new List<GameNode> {rightNeighbour, upNeighbour, leftNeighbour, downNeighbour};
                    _neighbours.RemoveAll(x => x == null);
                }

                return _neighbours;
            }
        }

        private Node _algoNode;

        public Node AlgoNode
        {
            get
            {
                if (_algoNode == null)
                {
                    GenerateAlgoNode();
                }

                return _algoNode;
            }
        }

        /// <summary>
        /// Generates the _algoNode which contains this vertexes information
        /// in the structure used by the algorithm.
        /// </summary>
        public void GenerateAlgoNode()
        {
            _algoNode = ConvertGameNode();
        }

        private Node ConvertGameNode()
        {
            var node = new Node
            {
                Original = this,
                Point = new Point {X = Position.x, Y = Position.y},
                Id = Guid.NewGuid(),
                Name = name
            };

            // foreach (var neighbour in gameNode.Neighbours)
            // {
            //     neighbour.Position;
            //     a.Connections.Add(new Edge(){ConnectedNode = b, Cost = 1, Length = 1});
            // }

            return node;
        }
    }
}
