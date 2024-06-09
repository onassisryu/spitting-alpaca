using System.Collections.Generic;
using System.Linq;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Pun.Demo.Cockpit;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
// 대기실 방에서 게임 준비 상태와 상태 UI를 관리하는 WaitingRoomManager
public class WaitingRoomManager : MonoBehaviourPunCallbacks{

    // 외부에서 싱글톤 오브젝트를 가져올 때 사용
    public static WaitingRoomManager instance
    {
        get{
            // 싱글톤 X
            if(m_instance == null){

                // 씬에서 WaitingManager를 찾아 할당한다
                m_instance = FindObjectOfType<WaitingRoomManager>();
            }else {
                m_instance.getTime();
            }
            // 싱글톤 오브젝트를 반환
            return m_instance;
        }
    }

    // 싱글톤이 할당된 static 변수
    private static WaitingRoomManager m_instance;

    // 생성할 플레이어 캐릭터 프리팹
    public GameObject playerPrefab;
    // 생성할 플레이어의 시점

    private GameObject spawnArea;

    public GameObject playerCamera;

    public string[] mapList = new string[]{
    };
    
    private Timer timer;
    // int createAt;

    bool isLoading = false;
    // private ObjectSpawner objectSpawner;
    
    HashSet<string> mapSet;

    public GameObject mapLoadingCanvas;
    private CanvasGroup mapLoadingCanvasGroup;
    public Slider loadingBar; // 로딩 막대로 사용할 Slider


    private void Awake(){
        // 씬에 싱글톤 오브젝트가 된 다른 GameManager 오브젝트가 있다면
        if(instance != this){
            // 자신을 파괴
            Destroy(gameObject);
        }
        
    }

    private void Start(){

        // 로딩 화면 캔버스 비활성화
        mapLoadingCanvasGroup = mapLoadingCanvas.GetComponent<CanvasGroup>();
        mapLoadingCanvasGroup.alpha = 0;

        timer = FindObjectOfType<Timer>();
        timer.time = getTime();

        PhotonNetwork.AutomaticallySyncScene = true;
        // 새로운 플레이어가 들어오는 것을 열어둔다
        mapSet = new HashSet<string>(mapList);

        // X축과 Z축에 대한 랜덤 위치를 지정된 범위 내에서 생성
        float randomX = Random.Range(-20.5f, 20.5f);
        float randomY = Random.Range(0f, 7f);
        float randomZ = Random.Range(-16.5f, 16.5f);
        Vector3 randomSpawnPos = new Vector3(randomX, randomY, randomZ);

        // 생성된 랜덤 위치에 맞는 카메라 위치 지정
        Vector3 randomCameraPos = new Vector3(randomX, randomY + 0.6f, randomZ);

        // 네트워크 상의 모든 클라이언트에서 생성 실행
        // 단, 해당 게임 오브젝트의 주도권은, 생성 매서드를 직접 실행한 클라이언트에게 있음
        PhotonNetwork.Instantiate(playerPrefab.name, randomSpawnPos, Quaternion.identity);
        PhotonNetwork.Instantiate(playerCamera.name, randomCameraPos, Quaternion.identity);
    }

    public float getTime(){
        return (float)(getRoomCreateAt().AddSeconds(100) -  System.DateTime.Now).TotalSeconds;
    }

    public void CheckAllPlayersReady(){

        if(PhotonNetwork.CurrentRoom.PlayerCount == 1){
            return;
        }
        

        var players = FindObjectsOfType<PlayerReady>();
        if(players.All(p => p.isReady) || (timer != null &&timer.time <= 0) ){
            Debug.Log("check platyer " + players.All(p => p.isReady) + " " +  (timer != null &&timer.time <= 0));
            MoveAllPlayerToNextScene();
        }
    }

    private System.DateTime getRoomCreateAt(){
        object createAtObj;
        if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue("createAt", out createAtObj))
        {
            double unixTime = (double)createAtObj;
            System.DateTime createAt = new System.DateTime(1970, 1, 1, 0, 0, 0, System.DateTimeKind.Utc);
            createAt = createAt.AddSeconds(unixTime);
            return createAt;
        }

