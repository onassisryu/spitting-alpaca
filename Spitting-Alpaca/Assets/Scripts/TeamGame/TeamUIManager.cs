using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
using System.Collections;
using UnityEngine.SceneManagement;
using Unity.VisualScripting;

public class TeamUIManager : MonoBehaviour
{
    public static TeamUIManager Instance
    {
        get
        {
            if (m_instance == null)
            {
                m_instance = FindObjectOfType<TeamUIManager>();
            }

            return m_instance;
        }
    }

    private static TeamUIManager m_instance;
    private PhotonView photonView;

    public TextMeshProUGUI killLogText;
    public TextMeshProUGUI livingPeopleText;

    public Image eatStatus;
    public Image spitStatus;

    public TextMeshProUGUI nicknameText;
    public Image stunText;
    private Animator stunAnimator;
    public Image stunBarLayout;
    public Image stunBarStatus;

    public Image aliveIcon;
    public Image deadIcon;

    public Image itemLayout;
    public Image itemTornado;
    public Image itemHormone;

    private string nickname;
    private string currentSceneName;

    // Menu
    public GameObject menuUI;
    public bool menuActiveSelf = false;

    // WaitingScene Room Name UI
    public TextMeshProUGUI roomName;

    // GameSetText
    public Image gameSetText;

    public Image hungryText;

    public DataSyncManager dataSyncManager;
    
    public void SubscribeToSceneEvents()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {  
        dataSyncManager = GameObject.FindAnyObjectByType<DataSyncManager>();
        // Timer timer = GameObject.FindAnyObjectByType<Timer>();
        livingPeopleText.text = "살아있는 사람 : " + dataSyncManager.sceneData.Defend.AliveCount;
    }
    private void Awake(){
        Debug.Log("team ui manager awake called");
        photonView = GetComponent<PhotonView>();
        currentSceneName = SceneManager.GetActiveScene().name;
    }

    
    private void Start()
    {
        
        PhotonNetwork.AutomaticallySyncScene = true;
        nickname = PhotonNetwork.NickName;
        nicknameText.text = nickname;
        // livingPeopleText.text = "살아있는 사람 : " + TeamGameManager.instance.data.Defend.AliveCount;
                
        if (roomName != null)
        {
            roomName.text = PhotonNetwork.CurrentRoom.Name;
            Debug.Log(PhotonNetwork.CurrentRoom.Name);
        }
        

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        if (currentSceneName != "WaitingScene")
        {
            stunBarLayout.enabled = false;
            stunBarStatus.enabled = false;
            deadIcon.enabled = false;
            itemTornado.enabled = false;
            itemHormone.enabled = false;

            killLogText.text = "";

            if (stunText != null)
            {
                stunAnimator = stunText.GetComponent<Animator>();
            }
        }

        if (menuUI != null)
        {
            menuUI.SetActive(false);
            menuActiveSelf = false;
        }
    }

    private void Update()
    {
        if(TeamGameManagerVer2.instance.dataSyncManager != null){
            livingPeopleText.text =  "살아있는 사람 : " + TeamGameManagerVer2.instance.dataSyncManager.sceneData.defend.AliveCount;
        }
    }

    public void SetMenu()
    {
        if (menuUI != null)
        {
            if (menuUI.activeSelf == false)
            {
                menuUI.SetActive(true);
                menuActiveSelf = true;
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
            }
            else
            {
                menuUI.SetActive(false);
                menuActiveSelf = false;
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
            }
        }
    }

    public void UpdateCooltimeText(float cooltime)
    {
        if (cooltime > 0)
        {
            spitStatus.fillAmount = (2f - cooltime) / 2f;
        }
        else
        {
            spitStatus.fillAmount = 1f;
        }
    }

    public void UpdateHungerBar(float hunger)
    {
        if (eatStatus != null)
        {
            eatStatus.fillAmount = hunger / 100f;
            if (eatStatus.fillAmount > 0.4f)
            {
                SetHungry(false);
            }
            else
            {
                SetHungry(true);
            }
        }
    }

