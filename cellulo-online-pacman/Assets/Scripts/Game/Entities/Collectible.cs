using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class Collectible : MonoBehaviourPun
{

    /// This should be that same number as that of the apple on the real paper Cellulo map. (1-6)
    public int id;


    /// <summary>
    /// Makes the apple disappear for all players connected to the game.
    /// </summary>
    public void Collect()
    {
        photonView.RPC("CollectibleCollectRPC", RpcTarget.All);
    }

    // ReSharper disable once UnusedMember.Local
    [PunRPC] private void CollectibleCollectRPC()
    {
        gameObject.SetActive(false);
    }

    /// <summary>
    /// Makes the apple reappear for all players connected to the game.
    /// </summary>
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
