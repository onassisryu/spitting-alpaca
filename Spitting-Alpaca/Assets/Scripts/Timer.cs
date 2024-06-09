using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class Timer : MonoBehaviourPunCallbacks, IPunObservable
{
    public float initialTime = 60f;
    public float time;
    public int prevTime;

    void Awake(){
        prevTime = (int) initialTime;
        time = initialTime;
    }
    void Update()
    {       
            // 타이머 갱신을 클라이언트들에게 PUNRPC로 전송
            
            if ((int)time == prevTime) return ;
            photonView.RPC("UpdateTimerRPC", RpcTarget.All, time);            
        
    }


    // RPC 메소드: 클라이언트들에게 타이머 갱신 정보 전송
    [PunRPC]
    private void UpdateTimerRPC(float newTime)
    {
        prevTime = (int)newTime;
    }

    public bool check(){
        if (!PhotonNetwork.IsMasterClient) return false ;
        
        if(PhotonNetwork.CurrentRoom.PlayerCount == 1) {
            time = initialTime;
            prevTime = (int) initialTime;
            return false;
        }

        return true;
    }
    // 동기화 처리
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            // 마스터 클라이언트에서만 쓰기
            stream.SendNext(time);
        }
        else
        {
            // 나머지 클라이언트들에서 읽기
            time = (float)stream.ReceiveNext();
        }
    }
}
