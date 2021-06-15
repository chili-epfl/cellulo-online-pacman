using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class Collectible : MonoBehaviourPun
{
    public int id;

    public void Collect()
    {
        photonView.RPC("CollectibleCollectRPC", RpcTarget.All);
    }

    // ReSharper disable once UnusedMember.Local
    [PunRPC] private void CollectibleCollectRPC()
    {
        gameObject.SetActive(false);
    }

    public void Drop()
    {
        photonView.RPC("CollectibleDropRPC", RpcTarget.All);
    }

    // ReSharper disable once UnusedMember.Local
    [PunRPC] private void CollectibleDropRPC()
    {
        gameObject.SetActive(true);
    }
}
