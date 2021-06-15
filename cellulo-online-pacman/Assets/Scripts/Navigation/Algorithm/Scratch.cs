using System;
using System.Collections.Generic;
using Navigation.Algorithm;

public class Scratch
{
    public static void Main()
    {
        // Console.WriteLine("This is C#");

        var node1 = new Node()
        {
            Point = new Point {X = 0, Y = 0},
            Id = new Guid(),
            Name = "node1"
        };

        var node2 = new Node(){
            Point = new Point {X = 1, Y = 1},
            Id = new Guid(),
            Name = "node2"
        };

        var node3 = new Node(){
            Point = new Point {X = 2, Y = 3},
            Id = new Guid(),
            Name = "node3"
        };

        var node4 = new Node(){
            Point = new Point {X = 1, Y = 4},
            Id = new Guid(),
            Name = "node4"
        };

        ConnectNodes(node1, node2);
        ConnectNodes(node2, node3);
        ConnectNodes(node3, node4);
        ConnectNodes(node2, node4);

        var map = new Map {StartNode = node1, Nodes = {node1, node2, node3, node4}, EndNode = node4};

        var search = new SearchEngine(map);
        var shortestPath = search.GetShortestPathDijikstra();

        // Console.WriteLine(map.ToString());
        // Console.WriteLine(shortestPath.ToString());

        foreach (var node in shortestPath)
        {
            Console.WriteLine(node.ToString());
        }
    }

    public static void ConnectNodes(Node a, Node b)
    {
        a.Connections.Add(new Edge(){ConnectedNode = b, Cost = 1, Length = 1});
        b.Connections.Add(new Edge(){ConnectedNode = a, Cost = 1, Length = 1});
    }
}
