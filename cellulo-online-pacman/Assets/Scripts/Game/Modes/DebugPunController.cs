using System;
using Photon.Pun;
using UnityEngine;

/// <summary>
/// Note: Is meant to be attached to same GameObject as GameController
/// </summary>
public class DebugPunController : MonoBehaviourPun
{
    private CelluloEntity player;

    private Vector2 _remoteInputAxes = Vector2.zero;

    private void Start()
    {
        PhotonNetwork.SendRate = 60;
        PhotonNetwork.SerializationRate = 30;

        if (PhotonNetwork.IsMasterClient)
        {
            // var celluloEntityPrefab = gameObject.GetComponent<GameController>().celluloEntityBasePrefab;
            // var playerGameObject = PhotonNetwork.Instantiate(celluloEntityPrefab.name, Vector3.zero, Quaternion.identity);
            // player = playerGameObject.AddComponent<CelluloEntityBase>();
            // player.Initialize(true, 5);
        }
    }

    private int frame = 0;
    private const int sendFrequency = 3;
    private void Update()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            // Take local input if remote input is 0
            if (_remoteInputAxes.magnitude < 0.001)
            {
                _remoteInputAxes = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
            }

            // player.SetInput(_remoteInputAxes);
        }
        else
        {
            if (frame % sendFrequency == 0)
               SendInputAxes(new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical")));
        }

        if (frame++ >= 60)
        {
            frame = 0;
        }
    }

    void SendInputAxes(Vector2 inputAxes)
    {
        this.photonView.RPC("RecieveInputAxes", RpcTarget.All, inputAxes);
    }

    // ReSharper disable once UnusedMember.Local
    [PunRPC] void RecieveInputAxes(Vector2 inputAxes)
    {
        _remoteInputAxes = inputAxes;
        Debug.Log("InputAxes = " + inputAxes);
    }
}
