using Photon.Realtime;
using Photon.Pun;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using ExitGames.Client.Photon.StructWrapping;
public class TeamGameManager : MonoBehaviourPunCallbacks, IPunObservable
{
    public static TeamGameManager instance
    {
        get
        {
            if (m_instance == null)
            {
                m_instance = FindObjectOfType<TeamGameManager>();
            }
            return m_instance;
        }
    }
    
    private static TeamGameManager m_instance;

    public GameObject attackPrefab;
    public GameObject defendPrefab;
    public GameObject playerCamera;

    public bool isGameActive = true;

    public TextMeshProUGUI gameStartText;

    private int score = 0;
    public bool isGameover { get; private set; }

    public SceneData data;

    private bool isSceneLoading = false;

    private List<GameObject> players = new List<GameObject>();

    private void Awake()
    {
        if (instance != this)
        {
            Destroy(gameObject);
            return ;
        }

        m_instance = this;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        data = new SceneData();
        Team attackTeam = new Team();
        Team defendTeam = new Team();
        
        attackTeam.TeamId = 1;
        attackTeam.TeamName = "attack";
        
        defendTeam.TeamId = 2;
        defendTeam.TeamName = "defend";
        
        data.Attack = attackTeam;
        data.Defend = defendTeam;
    }

    private void Start()
    {
        /* SetPlayerAlive(true); */

        gameStartText.text = "Team Game!";
        StartCoroutine(ClearTextAfterDelay(gameStartText, 2f));

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (PhotonNetwork.IsMasterClient)
        {
            StartCoroutine(DelayedAssignAndSpawnPlayers());
        }
    }

    private IEnumerator DelayedAssignAndSpawnPlayers()
    {
        yield return new WaitForSeconds(2.0f); // 2초 지연
        AssignAndSpawnPlayers();
    }

    private void AssignAndSpawnPlayers()
    {
        // ������ Ŭ���̾�Ʈ������ �� ������ �ϰ� �÷��̾ �����մϴ�.
        if (!PhotonNetwork.IsMasterClient) return;
        // 팀 정하는 부분
        Player[] playerList = PhotonNetwork.PlayerList;
        int totalPlayers = playerList.Length;
        int half = totalPlayers / 2;

        // ���� ���� ��� ���� ������ �÷��̾ �����մϴ�.
        for (int i = 0; i < totalPlayers; i++)
        {
            // 팀 세팅
            Team team = (i < half) ? data.Attack : data.Defend;
            team.PlayerAdd(playerList[i]);
            GameObject prefab = (i < half) ? attackPrefab : defendPrefab; 
            photonView.RPC("SpawnPlayer", playerList[i], prefab.name);

        }
        UIManager.Instance.setLivingPerson(data.Defend.AliveCount);
    }

    [PunRPC]
    private void SpawnPlayer(string prefabName, PhotonMessageInfo info)
    {
        Player player = info.Sender;
        GameObject prefab = (prefabName == attackPrefab.name) ? attackPrefab : defendPrefab;
        GameObject spawnPoint = null;

        if (prefab == attackPrefab)
        {
            spawnPoint = GameObject.FindWithTag("AttackSpawn");
        }
        else if (prefab == defendPrefab)
        {
            spawnPoint = GameObject.FindWithTag("DefendSpawn");
        }

        if (spawnPoint != null && prefab != null)
        {
            Vector3 spawnPosition = spawnPoint.transform.position + (Random.insideUnitSphere * 3f);
            spawnPosition.y = spawnPoint.transform.position.y;

            Vector3 cameraPosition = spawnPosition;
            cameraPosition.y += 0.6f;

            GameObject playerObject = PhotonNetwork.Instantiate(prefab.name, spawnPosition, Quaternion.identity);
            players.Add(playerObject); // ������ �÷��̾ ����Ʈ�� �߰�
            PhotonNetwork.Instantiate(playerCamera.name, cameraPosition, Quaternion.identity);
        }
        else
        {
            Debug.LogError("SpawnPoint or Prefab is null. Check prefab tags and spawn points.");
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(isGameActive);
        }
        else
        {
            isGameActive = (bool)stream.ReceiveNext();
        }
    }

