using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
using System.Collections;
using UnityEngine.SceneManagement;
using Unity.VisualScripting;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance
    {
        get
        {
            if (m_instance == null)
            {
                m_instance = FindObjectOfType<UIManager>();
            }

            return m_instance;
        }
    }

    private static UIManager m_instance;
    private PhotonView photonView;

    public Canvas mainCanvas; // 전체 UI Canvas

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

    private void Awake(){
        photonView = GetComponent<PhotonView>();
        currentSceneName = SceneManager.GetActiveScene().name;
    }


    private void Start()
    {
        nickname = PhotonNetwork.NickName;
        nicknameText.text = nickname;
        // livingPeopleText.text = "살아있는 사람 : " + TeamGameManager.instance.data.Defend.AliveCount;
        
        livingPeopleText.text = "살아있는 사람 : " + PhotonNetwork.CurrentRoom.PlayerCount;
        
        
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
        currentSceneName = SceneManager.GetActiveScene().name;

        if (currentSceneName == "WaitingScene" || currentSceneName == "Main" || currentSceneName == "Map2" || currentSceneName == "Map1")
        {
            if (Input.GetButtonDown("Cancel"))
            {
                SetMenu();
            }
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

    public void LeaveRoomByMenu()
    {
        PhotonNetwork.LeaveRoom();
    }

    public void UpdateCooltimeText(float cooltime, float maxCooltime)
    {
        if (cooltime > 0)
        {
            spitStatus.fillAmount = cooltime / maxCooltime; // 수정된 부분
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
        Debug.Log($"scene name {currentSceneName} ");
        if(currentSceneName == "JaechanFight" || currentSceneName == "TeamFight") return ;
        photonView.RPC("RpcCreateKillLog", RpcTarget.All, killer, victim);
    }

    [PunRPC]
    public void RpcCreateKillLog(string killer, string victim)
    {
        
        if(currentSceneName == "JaechanFight" || currentSceneName == "TeamFight") return ;

        // 새 킬 정보를 기존 로그에 추가
        string newKillLog = $"{killer} killed {victim}\n";
        killLogText.text += newKillLog;

        saveKillLogs(newKillLog);
        
        // 로그를 지우는 코루틴 시작
        StartCoroutine(ClearKillLog());
    }

        // 3초를 기다린 후 텍스트를 지우는 코루틴
    private IEnumerator ClearKillLog()
    {

        // 3초 동안 대기
        yield return new WaitForSeconds(3);

        // 텍스트 클리어
        if (killLogText.text.Contains("\n"))
        {
            // 가장 오래된 로그 메시지를 제거
            killLogText.text = killLogText.text.Substring(killLogText.text.IndexOf('\n') + 1);
        }
    }

    public void saveKillLogs(string killLog){
        if(currentSceneName == "JaechanFight" || currentSceneName == "TeamFight") return ;

        // Room propertis에서 현재의 킬 로그 가져오기
        
        string currentKillLog = string.Empty;
        if(PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey("killLog")){
            currentKillLog = (string)PhotonNetwork.CurrentRoom.CustomProperties["killLog"];
            Debug.Log("기존 로그 정보는 - " + currentKillLog);
        }

        // 새 킬 정보 추가
        string newKillLog = $"{currentKillLog}{killLog}\n";
        Debug.Log("로그 정보는 - " + newKillLog);

        // Room properties 업데이트
        var propertiesToUpdate = new ExitGames.Client.Photon.Hashtable();
        propertiesToUpdate["killLog"] = newKillLog;
        PhotonNetwork.CurrentRoom.SetCustomProperties(propertiesToUpdate);
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
    public void setLivingPerson(int livingPersonCount){
        livingPeopleText.text = "살아있는 사람 : " + livingPersonCount;
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