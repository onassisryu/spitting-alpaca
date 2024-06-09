using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.VisualScripting;
using System;
using System.Linq;

public class NetworkManager : MonoBehaviourPunCallbacks
{

    private string gameVersion = "1";

    public TMP_InputField passwordInputField;
    public TextMeshProUGUI wrongPasswordText;

    public static NetworkManager instance
    {
        get{
            // 싱글톤 X
            if(m_instance == null){

                // 씬에서 WaitingManager를 찾아 할당한다
                m_instance = FindObjectOfType<NetworkManager>();
            }
            // 싱글톤 오브젝트를 반환
            return m_instance;
        }
    }

    private static NetworkManager m_instance;

    private void Awake(){
        // 씬에 싱글톤 오브젝트가 된 다른 GameManager 오브젝트가 있다면
        if(instance != this){
            // 자신을 파괴
            Destroy(gameObject);
        }
    }

    public string NetworkClientState()
    {
        return PhotonNetwork.NetworkClientState.ToString();
    }

    void Start()
    {
        wrongPasswordText.gameObject.SetActive(false); // 처음에는 메시지를 숨깁니다.
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

    // public void JoinLobby() => PhotonNetwork.JoinLobby();

    // public override void OnJoinedLobby() => print("로비접속완료");

    // public void LeaveRoom() => PhotonNetwork.LeaveRoom();

    // public override void OnCreatedRoom()
    // {
    //     print("방만들기완료");
    // }


    public void ClickQuickButton()
    {
        PlayerPrefs.SetString("LoadingAction", "RandomRoom");
        PhotonNetwork.LoadLevel("LoadingScene");
    }

    public void ClickMakingRoomButton()
    {
        PlayerPrefs.SetString("LoadingAction", "CreatePrivateRoom");
        PhotonNetwork.LoadLevel("LoadingScene");
    }

    public void ClickEnterRoomButton()
    {
        PlayerPrefs.SetString("roomPassword", passwordInputField.text);
        PlayerPrefs.SetString("LoadingAction", "JoinPrivateRoom");

        if (!PhotonNetwork.InLobby)
        {
            PhotonNetwork.JoinLobby(TypedLobby.Default);
        }
        else
        {
            PhotonNetwork.LeaveLobby();
            PhotonNetwork.JoinLobby(TypedLobby.Default);
        }
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        string roomPassword = PlayerPrefs.GetString("roomPassword", "");
        foreach (var room in roomList)
        {
            if (room.Name == roomPassword)
            {
                PhotonNetwork.LoadLevel("LoadingScene");
                return;
            }
        }
        StartCoroutine(ShowMessageTemporary("다시 입력하세요.", 2.0f));
    }

    public IEnumerator ShowMessageTemporary(string message, float delay)
    {
        wrongPasswordText.text = message;
        wrongPasswordText.gameObject.SetActive(true);
        yield return new WaitForSeconds(delay);
        wrongPasswordText.gameObject.SetActive(false);
    }
}