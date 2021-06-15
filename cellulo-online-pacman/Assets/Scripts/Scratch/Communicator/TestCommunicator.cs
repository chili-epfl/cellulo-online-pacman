using System;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class TestCommunicator : MonoBehaviourPunCallbacks
{
    private TemplateCommunicator _communicator;

    private void Awake()
    {
        PhotonNetwork.AutomaticallySyncScene = true;
        _communicator = GetComponent<TemplateCommunicator>();
    }

    private void Start()
    {
        if (!PhotonNetwork.IsConnected)
            PhotonNetwork.ConnectUsingSettings();

        Debug.Log("Start called!");
    }

    public override void OnConnectedToMaster()
    {
        base.OnConnectedToMaster();

        Debug.Log("Connected to master!");

        PhotonNetwork.JoinOrCreateRoom(
            "debug",
            new RoomOptions()
            {
                IsVisible = true,
                IsOpen = true,
                MaxPlayers = 4
            },
            TypedLobby.Default);
    }

    public override void OnJoinedRoom()
    {
        base.OnJoinedRoom();

        Debug.Log("Joined room succesfully!");
    }

    // private int _frame = 0;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.D))
            _communicator.wow++;

        var inputAxes = (new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical")));
        if (inputAxes.magnitude > 0.0001)
            // _communicator.photonView.RPC("updateInputAxes", RpcTarget.All, inputAxes);
            _communicator.RaiseInputChangedEvent(inputAxes);

        // if (_frame++ >= 60)
        // {
        //     Debug.Log("wow = " + _communicator.wow);
        //     _frame = 0;
        // }
    }
}
