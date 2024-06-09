using System.Collections;
using System.Collections.Generic;
using ExitGames.Client.Photon.StructWrapping;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class CameraZoomToAlpaca : MonoBehaviour
{
    public GameObject networkManager;
    public Transform target; // 줌인 대상
    public GameObject canvasObject; // Canvas 오브젝트
    public TextMeshProUGUI nickNameText; // NickNameTag Text 컴포넌트를 위한 public 변수
    public GameObject mainCanvas; // MainCanvas 오브젝트를 인스펙터에서 할당
    public float zoomSpeed = 6f; // 줌인 속도
    public float rotationSpeed = 3f; // 회전 속도
    public float fadeSpeed = 0.5f; // 페이드 아웃 속도
    private Vector3 originalPosition; // 원래 카메라 위치
    private Quaternion originalRotation; // 원래 카메라 회전
    private bool isZooming = false; // 줌인 상태 여부
    private Camera cam; // 카메라 컴포넌트
    private CanvasGroup canvasGroup; // CanvasGroup
    private CanvasGroup mainCanvasGroup; // mainCanvas의 CanvasGroup
    private Vector3 startPosition; // 첫 위치를 저장할 변수
    private Vector3 lastPosition; // 마지막 위치를 저장할 변수
    private bool isMoving = false; // 움직임 상태를 저장할 변수



    void Start()
    {
        cam = Camera.main; // 메인 카메라 할당
        originalPosition = transform.position; // 초기 카메라 위치 저장
        originalRotation = transform.rotation; // 초기 카메라 회전 저장
        canvasGroup = canvasObject.GetComponent<CanvasGroup>();
        mainCanvasGroup = mainCanvas.GetComponent<CanvasGroup>(); // mainCanvas의 CanvasGroup을 가져옴
        startPosition = transform.position;
        lastPosition = transform.position;
        mainCanvasGroup.alpha = 0; // 초기에 mainCanvas를 완전히 투명하게 설정
        mainCanvasGroup.interactable = false; // 상호작용 불가능하게 설정
        mainCanvasGroup.blocksRaycasts = false; // 레이캐스트 차단
    }

    void Update()
    {
        // 카메라 범위 제한
        float x = Mathf.Clamp(transform.position.x, -21, 21);
        float y = transform.position.y;
        float z = Mathf.Clamp(transform.position.z, -17, 17);
        transform.position = new Vector3(x, y, z);
        
        if (Input.anyKeyDown && !isZooming && string.IsNullOrEmpty(nickNameText.text)) 
        {
            isZooming = true;
            PhotonNetwork.Disconnect();
            networkManager.GetComponent<NetworkManager>().Connect();
            nickNameText.text = PhotonNetwork.NickName;
        }

        if (isZooming)
        {
            // 대상의 오른쪽으로 카메라를 이동하여, 정면에서 보는 것과 다른 시각을 제공
            Vector3 rightOffset = target.right * 2.5f; // 대상의 오른쪽으로 3 유닛
            Vector3 finalPosition = target.position + rightOffset + new Vector3(0, 0.8f, 0.5f); // 위로 0.8 유닛 올라간 위치

            transform.position = Vector3.MoveTowards(transform.position, finalPosition, zoomSpeed * Time.deltaTime);
            transform.rotation = Quaternion.LookRotation(target.position - transform.position + new Vector3(0, 0.3f, 0));  // 카메라 각도 조금 내림

            // Canvas 페이드 아웃
            if (canvasGroup != null && canvasGroup.alpha > 0)
            {
                canvasGroup.alpha -= fadeSpeed * Time.deltaTime;
                if (canvasGroup.alpha <= 0)
                {
                    canvasObject.SetActive(false); // 투명도가 0이 되면 Canvas 비활성화
                }
            }

            if (Vector3.Distance(transform.position, finalPosition) < 0.5f)
            {
                isZooming = false;
                target.GetComponent<AiScript>().isLive = false;
            }
        }

        // 첫 위치와 다르면 실행
        if (transform.position != startPosition)
        {
            // 현재 위치와 마지막 위치가 다르면 움직이고 있는 것으로 판단
            if (transform.position != lastPosition)
            {
                isMoving = true;
                mainCanvasGroup.alpha = 0;
                mainCanvasGroup.interactable = false;
                mainCanvasGroup.blocksRaycasts = false;
            }
            else
            {
                isMoving = false;
                mainCanvasGroup.alpha = 1;
                mainCanvasGroup.interactable = true;
                mainCanvasGroup.blocksRaycasts = true;
            }
        }

        lastPosition = transform.position; // 현재 위치를 마지막 위치로 업데이트
    }
}

