using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Portal : MonoBehaviour
{
    // ReadyZone 안에 들어 온 경우 
    private void OnTriggerEnter(Collider other){

        PlayerReady playerReady = other.GetComponent<PlayerReady>();

        Debug.Log("충돌발생");
        Debug.Log(playerReady);
        if (playerReady != null && playerReady.photonView.IsMine) {
            // 준비 상태를 true로 설정합니다.
            playerReady.SetReady(true);
            
            // UI 수정
            WaitingUIManager.instance.UpdateReadyText(true);
        }
       
    }
    // ReadyZone 밖으로 나간 경우
    private void OnTriggerExit(Collider other){
        
        Debug.Log("충돌발생");
        PlayerReady playerReady = other.GetComponent<PlayerReady>();
        if (playerReady != null && playerReady.photonView.IsMine) {
            // 준비 상태를 true로 설정합니다.
            playerReady.SetReady(false);
            // UI 수정
            WaitingUIManager.instance.UpdateReadyText(false);
        }
    }
}