        return System.DateTime.Now;
    }


    private void Update(){
        
        if(Input.GetKeyDown(KeyCode.Q)){

            PhotonNetwork.LeaveRoom();
        }


       if(!timer.check()) return ;
        timer.time -= Time.deltaTime;

       if(timer != null && timer.time <= 0){
            if(isLoading) return ;
            isLoading = true;
            CheckAllPlayersReady();
        }
    }

    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
    }

    public override void OnLeftRoom(){
        SceneManager.LoadScene("preGameMap");
    }

    
    public void MoveAllPlayerToNextScene(){

        // 새로운 플레이어가 들어오는 것을 막는다
        PhotonNetwork.CurrentRoom.IsOpen = false;

        if (PhotonNetwork.IsMasterClient)
        {
            RandomMapSelectAndShare();
        }
    }

    private void RandomMapSelectAndShare()
    {
        int randInt = Random.Range(0, mapList.Length);
        string selectedMap = mapList[randInt];

        // 선택된 맵 정보를 Custom Properties에 저장
        Hashtable props = new Hashtable { { "selectedMap", selectedMap } };
        PhotonNetwork.CurrentRoom.SetCustomProperties(props);
    }

    public override void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
    {
        if (propertiesThatChanged.ContainsKey("selectedMap"))
        {
            // 모든 클라이언트: 선택된 맵 로드
            string mapToLoad = (string)propertiesThatChanged["selectedMap"];
            Debug.Log($"{mapToLoad} 맵 선택됨!!!!!");
            StartCoroutine(LoadSceneAsync(mapToLoad));
        }
    }

    private System.Collections.IEnumerator LoadSceneAsync(string mapName)
    {
        mapLoadingCanvas.SetActive(true);
        // UI 업데이트로 로딩 화면 표시 및 로딩 막대 활성화
        mapLoadingCanvasGroup.alpha = 1;
        loadingBar.gameObject.SetActive(true);

        // LoadSceneMode.Single을 사용하여 새 맵 로드
        Debug.Log($"{mapName} 맵 로딩시작!!!!!");
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(mapName, LoadSceneMode.Single);
        // 로딩 진행률 확인
        while (!asyncLoad.isDone)
        {
            Debug.Log($"{mapName} 맵 Loading progress: {asyncLoad.progress * 100}%");       
            loadingBar.value = asyncLoad.progress; // 로딩 막대 업데이트
            yield return null;
        }

        // 로딩 완료를 나타내는 Custom Property 설정
        
        Hashtable props = new Hashtable { { "isMapLoaded", true } };
        PhotonNetwork.LocalPlayer.SetCustomProperties(props);

        // 로딩 막대 숨기기 및 UI 비활성화
        loadingBar.gameObject.SetActive(false);
        mapLoadingCanvasGroup.alpha = 0;
        mapLoadingCanvas.SetActive(false);

       
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
    {
        // 모든 플레이어의 로딩 상태 확인
        if (PhotonNetwork.IsMasterClient)
        {
            bool allLoaded = PhotonNetwork.PlayerList.All(player => player.CustomProperties.ContainsKey("isMapLoaded") && (bool)player.CustomProperties["isMapLoaded"]);
            Debug.Log("맵 로딩됨!!!!!");
            if (allLoaded)
            {
                // 모든 플레이어가 로딩 완료 시 게임 시작
                Debug.Log("맵 전환된다!!!!!");
               

                PhotonNetwork.LoadLevel("MainGameScene");
            }
        }
    }


    public override void OnPlayerLeftRoom(Player otherPlayer)
    {

        if(!PhotonNetwork.IsMasterClient) return ;

        Player[] players = PhotonNetwork.PlayerList;
        int readyCnt = 0;
        
        foreach(Player player in players){
            object obj;
            if (player.CustomProperties.TryGetValue("isReady", out obj))
            {
                bool playerReady = (bool)obj;
                if(playerReady) readyCnt++;
            }
            
        }
        Hashtable customProperties = PhotonNetwork.CurrentRoom.CustomProperties;
        customProperties["readyCount"] = readyCnt;
        
        PhotonNetwork.CurrentRoom.SetCustomProperties(customProperties);
    }
}
