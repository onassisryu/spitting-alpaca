using Photon.Realtime;
using Photon.Pun;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class TeamGameManagerVer2 : MonoBehaviourPunCallbacks
{
    public static TeamGameManagerVer2 instance
    {
        get
        {
            if (m_instance == null)
            {
                m_instance = FindObjectOfType<TeamGameManagerVer2>();
                if (m_instance == null)
                {
                    GameObject obj = new GameObject();
                    m_instance = obj.AddComponent<TeamGameManagerVer2>();
                    obj.name = typeof(TeamGameManagerVer2).ToString() + " (Singleton)";
                }
                m_instance.SubscribeToSceneEvents();
            }
            return m_instance;
        }
    }
    
    private static TeamGameManagerVer2 m_instance;
    
    public PhotonView photonView;
    public GameObject attackPrefab;
    public GameObject defendPrefab;
    public GameObject playerCamera;

    public bool isGameActive = false;

    public DataSyncManager dataSyncManager;

    private bool isSceneLoading = false;

    private bool playerSpawned = false;
    private int playersReady = 0;
    private bool isGameOver = false;

    private void Awake()
    {
        if (m_instance != null && m_instance != this)
        {
            Destroy(gameObject);
            return;
        }

        m_instance = this;
        DontDestroyOnLoad(gameObject);
        photonView =  GetComponent<PhotonView>();
        SubscribeToSceneEvents();
    }
    public void SubscribeToSceneEvents()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    
   private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
   {
       
    }
   

    private IEnumerator StartGame()
    {
        Debug.Log("All players are loaded. Starting the game...");
        photonView.RPC("InitializeGame", RpcTarget.All);

        yield return null;
    }
    

    [PunRPC]
    private void InitializeGame()
    {
        dataSyncManager = FindObjectOfType<DataSyncManager>();
        StartCoroutine(DelayedAssignAndSpawnPlayers());
    }

    private void init(){
         if (dataSyncManager != null && !playerSpawned)
        {
            dataSyncManager.InitializeSceneData();
            AssignTeams();
            SpawnPlayer();
            dataSyncManager.SyncData();
            playerSpawned = true;
        }
    }
    private void initValue(){
        isGameOver = false;
        isGameActive = true;
        Debug.Log("상태변경");
    }
    private void Start()
    {        
        Invoke("initValue", 2f);
        // Timer timer = GameObject.FindAnyObjectByType<Timer>();
      

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false; 
        StartCoroutine(StartGame());
    }

    void AssignTeams()
    {
        Player[] players = PhotonNetwork.PlayerList;
        
        int half = players.Length / 2;

        Team attackTeam = dataSyncManager.GetAttackTeam();
        Team defendTeam = dataSyncManager.GetDefendTeam();

        for (int i = 0; i < players.Length; i++)
        {
            Team team = (i < half) ? attackTeam :defendTeam;
            team.PlayerAdd(players[i]);
        }
        Debug.Log($"Teams assigned: Attack team count = {attackTeam.AliveCount} Defend team count = {defendTeam.AliveCount}");
    }

    private IEnumerator DelayedAssignAndSpawnPlayers()
    {
        yield return new WaitForSeconds(2.0f); // 2초 지연
        init();
    }


    private void SpawnPlayer()
    {
        Team attackTeam = dataSyncManager.GetAttackTeam();
        // Team defendTeam = dataSyncManager.GetDefendTeam();
        GameObject playerPrefab = attackTeam.Players.Contains(PhotonNetwork.LocalPlayer) ? attackPrefab : defendPrefab;
        GameObject spawnObject = attackTeam.Players.Contains(PhotonNetwork.LocalPlayer) ? GameObject.FindWithTag("AttackSpawn") : GameObject.FindWithTag("DefendSpawn");
        PlayerInstantiate(playerPrefab,spawnObject );
    }

    void PlayerInstantiate(GameObject playerObj, GameObject spawnObject ){
        Vector3 spawnPosition = spawnObject.transform.position + (Random.insideUnitSphere * 3f);
        spawnPosition.y = spawnObject.transform.position.y;

        Vector3 cameraPosition = spawnPosition;
        cameraPosition.y += 0.6f;

        GameObject player = PhotonNetwork.Instantiate(playerObj.name, spawnPosition, Quaternion.identity);
        GameObject camera = PhotonNetwork.Instantiate(playerCamera.name, cameraPosition, Quaternion.identity);
        
        Debug.Log("Player instance created");
        Debug.Log("instance create test");
    }

    IEnumerator ClearTextAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        Debug.Log(otherPlayer.NickName + " left the room.");
        
        if(PhotonNetwork.IsMasterClient){
            dataSyncManager.sceneData.LeftPlayer(otherPlayer);
            dataSyncManager.SyncData();
        }
    } 

    private void Update()
    {

        if(isGameActive && dataSyncManager.sceneData.IsGameOver()){
            if(!PhotonNetwork.IsMasterClient) return;
            Debug.Log("TEST =- 1");
            SceneData sdata = SerializationHelper.DeepCopy(dataSyncManager.sceneData);
            SceneDataHandler handler = new SceneDataHandler();
            handler.SaveSceneDataToRoomProperties(sdata);
            PhotonNetwork.LoadLevel("TeamResultScene");
        }
    }


    public override void OnLeftRoom()
    {
        SceneManager.LoadScene("preGameMap");
    }


    IEnumerator LoadLevelDelay()
    {
        yield return new WaitForSeconds(3f);
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.LoadLevel("ResultScene");
        }
    }
}
