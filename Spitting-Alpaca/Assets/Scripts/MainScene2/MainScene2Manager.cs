using System.Collections;
using System.Collections.Generic;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;


public class MainScene2Manager : MonoBehaviourPunCallbacks
{

  private string gameVersion = "1";
  private string randomRoomName = "안녕 알파카";
  public Button quickEnterButton;

  public static MainScene2Manager instance
  {
    get
    {
      // 싱글톤 X
      if (m_instance == null)
      {

        // 씬에서 WaitingManager를 찾아 할당한다
        m_instance = FindObjectOfType<MainScene2Manager>();
      }
      // 싱글톤 오브젝트를 반환
      return m_instance;
    }
  }

  private static MainScene2Manager m_instance;

  private void Awake()
  {
    // 씬에 싱글톤 오브젝트가 된 다른 GameManager 오브젝트가 있다면
    if (instance != this)
    {
      // 자신을 파괴
      Destroy(gameObject);
    }

  }

  public string NetworkClientState()
  {
    return PhotonNetwork.NetworkClientState.ToString();
  }

  public void Connect()
  {
    PhotonNetwork.GameVersion = gameVersion;
    PhotonNetwork.ConnectUsingSettings();
    PhotonNetwork.JoinLobby(TypedLobby.Default);
    string randomNickname = FindObjectOfType<RandomNickname>().Generate();

    PhotonNetwork.NickName = randomNickname;
    Debug.Log(randomNickname);
  }

  public override void OnConnectedToMaster()
  {
    print("서버접속완료");
  }

  public void Disconnect() => PhotonNetwork.Disconnect();

  public override void OnDisconnected(DisconnectCause cause) => print("연결끊김");

  public override void OnCreateRoomFailed(short returnCode, string message) => print("방만들기실패");

  public void ClickQuickButton()
  {
    // PhotonNetwork.LoadLevel("LoadingScene");
    JoinRandomRoom();
  }

  void JoinRandomRoom()
  {
    PhotonNetwork.JoinRandomRoom();
  }

  public override void OnJoinedRoom()
  {
    PhotonNetwork.LoadLevel("WaitingScene");
  }

  public override void OnJoinRandomFailed(short returnCode, string message)
  {
    double createAt = (System.DateTime.Now - new System.DateTime(1970, 1, 1)).TotalSeconds;
    
    // 게임 오브젝트 제거 방지
    RoomOptions roomOptions = new RoomOptions { MaxPlayers = 4 };
    // roomOptions.CleanupCacheOnLeave = false;

    roomOptions.CustomRoomProperties = new ExitGames.Client.Photon.Hashtable()
    {
            {"createAt", createAt}
    };
    PhotonNetwork.CreateRoom(randomRoomName, roomOptions);
  }
}
