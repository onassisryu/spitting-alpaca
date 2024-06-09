using System.Collections;
using System.Collections.Generic;
using ExitGames.Client.Photon.StructWrapping;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StaticCameraSetup : MonoBehaviour
{
    public GameObject mainScene2Manager;
    public Transform target; // 카메라가 바라볼 대상
    public Vector3 cameraOffset = new Vector3(2.5f, 0.8f, 0.5f); // 카메라 위치 오프셋 (오른쪽, 위, 앞으로)
    public GameObject mainCanvas; // MainCanvas 오브젝트
    private CanvasGroup mainCanvasGroup;
    public TextMeshProUGUI nickNameText; // 닉네임을 표시할 TextMeshProUGUI

    void Start()
    {
        PhotonNetwork.Disconnect();
        mainScene2Manager.GetComponent<MainScene2Manager>().Connect();
        nickNameText.text = PhotonNetwork.NickName;

        // 카메라 위치 계산 및 설정
        Camera.main.transform.position = CalculateCameraPosition();

        // 카메라 방향 설정
        Camera.main.transform.LookAt(target.position);

        // MainCanvas 활성화
        mainCanvasGroup = mainCanvas.GetComponent<CanvasGroup>();
        ActivateMainCanvas();

        // target의 움직임을 멈춤
        StopTargetMovement();
    }

    Vector3 CalculateCameraPosition()
    {
        Vector3 rightOffset = target.right * cameraOffset.x; // 대상의 오른쪽으로
        Vector3 upOffset = Vector3.up * cameraOffset.y; // 위로
        Vector3 forwardOffset = target.forward * cameraOffset.z; // 앞으로
        return target.position + rightOffset + upOffset - forwardOffset; // forwardOffset을 빼면 카메라가 대상의 뒤쪽으로 갑니다.
    }

    void ActivateMainCanvas()
    {
        if (mainCanvasGroup != null)
        {
            mainCanvasGroup.alpha = 1;
            mainCanvasGroup.interactable = true;
            mainCanvasGroup.blocksRaycasts = true;
        }
    }

    void StopTargetMovement()
    {
        AiScript aiScript = target.GetComponent<AiScript>();
        if (aiScript != null)
        {
            aiScript.isLive = false;
        }
    }
}
