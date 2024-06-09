using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;


public class LoadSceneButtonScript : MonoBehaviourPunCallbacks
{   

    private string roomUniqueId;
    private string randomRoomName;

    void Start(){

        if(PhotonNetwork.IsMasterClient){

            // 고유값 생성
            roomUniqueId = Guid.NewGuid().ToString();
            randomRoomName = FindObjectOfType<RandomRoomName>().Generate();
            Debug.Log("Generated Room Unique ID: " + roomUniqueId);
            Debug.Log("Generated Room Unique Name: " + randomRoomName);

             // 모든 클라이언트에게 동기화하기 위해 RPC 호출
            photonView.RPC("SyncRoomData", RpcTarget.AllBuffered, roomUniqueId, randomRoomName);
        }
    }

    [PunRPC]
    void SyncRoomData(string uniqueId, string roomName)
    {
        // 받은 값으로 로컬 변수 설정
        roomUniqueId = uniqueId;
        randomRoomName = roomName;
    }

    public void LoadWaitingScene()
    {   
        PhotonNetwork.LeaveRoom();
    }

    // 방을 나간 후 호출되는 콜백
    public override void OnLeftRoom() 
    {       
    }   

    // 마스터 서버에 다시 연결된다면
    public override void OnConnectedToMaster()
    {
        PhotonNetwork.JoinRoom(randomRoomName);
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {   
        RoomOptions options = new RoomOptions{MaxPlayers = 6};
        options.CleanupCacheOnLeave = false;
        options.CustomRoomProperties = new ExitGames.Client.Photon.Hashtable() { { "RoomUniqueId", roomUniqueId } };

        PhotonNetwork.CreateRoom(randomRoomName);
    }

    public override void OnJoinedRoom()
    {   
        PhotonNetwork.LoadLevel("WaitingScene");
    }
}
