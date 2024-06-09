using UnityEngine;
using System.Collections;
using Photon.Pun; // Photon 사용

public class Hormone : MonoBehaviourPunCallbacks
{
    public float scanRadius = 10.0f;
    public GameObject markerPrefab; // 플레이어 머리 위에 띄울 마커 프리팹

    // 아이템 사용 함수
    public void UseHormone()
    {
        if (!photonView.IsMine)
            return;

        Collider[] hitColliders = Physics.OverlapSphere(transform.position, scanRadius);
        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.CompareTag("Player"))
            {
                // Player 태그를 가진 객체에 마커 생성 (로컬에서만 생성)
                ShowMarkerOverPlayer(hitCollider.gameObject);
            }
        }
    }

    [PunRPC]
    void ShowMarkerOverPlayerRPC(int photonViewID)
    {
        GameObject player = PhotonView.Find(photonViewID).gameObject;
        ShowMarkerOverPlayer(player);
    }

    void ShowMarkerOverPlayer(GameObject player)
    {
        if (photonView.IsMine && player.GetComponent<PhotonView>().Owner != PhotonNetwork.LocalPlayer)
        {
            Vector3 markerPosition = player.transform.position + new Vector3(0, 1, 0); // Y축으로 1 미터 올리기
            GameObject marker = Instantiate(markerPrefab, markerPosition, Quaternion.identity, player.transform);
            StartCoroutine(RemoveMarker(marker));
        }
    }

    IEnumerator RemoveMarker(GameObject marker)
    {
        yield return new WaitForSeconds(5.0f);
        Destroy(marker);
    }
}
