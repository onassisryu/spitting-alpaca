using Photon.Realtime;
using Photon.Pun;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;
using Unity.VisualScripting.Antlr3.Runtime.Tree;
using Unity.VisualScripting;
using System.Collections.Generic;


// ������ ���� ���� ����, ���� UI�� �����ϴ� ���� �Ŵ���
public class GameManager : MonoBehaviourPunCallbacks, IPunObservable
{
    // �ܺο��� �̱��� ������Ʈ�� �����ö� ����� ������Ƽ
    public static GameManager instance
    {
        get
        {
            // ���� �̱��� ������ ���� ������Ʈ�� �Ҵ���� �ʾҴٸ�
            if (m_instance == null)
            {
                // ������ GameManager ������Ʈ�� ã�� �Ҵ�
                m_instance = FindObjectOfType<GameManager>();
            }

            // �̱��� ������Ʈ�� ��ȯ
            return m_instance;
        }
    }

    private static GameManager m_instance; // �̱����� �Ҵ�� static ����

    public GameObject playerPrefab; // ������ �÷��̾� ĳ���� ������
    public GameObject playerCamera;

    public bool isGameActive = true;

    public TextMeshProUGUI gameStartText; // ���ӽ��� �˸��� text
    public TextMeshProUGUI flowerText; // ����ȭ ���� �Ǿ����ϴ� text
    public AudioSource audioSource;

    private int score = 0; // ���� ���� ����
    public bool isGameover { get; private set; } // ���� ���� ����

    // 씬로드가 한 번만 일어나게 check하는 bool 변수
    private bool isSceneLoading = false;

    private int AliveCount = 0;
    

    private bool playersInitialized = false;
    int index = 1;

    private void Awake()
    {

        // ���� �̱��� ������Ʈ�� �� �ٸ� GameManager ������Ʈ�� �ִٸ�
        if (instance != this)
        {
            // �ڽ��� �ı�
            Destroy(gameObject);
        }
    }

    // ���� ���۰� ���ÿ� �÷��̾ �� ���� ������Ʈ�� ����
    private void Start()
    {

        // SetPlayerAlive(true);
        UIManager.Instance.setLivingPerson(PhotonNetwork.CurrentRoom.PlayerCount);

        gameStartText.text = "Lets spitting!";
        StartCoroutine(ClearTextAfterDelay(gameStartText, 2f));

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        //부모 오브젝트 찾기
        GameObject[] spawners = GameObject.FindGameObjectsWithTag("Spawn");
        if (spawners != null)
        {
            //spanwers 랜덤 선택
            GameObject spawner = spawners[Random.Range(0, spawners.Length)];
            Vector3 spawnerPosition = spawner.transform.position;
            Vector3 randomSpawnPos = spawnerPosition + (Random.insideUnitSphere * 3f);
            // ��ġ y���� 0���� ����
            randomSpawnPos.y = spawnerPosition.y + 3;

            Vector3 randomCameraPos = randomSpawnPos;
            randomCameraPos.y += 0.6f;
            PhotonNetwork.Instantiate(playerPrefab.name, randomSpawnPos, Quaternion.identity);
            PhotonNetwork.Instantiate(playerCamera.name, randomCameraPos, Quaternion.identity);
            StartCoroutine(StartFlowerTextAfterDelay(5f, "무궁화 꽃이 피었습니다", 5f));
        }
        else
        {
            Vector3 randomSpawnPos = Random.insideUnitSphere * 3f;
            // ��ġ y���� 0���� ����
            randomSpawnPos.y = 0f;

            Vector3 randomCameraPos = randomSpawnPos;
            randomCameraPos.y = 0.6f;

            // ��Ʈ��ũ ���� ��� Ŭ���̾�Ʈ�鿡�� ���� ����
            // ��, �ش� ���� ������Ʈ�� �ֵ�����, ���� �޼��带 ���� ������ Ŭ���̾�Ʈ���� ����
            PhotonNetwork.Instantiate(playerPrefab.name, randomSpawnPos, Quaternion.identity);
            PhotonNetwork.Instantiate(playerCamera.name, randomCameraPos, Quaternion.identity);
            StartCoroutine(StartFlowerTextAfterDelay(5f, "무궁화 꽃이 피었습니다", 5f));
        }

        // ������ ���� ��ġ ����

    }

    IEnumerator StartFlowerTextAfterDelay(float initialDelay, string text, float completeDuration)
    {
        // ���� �ð��뿡 ������ ���� 
        float randomDelay = Random.Range(300, 600);
        yield return new WaitForSeconds(randomDelay);

        // ȣ��Ʈ������ RPC ȣ��
        if (PhotonNetwork.IsMasterClient)
        {
            photonView.RPC("ShowTextGraduallyRPC", RpcTarget.All, text, completeDuration);
        }
    }

    // �߰��ٸ�, IEnumerator �ȿ����� StartCoroutine�� �� �Ἥ PV.RPC�� ShowText..�� ȣ�� 
    [PunRPC]
    void ShowTextGraduallyRPC(string text, float duration) // �ڷ�ƾ�� ������� �ʴ� ������ �������� ����
    {
        StartCoroutine(ShowTextGradually(flowerText, text, duration)); // ���� TextMeshProUGUI ������Ʈ ���
    }

