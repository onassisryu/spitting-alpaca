using UnityEngine.SceneManagement;
using System.Collections.Generic;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using System.Linq;

public class LoadingManager : MonoBehaviourPunCallbacks
{
    private string randomRoomName = "안녕 알파카";

    void Start()
    {
        randomRoomName = FindObjectOfType<RandomRoomName>().Generate();
        string action = PlayerPrefs.GetString("LoadingAction");

        if (PhotonNetwork.IsConnected)
        {
            Invoke("ExecuteAction", 2.6f);
        }
        else
        {
            PhotonNetwork.ConnectUsingSettings();
        }
    }

    void ExecuteAction()
    {
        string action = PlayerPrefs.GetString("LoadingAction");
        switch (action)
        {
            case "RandomRoom":
                JoinRandomRoom();
                break;
            case "CreatePrivateRoom":
                CreatePrivateRoom();
                break;
            case "JoinPrivateRoom":
                JoinPrivateRoom();
                break;
        }
    }

    void JoinRandomRoom()
    {
        Hashtable expectedCustomRoomProperties = new Hashtable() { { "isSecret", false } };
        PhotonNetwork.JoinRandomRoom(expectedCustomRoomProperties, 0);
    }

    void CreatePrivateRoom()
    {
        string password = GenerateRandomPassword(8);
        RoomOptions roomOptions = new RoomOptions { MaxPlayers = 6 };
        Hashtable hash = new Hashtable
        {
            { "password", password },
            { "isSecret", true }
        };
        roomOptions.CustomRoomProperties = hash;
        roomOptions.CustomRoomPropertiesForLobby = new string[] { "password", "isSecret" };
        PhotonNetwork.CreateRoom(password, roomOptions);
    }

    void JoinPrivateRoom()
    {
        string roomPassword = PlayerPrefs.GetString("roomPassword", "");
        if (!string.IsNullOrEmpty(roomPassword))
        {
            PhotonNetwork.JoinRoom(roomPassword);
        }
        else
        {
            Debug.LogError("No room password provided.");
        }
    }

    private string GenerateRandomPassword(int length)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        var random = new System.Random();
        return new string(Enumerable.Repeat(chars, length).Select(s => s[random.Next(s.Length)]).ToArray());
    }

    public override void OnJoinedRoom()
    {
        Debug.Log($"Joined room {PhotonNetwork.CurrentRoom.Name}");
        PhotonNetwork.LoadLevel("WaitingScene");
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        CreateRoomWithCustomOptions((System.DateTime.Now - new System.DateTime(1970, 1, 1)).TotalSeconds);
    }

    private void CreateRoomWithCustomOptions(double createAt)
    {
        RoomOptions options = new RoomOptions
        {
            MaxPlayers = 4,
            CustomRoomProperties = new Hashtable
            {
                { "isSecret", false },
                { "createAt", createAt }
            },
            CustomRoomPropertiesForLobby = new string[] { "isSecret" }
        };
        PhotonNetwork.CreateRoom(randomRoomName, options);
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.LogError("Failed to join room: " + message);
        if (PlayerPrefs.GetString("LoadingAction") == "JoinPrivateRoom")
        {
            string roomPassword = PlayerPrefs.GetString("roomPassword", "");
            PhotonNetwork.JoinRoom(roomPassword);
        }
    }
}