using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class TestScript : MonoBehaviour
{
    [SerializeField] private GameObject prefab;

    private void Awake()
    {
        var player = PhotonNetwork.Instantiate(prefab.name, Vector3.zero, Quaternion.identity);
        var a = gameObject.AddComponent<TestScriptPart2>();
        a.initialize(10);

    }

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void FixedUpdate()
    {
        // Debug.Log(Input.GetAxis("Horizontal"));

        // Debug.Log("_input = " + (new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"))) * moveSpeed);
    }
}