    // ������ ����
    IEnumerator ShowTextGradually(TextMeshProUGUI textElement, string text, float duration)
    {
        audioSource.Play();
        textElement.text = "";
        float delay = duration / text.Length;  // ��ü ���̿� ���� �յ��ϰ� ���� ǥ�� �ð� �й�

        foreach (char c in text)
        {
            textElement.text += c;
            yield return new WaitForSeconds(delay);
        }

        // ��� AI�� �������� ����ϴ�.
        UpdateAllAiMovementState(false);

        // ��ü ������ ǥ�õ� �� 1�ʸ� ��ٸ��ϴ�.
        yield return new WaitForSeconds(1.0f);

        // �ؽ�Ʈ�� ����ϴ�.
        textElement.text = "";

        // n�ʰ� ���� ��, AI�� �������� �ٽ� Ȱ��ȭ�մϴ�.
        yield return new WaitForSeconds(3f);
        UpdateAllAiMovementState(true);
    }

    // �������ϸ� AiScript�� �Լ� ����
    void UpdateAllAiMovementState(bool isActive)
    {
        var ais = FindObjectsOfType<Ai>(); //��� Ai�ν��Ͻ��� ã�� �迭�� ����
        foreach (var ai in ais) // �迭�� �� AiScript �ν��Ͻ��� ���� �ݺ�
        {
            ai.UpdateMovementState(isActive); // �� AI�� ������ ���¸� isActive �Ű����� ������ ������Ʈ

        }
    }

    [PunRPC]
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

    // 1���� ȭ�鿡 ���ִ� ������ ���� ����
    IEnumerator ClearTextAfterDelay(TextMeshProUGUI textElement, float delay)
    {
        yield return new WaitForSeconds(delay);
        textElement.text = "";
    }

    // ������ �߰��ϰ� UI ����
    public void AddScore(int newScore)
    {
        // ���� ������ �ƴ� ���¿����� ���� ���� ����
        if (!isGameover)
        {
            // ���� �߰�
            score += newScore;
        }
    }

    // ���� ���� ó��
    public void EndGame()
    {
        // ���� ���� ���¸� ������ ����
        isGameover = true;
    }

    // Ű���� �Է��� �����ϰ� ���� ������ ��
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            PhotonNetwork.LeaveRoom(false);
        }
    }

    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
    }

    // ���� ������ �ڵ� ����Ǵ� �޼���
    public override void OnLeftRoom(){
        // 방을 떠난 플레이어가 본인일 경우
        if (photonView.IsMine){

        // 게임 오브젝트를 찾고 isAlive 상태를 false로 설정
            GameObject[] playerObjects = GameObject.FindGameObjectsWithTag("Player");
            foreach (GameObject playerObject in playerObjects){

                PlayerMovementTest playerScript = playerObject.GetComponent<PlayerMovementTest>();
                if (playerScript != null && playerScript.photonView.IsMine){
                    playerScript.isAlive = false;
                }
            }

            // 모든 클라이언트에서 CheckAlivePlayers 호출
            photonView.RPC("CheckAlivePlayers", RpcTarget.All);

            // 씬 전환
            SceneManager.LoadScene("PreGameMap");
        }
    }

public override void OnPlayerLeftRoom(Player otherPlayer)
{
    Debug.Log($"Player {otherPlayer.NickName} left the room.");

    GameObject[] playerObjects = GameObject.FindGameObjectsWithTag("Player");
    Debug.Log($"Found {playerObjects.Length} player objects.");

    foreach (GameObject playerObject in playerObjects)
    {
        PlayerMovementTest playerScript = playerObject.GetComponent<PlayerMovementTest>();
        if (playerScript != null)
        {
            Debug.Log($"Checking player object with owner: {playerScript.photonView.Owner.NickName}");

            if (playerScript.photonView.Owner == otherPlayer)
            {
                playerScript.isAlive = false;
                Debug.Log($"Setting isAlive to false for player: {otherPlayer.NickName}");
            }
        }
        else
        {
            Debug.LogWarning("Player object does not have PlayerMovementTest component.");
        }
    }

    // 모든 클라이언트에서 CheckAlivePlayers 호출
    Debug.Log("Calling CheckAlivePlayers via RPC");
    photonView.RPC("CheckAlivePlayers", RpcTarget.All);
}

    // public void SetPlayerAlive(bool isAlive)
    // {

    //     ExitGames.Client.Photon.Hashtable props = new ExitGames.Client.Photon.Hashtable{
    //         {"isAlive", isAlive}
    //     };
    //     PhotonNetwork.LocalPlayer.SetCustomProperties(props);
    // }

    [PunRPC]
    public void CheckAlivePlayers()
    {   
        Debug.Log("확인합니다 !");

        if (isSceneLoading) return;
        int deathCount = 0;

        GameObject lastSurvivor = null;
        string winner;
        int survivorsCount = 0;


        List<GameObject> players = new List<GameObject>();
        players.AddRange(GameObject.FindGameObjectsWithTag("Player"));
        
        foreach (GameObject player in players)
        {
            PlayerMovementTest playerScript = player.GetComponent<PlayerMovementTest>();
            if (playerScript != null && playerScript.isAlive)
            {
                lastSurvivor = player;
                survivorsCount++;
            }
            
        }
        UIManager.Instance.setLivingPerson(survivorsCount);

        // 생존 플레이어가 1명 남았을 경우
        if (survivorsCount <= 1)
        {   
            UIManager.Instance.GameSetUI();
            winner = lastSurvivor.GetComponent<PhotonView>().Owner.NickName;
            
            isSceneLoading = true;
            string newKillLog = $"{winner} killed {winner}\n";
            UIManager.Instance.saveKillLogs(newKillLog);
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
        // 새 플레이어가 입장할 때 초기 상태를 강제로 설정
        ExitGames.Client.Photon.Hashtable props = new ExitGames.Client.Photon.Hashtable
    {
        {"isAlive", true}, // 예시로 사용될 초기값
        {"isReady", false}
    };

        newPlayer.SetCustomProperties(props);
    }
}


    