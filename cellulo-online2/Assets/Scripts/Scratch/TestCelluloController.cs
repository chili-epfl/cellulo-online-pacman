using System;
using System.Collections.Generic;
using System.Linq;
using Navigation;
using UnityEngine;

public class TestCelluloController : MonoBehaviour
{
    private bool _enableConstantInputPooling;

    [SerializeField] private GameObject nodesParent;

    [SerializeField] private GameObject celluloEntity;

    private CelluloEntity _celluloEntityScript;


    [Header("Cellulo Visual Effect")]

    public long effect;
    public CelluloEnums.VisualEffect visualEffect;
    public Color color;
    public long value;


    private void Awake()
    {
        CelluloManager.TryInitialize();
    }

    private void OnApplicationQuit()
    {
        CelluloManager.TryDeinitialize();
    }

    // Start is called before the first frame update
    void Start()
    {
        _celluloEntityScript = celluloEntity.AddComponent<CelluloEntity>();
        _celluloEntityScript.Initialize(!Globals.IsPlatformCelluloCompatible(), 100, Color.white);
    }

    // Update is called once per frame
    void Update()
    {
        var input = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        if (input.magnitude > 0.001)
            _enableConstantInputPooling = true;

        var pos1 = new Vector2(60, -60);
        var pos2 = new Vector2(560, -60);

        if (_enableConstantInputPooling)
            _celluloEntityScript.SetDirectionalInput(input);

        if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            _enableConstantInputPooling = false;
            _celluloEntityScript.SetDirectionalInput(Vector2.zero);
            _celluloEntityScript.LightsDefault();
            // _celluloEntityScript.Cellulo.reset();
        }

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            _enableConstantInputPooling = false;
            _celluloEntityScript.SetGoalPosition(pos1);
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            _enableConstantInputPooling = false;
            _celluloEntityScript.SetGoalPosition(pos2);
        }

        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            _enableConstantInputPooling = false;

            var navNodes = nodesParent.GetComponentsInChildren<GameNode>().ToList();

            // Debug.Log("Nav Nodes : ");
            // foreach (var node in navNodes)
            // {
            //     Debug.Log("\t" + node.Position);
            // }

            // var path = navNodes;
            var path = new List<GameNode> {navNodes[3], navNodes[4], navNodes[5]}
                .Select(x => x.Position).ToList();

            _celluloEntityScript.SetGoalPath(path);
        }

        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            _enableConstantInputPooling = false;

            var navNodes = nodesParent.GetComponentsInChildren<GameNode>().ToList();
            // var navNode = navNodes[6];

            var (startNode, _) = Navigator.FindClosestNode(navNodes, _celluloEntityScript.Position);
            // var startNode = navNodes[3];
            var endNode = navNodes[35];

            var path = Navigator.FindShortestPath(navNodes, startNode, endNode);

            _celluloEntityScript.SetGoalPath(path);

            // Debug.Log("Neighbours : ");
            // navNode.Neighbours.ForEach(x => Debug.Log("\t" + x));
        }

        if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            _enableConstantInputPooling = false;

            var navNodes = nodesParent.GetComponentsInChildren<GameNode>().ToList();

            var (closestNode, dist) = Navigator.FindClosestNode(navNodes, _celluloEntityScript.Position);
            Debug.Log("closestNodeDistTuple : " + closestNode.Position + "\t" + dist);
        }

        if (Input.GetKeyDown(KeyCode.Alpha6))
        {
            _enableConstantInputPooling = false;

            var navNodes = nodesParent.GetComponentsInChildren<GameNode>().ToList();

            // var closestNode = Navigator.FindClosestNode(navNodes, _celluloEntityScript.GetPosition());

            // var path = Na
            //
            // _celluloEntityScript.SetGoalPath(path);
        }

        if (Input.GetKeyDown(KeyCode. Alpha7)) {
            var r = (long) color.r * 255;
            var g = (long) color.g * 255;
            var b = (long) color.b * 255;

            _celluloEntityScript.Cellulo.setVisualEffect(effect, r, g, b, value);
        }

        if (Input.GetKeyDown(KeyCode. Alpha8)) {
            _celluloEntityScript.SetVisualEffect(visualEffect, color, value);
        }

        if (Input.GetKeyDown(KeyCode.L))
        {
            Debug.Log("Position : " + _celluloEntityScript.Position);
            Debug.Log("DistToGoalPos : " + _celluloEntityScript.DistToGoalPos);
        }
    }
}
