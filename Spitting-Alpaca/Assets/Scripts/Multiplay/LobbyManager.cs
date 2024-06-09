using Photon.Pun; // ����Ƽ�� ���� ������Ʈ��
using Photon.Realtime; // ���� ���� ���� ���̺귯��
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// ������(��ġ ����ŷ) ������ �� ������ ���
public class LobbyManager : MonoBehaviourPunCallbacks
{
    private string gameVersion = "100"; // ���� ����

    public TextMeshProUGUI connectionInfoText; // ��Ʈ��ũ ������ ǥ���� �ؽ�Ʈ
    public Button joinButton; // �� ���� ��ư

    // ���� ����� ���ÿ� ������ ���� ���� �õ�
    private void Start()
    {
        // ���ӿ� �ʿ��� ����(���� ����) ����
        PhotonNetwork.GameVersion = gameVersion;
        // ������ ������ ������ ������ ���� ���� �õ�
        PhotonNetwork.ConnectUsingSettings();

        string randomNickname = FindObjectOfType<RandomNickname>().Generate();
        PhotonNetwork.NickName = randomNickname;

        // �� ���� ��ư�� ��� ��Ȱ��ȭ
        joinButton.interactable = false;
        // ������ �õ� ������ �ؽ�Ʈ�� ǥ��
        connectionInfoText.text = "connecting to master server..";
    }

    // ������ ���� ���� ������ �ڵ� ����
    public override void OnConnectedToMaster()
    {
        // �� ���� ��ư�� Ȱ��ȭ
        joinButton.interactable = true;
        // ���� ���� ǥ��
        connectionInfoText.text = "online : connect";
    }

    // ������ ���� ���� ���н� �ڵ� ����
    public override void OnDisconnected(DisconnectCause cause)
    {
        // �� ���� ��ư�� ��Ȱ��ȭ
        joinButton.interactable = false;
        // ���� ���� ǥ��
        connectionInfoText.text = "offline : retry...";

        // ������ �������� ������ �õ�
        PhotonNetwork.ConnectUsingSettings();
    }

    // �� ���� �õ�
    public void Connect()
    {
        // �ߺ� ���� �õ��� ���� ����, ���� ��ư ��� ��Ȱ��ȭ
        joinButton.interactable = false;

        // ������ ������ �������̶��
        if (PhotonNetwork.IsConnected)
        {
            // �� ���� ����
            connectionInfoText.text = "Connecting...";
            PhotonNetwork.JoinRandomRoom();
        }
        else
        {
            // ������ ������ �������� �ƴ϶��, ������ ������ ���� �õ�
            connectionInfoText.text = "offline : retry...";
            // ������ �������� ������ �õ�
            PhotonNetwork.ConnectUsingSettings();
        }
    }

    // (�� ���� ����)���� �� ������ ������ ��� �ڵ� ����
    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        // ���� ���� ǥ��
        connectionInfoText.text = "no room, create new room...";
        // �ִ� 6���� ���� ������ ����� ����

        // 게임 오브젝트 제거 방지
        RoomOptions roomOptions = new RoomOptions {MaxPlayers = 6};
        roomOptions.CleanupCacheOnLeave = false;

        PhotonNetwork.CreateRoom(null, roomOptions);
    }

    // �뿡 ���� �Ϸ�� ��� �ڵ� ����
    public override void OnJoinedRoom()
    {
        // ���� ���� ǥ��
        connectionInfoText.text = "success";
        // ��� �� �����ڵ��� Main ���� �ε��ϰ� ��
        PhotonNetwork.LoadLevel("WaitingScene");
    }
}