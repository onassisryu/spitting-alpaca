using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.SceneManagement;

public class TeamSpit : MonoBehaviourPun
{
    private GameObject player;
    public GameObject stunEffect;
    public GameObject hitEffect;

    private string sceneName;

    private void OnCollisionEnter(Collision collision)
    {

    
        if (sceneName == "WaitingScene" && photonView.IsMine)
        {
            PhotonNetwork.Destroy(gameObject);
            return;
        }

        if (collision.gameObject.CompareTag("DefendPlayer") && photonView.IsMine)
        {
            ContactPoint contact = collision.contacts[0];
            Vector3 collisionPoint = contact.point;
            Quaternion collisionRotation = Quaternion.FromToRotation(Vector3.up, contact.normal);

            PhotonNetwork.Instantiate(hitEffect.name, collisionPoint, collisionRotation);
            
            PhotonView targetPhotonView = collision.gameObject.GetComponent<PhotonView>();
            if (targetPhotonView != null && !targetPhotonView.IsMine)
            {      
                string killerName = photonView.Owner.NickName; // ?¨Îü¨???âÎÑ§??
                string victimName = targetPhotonView.Owner.NickName; // ?ºÌï¥?êÏùò ?âÎÑ§??

                // ??Î°úÍ∑∏Î•?UIManagerÎ•??µÌï¥ ?ùÏÑ±
                // TeamGameManager.instance.data.kill(killerName, victimName);
                TeamGameManagerVer2.instance.dataSyncManager.setKill(killerName, victimName);
                TeamUIManager.Instance.createKillLog(killerName, victimName);

                targetPhotonView.RPC("OnDamage", RpcTarget.All);
            }
        }
        else if (collision.gameObject.CompareTag("Ai"))
        {
            UIManager.Instance.SetStun(true);

            ContactPoint contact = collision.contacts[0];
            Vector3 collisionPoint = contact.point;
            Quaternion collisionRotation = Quaternion.FromToRotation(Vector3.up, contact.normal);

            PhotonNetwork.Instantiate(hitEffect.name, collisionPoint, collisionRotation);

            PhotonView targetPhotonView = collision.gameObject.GetComponent<PhotonView>();
            if (targetPhotonView != null)
            {

                string killerName = photonView.Owner.NickName; // ?¨Îü¨???âÎÑ§??

                // ??Î°úÍ∑∏Î•?UIManagerÎ•??µÌï¥ ?ùÏÑ±
                TeamUIManager.Instance.createKillLog(killerName, "AI");
                TeamGameManagerVer2.instance.dataSyncManager.setKill(killerName, "AI");

                targetPhotonView.RPC("OnDamage", RpcTarget.All);
            }

            photonView.RPC("StunPlayer", RpcTarget.All, player.GetComponent<PhotonView>().ViewID);

            Vector3 stunEffectPosition = player.transform.position;
            stunEffectPosition.y += 1.2f;
            Quaternion stunEffectRotation = Quaternion.Euler(-90f, 0f, 0f);

            PhotonNetwork.Instantiate(stunEffect.name, stunEffectPosition, stunEffectRotation);

            // ƒ≈∏¿” ¡ı∞°
            player.GetComponent<AttackPlayer>().IncreaseCooldown();
        }

        PhotonNetwork.Destroy(gameObject);
    }

    void Start()
    {
        GameObject[] tagObjects = GameObject.FindGameObjectsWithTag("AttackPlayer");

        foreach (GameObject tagObject in tagObjects)
        {
            PhotonView tagPhotonView = tagObject.GetComponent<PhotonView>();

            if (tagPhotonView.IsMine)
            {
                player = tagObject;
            }
        }
    }

    [PunRPC]
    public void StunPlayer(int playerViewId)
    {
        GameObject playerObject = PhotonView.Find(playerViewId).gameObject;
        AttackPlayer playerMovement = playerObject.GetComponent<AttackPlayer>();
        // PlayerMovementTest playerMovement = playerObject.GetComponent<PlayerMovementTest>();
        playerMovement.Stun();
    }

    private void Update()
    {
        sceneName = SceneManager.GetActiveScene().name;
    }
}