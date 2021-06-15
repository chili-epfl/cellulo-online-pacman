using System;
using UnityEngine;

public class TestScriptPart2: MonoBehaviour
{
    // [SerializeField] private GameObject prefab;

    //---------------------------------
    // Initializable fields
    private bool _initialized;

    private float _moveSpeed;

    //---------------------------------

    public void initialize(float moveSpeed)
    {
        if (!_initialized)
            _initialized = true;
        else
            throw new InvalidOperationException("Can only call initialize() once!");

        _moveSpeed = moveSpeed;
    }

    void Start()
    {
        if (!_initialized)
            throw new InvalidOperationException("Must call initialize() upon object creation");

        Debug.Log("moveSpeed = " + _moveSpeed);
    }
}
