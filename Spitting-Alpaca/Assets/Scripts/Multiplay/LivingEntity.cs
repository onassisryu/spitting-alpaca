using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class LivingEntity : MonoBehaviourPun, IDamageable
{
    public bool dead;

    [PunRPC]
    public void ApplyUpdatedStatus(bool newDead)
    {
        dead = newDead;
    }

    protected virtual void OnEnable()
    {
        dead = false;
    }

    [PunRPC]
    public virtual void OnDamage() 
    {
        if (PhotonNetwork.IsMasterClient)
        {
            photonView.RPC("ApplyUpdatedStatus", RpcTarget.Others, dead);

            photonView.RPC("OnDamage", RpcTarget.Others);
        }

        Die();
        /*photonView.RPC("Die", RpcTarget.MasterClient);*/
    }

    [PunRPC]
    public virtual void Die()
    {
        dead = true;
    }
}
