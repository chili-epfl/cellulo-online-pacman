using System;
using System.Collections.Generic;
using Navigation.Algorithm;
using UnityEngine;

namespace Navigation
{
    public class GameNode : MonoBehaviour
    {
        public Vector2 Position => gameObject.transform.position;

        public GameNode upNeighbour;
        public GameNode leftNeighbour;
        public GameNode rightNeighbour;
        public GameNode downNeighbour;

        private List<GameNode> _neighbours;

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
