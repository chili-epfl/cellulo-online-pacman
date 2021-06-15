using System;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

/// This class contains 3 different methods of sending/syncing data using PhotonPUN.
/// Note: (According to my own design) Anything other then things that can be
/// syncronized through configuring a PhotonView in Unity inspector should be
/// syncronized through a single instance of this class.
public class TemplateCommunicator : MonoBehaviourPunCallbacks, IPunObservable, IOnEventCallback
{
    private Vector2 _inputAxes;



    public int wow = 0;

    private void Start()
    {
        // Set the sending rate of Photon Serialize view
        // PhotonNetwork.SendRate = 60;
        // PhotonNetwork.SerializationRate = 30;
    }

    //========================================================================
    // OnPhotonSerializeView : (Can be used to sync vars from the master to
    // other clients. Use events to send data in other direction(s)).

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            Debug.Log("Sending" + wow);
            stream.SendNext(wow);
        }
        if (stream.IsReading)
        {
            wow = (int) stream.ReceiveNext();
            Debug.Log("Recieving" + wow);
        }
    }

    //========================================================================
    // RPCs : Simple

    void dispatchInputAxes(Vector2 inputAxes)
    {
        // 2nd parameter determines to whom you're "sending" this RPC.
        photonView.RPC("updateInputAxes", RpcTarget.All, inputAxes);
    }

    /// Call this like this : _communicator.photonView.RPC("updateInputAxes", RpcTarget.All, inputAxes);
    // ReSharper disable once UnusedMember.Local
    [PunRPC] void updateInputAxes(Vector2 inputAxes)
    {
        _inputAxes = inputAxes;
        Debug.Log("InputAxes = " + inputAxes);
    }

    //========================================================================
    // Events : More versatile!

    //------------------------------------------------------------------------
    // Required setup for Events to work
    public override void OnEnable()
    {
        base.OnEnable();
        PhotonNetwork.AddCallbackTarget(this);
    }

    public override void OnDisable()
    {
        base.OnDisable();
        PhotonNetwork.RemoveCallbackTarget(this);
    }

    //------------------------------------------------------------------------
    // Event dispatchers

    // Our custom event codes can be anywhere within [0-199]
    public const byte InputChangedEventCode = 1;
    public void RaiseInputChangedEvent(Vector2 inputAxes)
    {
        Debug.Log("Sending Event!");

        object[] content = new object[]
        {
            inputAxes
        };

        PhotonNetwork.RaiseEvent(
            InputChangedEventCode,
            content,
            new RaiseEventOptions() {Receivers = ReceiverGroup.MasterClient},
            SendOptions.SendReliable    // Change this as required
        );
    }

    //------------------------------------------------------------------------
    // Event receivers
    public void OnEvent(EventData photonEvent)
    {
        byte eventCode = photonEvent.Code;
        if (eventCode == InputChangedEventCode)
        {
            object[] content = (object[]) photonEvent.CustomData;
            _inputAxes = (Vector2) content[0];

            Debug.Log("Received Event : " + photonEvent.Code + " Axes = " + _inputAxes.ToString());
        }
        else
        {
            Debug.Log("Received Event : " + photonEvent.Code);
        }
    }
}