    IEnumerator ClearTextAfterDelay(TextMeshProUGUI textElement, float delay)
    {
        yield return new WaitForSeconds(delay);
        textElement.text = "";
    }

    public void AddScore(int newScore)
    {
        if (!isGameover)
        {
            score += newScore;
        }
    }

    public void EndGame()
    {
        isGameover = true;
    }

    private void Update()
    {
    }

    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
    }

    public override void OnLeftRoom()
    {
        SceneManager.LoadScene("preGameMap");
    }

    public void SetPlayerAlive(bool isAlive)
    {
        ExitGames.Client.Photon.Hashtable props = new ExitGames.Client.Photon.Hashtable
        {
            {"isAlive", isAlive}
        };
        PhotonNetwork.LocalPlayer.SetCustomProperties(props);
    }

    private void CheckAlivePlayers()
    {
        if (isSceneLoading) return;
        
        // if(data.isGameover()){
        //     isSceneLoading = true;
        //     ExitGames.Client.Photon.Hashtable customRoomProperties = PhotonNetwork.CurrentRoom.CustomProperties;

        //     // "result" 프로퍼티가 이미 존재하는지 확인하고 값 변경 또는 추가
        //     if (customRoomProperties.ContainsKey("result"))
        //         customRoomProperties["result"] = data; // 기존 값 변경
        //     else
        //         customRoomProperties.Add("result", data); // 새로운 값 추가
        //     // 변경된 커스텀 프로퍼티 적용
        //     PhotonNetwork.CurrentRoom.SetCustomProperties(customRoomProperties);
            

            
        //     StartCoroutine(LoadLevelDelay());
        // }

        UIManager.Instance.setLivingPerson(data.Defend.AliveCount);

        int deathCount = 0;

        foreach (Player player in PhotonNetwork.PlayerList)
        {
            object isAlive;
            if (player.CustomProperties.TryGetValue("isAlive", out isAlive))
            {
                if (!(bool)isAlive)
                {
                    deathCount++;
                }
            }
        }

        int livingPeopleCount = PhotonNetwork.CurrentRoom.PlayerCount - deathCount;
        UIManager.Instance.setLivingPerson(livingPeopleCount);

        if (livingPeopleCount == 1)
        {
            UIManager.Instance.GameSetUI();

            players.AddRange(GameObject.FindGameObjectsWithTag("DefendPlayer"));
            string winner = "";
            foreach (GameObject player in players)
            {
                PlayerMovementTest playerScript = player.GetComponent<PlayerMovementTest>();
                if (playerScript != null && playerScript.isAlive)
                {
                    winner = player.GetComponent<PhotonView>().Owner.NickName;
                    break;
                }
            }

            isSceneLoading = true;
           
            StartCoroutine(LoadLevelDelay());
        }
    }

    IEnumerator LoadLevelDelay()
    {
        yield return new WaitForSeconds(3f);
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.LoadLevel("ResultScene");
        }
    }

    public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
    {
        ExitGames.Client.Photon.Hashtable props = new ExitGames.Client.Photon.Hashtable
        {
            {"isAlive", true},
            {"isReady", false}
        };

        newPlayer.SetCustomProperties(props);

        // ���ο� �÷��̾ ���� �� �ش� �÷��̾ �����մϴ�.
        // if (PhotonNetwork.IsMasterClient)
        // {
        //     int totalPlayers = PhotonNetwork.PlayerList.Length;
        //     int half = totalPlayers / 2;

        //     GameObject prefab = (newPlayer.ActorNumber <= half) ? attackPrefab : defendPrefab;
        //     photonView.RPC("SpawnPlayer", RpcTarget.AllBuffered, newPlayer.ActorNumber, prefab.name);
        // }
    }
}
