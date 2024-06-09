using UnityEngine;
using Photon.Pun;
using UnityEngine.SceneManagement; // 씬 관리자 관련 코드
using UnityEngine.UI; // UI 관련 코드
using ExitGames.Client.Photon;
using TMPro; // Photon의 Hashtable 사용을 위한 네임스페이스

public class WaitingUIManager : MonoBehaviourPun
{
    public static WaitingUIManager instance
    {
        get {
            if (m_instance == null) {
                m_instance = FindObjectOfType<WaitingUIManager>();
            }
            return m_instance;
        }
    }
    private static WaitingUIManager m_instance;

    public TextMeshProUGUI readyText;

    public TextMeshProUGUI timerText;
    private Timer timer;

    public GameObject gameStartUI;

    private void Update() {
        if (PhotonNetwork.CurrentRoom != null) {
        int playerCount = PhotonNetwork.CurrentRoom.PlayerCount;
        int readyCount = GetReadyCount();
        readyText.text = $"{readyCount}/{playerCount}";
        timerText.text = $"{Mathf.CeilToInt(timer.time)}";
        }
    }

    public void Start(){
        timer = FindObjectOfType<Timer>();
    }
    public void UpdateReadyText(bool isReady) {
        int currentReadyCount = GetReadyCount();
        if (isReady) {
            currentReadyCount++;
        } else {
            currentReadyCount--;
            if(currentReadyCount < 0){
                currentReadyCount = 0;
            }
        }
        SetReadyCount(currentReadyCount);
    }


    private int GetReadyCount() {
        object readyCount;
        if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue("readyCount", out readyCount)) {
            return (int)readyCount;
        }
        return 0; // 기본값은 0
    }

    private void SetReadyCount(int newReadyCount) {
        Hashtable props = new Hashtable() { { "readyCount", newReadyCount } };
        PhotonNetwork.CurrentRoom.SetCustomProperties(props);
    }

    public void SetActiveGameStartUI(bool active) {
        gameStartUI.SetActive(active);
    }
}
