using System.Collections;
using Photon.Pun;
using UnityEngine;

public class AttackerFence : MonoBehaviourPun
{
    private Vector3 initialPosition;
    private Vector3 targetPosition;

    void Start()
    {
        // 초기 위치를 저장합니다.
        initialPosition = transform.position;

        // 목표 위치를 초기 위치에서 아래로 일정 거리 설정
        targetPosition = initialPosition + new Vector3(0, -20, 0);

        // 5초 후에 울타리를 내리는 코루틴 시작
        if (photonView.IsMine)
        {
            StartCoroutine(LowerFenceAfterDelay(10f));
        }
    }

    IEnumerator LowerFenceAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        photonView.RPC("LowerFence", RpcTarget.AllBuffered);
    }

    [PunRPC]
    void LowerFence()
    {
        StartCoroutine(LowerFenceCoroutine());
    }

    IEnumerator LowerFenceCoroutine()
    {
        float elapsedTime = 0f;
        float duration = 2f; // 내려가는 시간

        while (elapsedTime < duration)
        {
            transform.position = Vector3.Lerp(initialPosition, targetPosition, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        transform.position = targetPosition;
    }
}
