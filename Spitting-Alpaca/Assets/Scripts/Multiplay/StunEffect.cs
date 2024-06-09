using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class StunEffect : MonoBehaviourPun
{
    // Start is called before the first frame update
    void Start()
    {
        Invoke("DestroyObject", 4f);
    }

    // Update is called once per frame
    void DestroyObject()
    {
        PhotonNetwork.Destroy(gameObject);
    }
}
