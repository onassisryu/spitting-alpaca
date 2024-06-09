using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class MainCameraScript : MonoBehaviourPun
{

    // Start is called before the first frame update
    void Start()
    {
        if (!photonView.IsMine)
        {
            gameObject.SetActive(false);
            return;
        }

        transform.localPosition = new Vector3(0f, 1.2f, -2.49f);
    }

    // Update is called once per frame
    void Update()
    {
        if (!photonView.IsMine)
        {
            gameObject.SetActive(false);
            return;
        }

        transform.localPosition = new Vector3(0f, 1.2f, -2.49f);
    }
}