    public void createKillLogHungry(string killer, string victim)
    {
        photonView.RPC("RpcCreateKillLog", RpcTarget.All, killer, victim);
    }

    public void UpdateStunStatus(float leftTime)
    {
        if (stunBarLayout != null && stunBarStatus != null)
        {
            if (leftTime > 0)
            {
                stunBarLayout.enabled = true;
                stunBarStatus.fillAmount = leftTime / 4f;
            }
            else
            {
                stunBarLayout.enabled = false;

            }
        }
    }

    public void SetStun(bool status)
    {
        if (stunAnimator != null && stunBarStatus != null)
        {
            if (status)
            {
                stunAnimator.SetBool("isStunned", true);
                stunBarStatus.enabled = true;
            }
            else
            {
                stunAnimator.SetBool("isStunned", false);
                stunBarStatus.enabled = false;
            }
        }
    }

    public void SetDeadUI(bool dead)
    {
        if (dead)
        {
            stunText.enabled = false;
            stunBarLayout.enabled = false;
            stunBarStatus.enabled = false;
            eatStatus.enabled = false;
            spitStatus.enabled = false;
            aliveIcon.enabled = false;
            deadIcon.enabled = true;
            itemLayout.enabled = false;
            itemTornado.enabled = false;
            itemHormone.enabled = false;
        }
    }

    public void SetItem(string itemName)
    {
        if (itemName == "Tornado" && itemTornado != null)
        {
            itemTornado.enabled = true;
            if (itemHormone != null)
            {
                itemHormone.enabled = false;
            }
        }
        else if (itemName == "Hormone" && itemHormone != null)
        {
            itemHormone.enabled = true;
            if (itemTornado != null)
            {
                itemTornado.enabled = false;
            }
        }
        else if (itemName == "" && itemTornado != null && itemHormone != null)
        {
            itemTornado.enabled = false;
            itemHormone.enabled = false;
        }
    }

    // 킬 로그를 생성하고. 3초 후에 로그를 지우는 메소드
    public void createKillLog(string killer, string victim)
    {
        if(photonView.IsMine || PhotonNetwork.IsMasterClient){
            photonView.RPC("RpcCreateKillLog", RpcTarget.All, killer, victim);
        }
    }

    [PunRPC]
    private void RpcCreateKillLog2(string killer, string victim){
        Debug.Log("master master");
         photonView.RPC("RpcCreateKillLog", RpcTarget.Others, killer, victim);
    }

    [PunRPC]
    public void RpcCreateKillLog(string killer, string victim)
    {
        // 새 킬 정보를 기존 로그에 추가
        string newKillLog = $"{killer} killed {victim}\n";
        killLogText.text += newKillLog;
        
        Invoke("clearLog", 3f);
    }
    private void clearLog(){
        if (killLogText.text.Contains("\n"))
        {
            // 가장 오래된 로그 메시지를 제거
            killLogText.text = killLogText.text.Substring(killLogText.text.IndexOf('\n') + 1);
        }else {
            killLogText.text = "";
        }
    }
        
    public void GameSetUI()
    {
        StartCoroutine(LoadGameSetUI());
    }

    IEnumerator LoadGameSetUI()
    {
        if (gameSetText != null)
        {
            gameSetText.gameObject.SetActive(true);
            gameSetText.gameObject.GetComponent<Animator>().SetBool("isActive", true);
            yield return new WaitForSeconds(2.5f);

            gameSetText.gameObject.SetActive(false);
        }
        yield return null;
    }
   
    public void SetHungry(bool status)
    {
        if (hungryText != null)
        {
            hungryText.gameObject.SetActive(status);

            Animator hungryAnimator = hungryText.gameObject.GetComponent<Animator>();
            hungryAnimator.SetBool("isActive", status);
        }
    }
}