using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.SceneManagement;

public class Spit : MonoBehaviourPun
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

        if (collision.gameObject.CompareTag("Player") && photonView.IsMine)
        {
            ContactPoint contact = collision.contacts[0];
            Vector3 collisionPoint = contact.point;
            Quaternion collisionRotation = Quaternion.FromToRotation(Vector3.up, contact.normal);

            PhotonNetwork.Instantiate(hitEffect.name, collisionPoint, collisionRotation);
            
            PhotonView targetPhotonView = collision.gameObject.GetComponent<PhotonView>();
            if (targetPhotonView != null && !targetPhotonView.IsMine)
            {      
                string killerName = photonView.Owner.NickName; // 킬러의 닉네임
                string victimName = targetPhotonView.Owner.NickName; // 피해자의 닉네임

                // 킬 로그를 UIManager를 통해 생성
                    UIManager.Instance.createKillLog(killerName, victimName);
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

                string killerName = photonView.Owner.NickName; // 킬러의 닉네임

                // 킬 로그를 UIManager를 통해 생성
                
                   
                UIManager.Instance.createKillLog(killerName, "AI");

                targetPhotonView.RPC("OnDamage", RpcTarget.All);
                
            }

            photonView.RPC("StunPlayer", RpcTarget.All, player.GetComponent<PhotonView>().ViewID);

            Vector3 stunEffectPosition = player.transform.position;
            stunEffectPosition.y += 1.2f;
            Quaternion stunEffectRotation = Quaternion.Euler(-90f, 0f, 0f);

            PhotonNetwork.Instantiate(stunEffect.name, stunEffectPosition, stunEffectRotation);
        }

        PhotonNetwork.Destroy(gameObject);
    }

    void Start()
    {
        GameObject[] tagObjects = GameObject.FindGameObjectsWithTag("Player");

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
        PlayerMovementTest playerMovement = playerObject.GetComponent<PlayerMovementTest>();
        playerMovement.Stun();
    }

    private void Update()
    {
        sceneName = SceneManager.GetActiveScene().name;
    }
}
