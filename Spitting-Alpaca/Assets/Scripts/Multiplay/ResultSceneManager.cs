using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ResultSceneManager : MonoBehaviourPunCallbacks
{
    public Transform waypoint;
    public Transform targetPosition;
    private Quaternion targetRotation = Quaternion.Euler(new Vector3(4, 0, 0));
    public float moveDuration = 3f;

    public GameObject platform;
    public GameObject[] objects;
    Animator _animator;

    public float rotationTime = 3f;
    public float smoothRotationTime = 1f;
    private Quaternion platformTargetRotation;
    public int rotateNumber = 3;

    public TextMeshProUGUI nicknameText;

    public Button mainButton;

    public Image[] titleArray;
    private string[] nicknameArray;
    public Image award;

    public Camera _camera;

    // CountDown
    private float countdownTimer;
    public Image[] countdowns;
    public Image countdownAlert;

    bool flag = true;

    // Start is called before the first frame update
    void Start()
    {
        if (platform == null)
        {
            platform = gameObject;  // ������ �������� �ʾҴٸ� �� ��ũ��Ʈ�� ���� ������Ʈ�� ���
        }

        if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey("killLog"))
        {

            var killLogObject = PhotonNetwork.CurrentRoom.CustomProperties["killLog"];

            Debug.Log(killLogObject);

            if (killLogObject is string killLogString)
            {
                string[] killLog = killLogString.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
                int i = 1;
                foreach (string kill in killLog)
                {
                    Debug.Log(i + " " + kill);
                    i++;
                }
                createResult(killLog);
            }
        }

        award.gameObject.SetActive(true);

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        platformTargetRotation = platform.transform.rotation;
        StartCoroutine(MoveCameraToPosition());
    }

    IEnumerator MoveCameraToPosition()
    {
        Vector3 startPosition = _camera.transform.position;
        Quaternion startRotation = _camera.transform.rotation;
        float totalDuration = moveDuration;
        float timeElapsed = 0f;

        while (timeElapsed < totalDuration)
        {
            float t = timeElapsed / totalDuration;
            // ��ü �̵� ��ο� ���� ����(t)�� ������� ��ġ�� ȸ���� ���
            if (t < 0.5f) // ��������Ʈ������ �̵�
            {
                _camera.transform.position = Vector3.Lerp(startPosition, waypoint.position, t * 2); // t*2�� ����Ͽ� ��ü �̵��� ���� ���� ��������Ʈ���� �̵�
            }
            else // ��������Ʈ���� ���� ��ǥ���������� �̵�
            {
                _camera.transform.position = Vector3.Lerp(waypoint.position, targetPosition.position, (t - 0.5f) * 2); // (t-0.5f)*2�� ����Ͽ� ������ ���� ���� ���� ��ǥ�������� �̵�
            }
            _camera.transform.rotation = Quaternion.Slerp(startRotation, targetRotation, t);
            timeElapsed += Time.deltaTime;
            yield return null;
        }

        _camera.transform.position = targetPosition.position;
        _camera.transform.rotation = targetRotation;

        OnMovementComplete();
    }


    void OnMovementComplete()
    {
        award.gameObject.SetActive(false);
        nicknameText.gameObject.SetActive(true);

        for (int i = 0; i < 4; i++)
        {
            if (nicknameArray[i] == "없음")
            {
                objects[i].gameObject.SetActive(false);
            }
        }

        StartCoroutine(RotateObjects());
    }

    IEnumerator RotateObjects()
    {
        int num = 0;
        // 3���� ȸ���� �� �ֵ���
        while (num < rotateNumber)
        {
            if (num != 0)
            {
                titleArray[num - 1].gameObject.SetActive(false);
            }

            // �ش� ����ī�� Ÿ��Ʋ, �г���, �ִϸ��̼� ����
            titleArray[num].gameObject.SetActive(true);
            if (nicknameArray[num] == "없음")
            {
                nicknameText.gameObject.SetActive(false);
            }
            else
            {
                nicknameText.gameObject.SetActive(true);
                nicknameText.text = nicknameArray[num];
            }

            _animator = objects[num].GetComponent<Animator>();
            _animator.Play("Spin", -1, 0f);

            yield return new WaitForSeconds(rotationTime - smoothRotationTime);
            UpdateTargetRotation();
            num++;
            yield return StartCoroutine(SmoothRotate());
        }
        // ������ ȸ���� Ÿ��Ʋ, �г��Ӹ� ����
        if (num == rotateNumber)
        {
            titleArray[num-1].gameObject.SetActive(false);
            // �ش� ����ī�� Ÿ��Ʋ, �г���, �ִϸ��̼� ����
            titleArray[num].gameObject.SetActive(true);
            nicknameText.text = nicknameArray[num];
            _animator = objects[num].GetComponent<Animator>();
            _animator.Play("Death", -1, 0f);

            yield return new WaitForSeconds(3);  // ������ ȸ�� �� 3�� ���
            titleArray[num].gameObject.SetActive(false);  // �ؽ�Ʈ �����
            nicknameText.gameObject.SetActive(false);  // �ؽ�Ʈ �����

            // ���콺 Ŀ���� Ȱ��ȭ�ϰ� ����� �����մϴ�.
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;

            PhotonNetwork.AutomaticallySyncScene = false;
            ExitGames.Client.Photon.Hashtable propsToRemove = new ExitGames.Client.Photon.Hashtable
            {
                { "isAlive", null } // Ű�� �ش��ϴ� ���� null�� �����Ͽ� ����
            };

            PhotonNetwork.LocalPlayer.SetCustomProperties(propsToRemove);

            countdownTimer = 6f;
            countdownAlert.gameObject.SetActive(true);
            StartCoroutine(CountDownCoroutine());
        }
    }

    IEnumerator CountDownCoroutine()
    {
        while (countdownTimer > 0f)
        {
            yield return new WaitForSeconds(1f);
            countdownTimer -= 1f;
            UpdateTimerUI((int)countdownTimer);
        }

        flag = true;

        /*PhotonNetwork.CurrentRoom.IsOpen = true;*/

        /*if (PhotonNetwork.IsMasterClient)
        {
            Debug.Log("�̵��մϴ�");
            
        }*/
        /*PhotonNetwork.LoadLevel("WaitingScene");*/
        PhotonNetwork.LeaveRoom();
    }

    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
    }

     public override void OnLeftRoom(){
        if(flag){
            SceneManager.LoadScene("preGameMap");
        }
    }


    void UpdateTimerUI(int timer)
    {
        Debug.Log("countdown" +  timer);
        if (timer == 5)
        {
            countdowns[timer - 1].gameObject.SetActive(true);
            countdowns[timer - 1].gameObject.GetComponent<Animator>().SetBool("isActive", true);
        }
        else if (timer == 0)
        {
            countdowns[timer].gameObject.SetActive(false);
            countdowns[timer].gameObject.GetComponent<Animator>().SetBool("isActive", false);
        }
        else
        {
            countdowns[timer].gameObject.SetActive(false);
            countdowns[timer].gameObject.GetComponent<Animator>().SetBool("isActive", false);
            countdowns[timer - 1].gameObject.SetActive(true);
            countdowns[timer - 1].gameObject.GetComponent<Animator>().SetBool("isActive", true);
        }
    }

    void UpdateTargetRotation()
    {
        float angle = 360f / objects.Length;  // �� ������Ʈ ���� ����
        platformTargetRotation *= Quaternion.Euler(0, angle, 0);  // ��ǥ ȸ�� ����
    }

    IEnumerator SmoothRotate()
    {
        float elapsedTime = 0;
        float totalRotationAmount = 360f / objects.Length;  // ��ü ȸ���ؾ� �� ����
        Vector3 rotationAxis = platform.transform.up;  // ������ ȸ�� ��

        while (elapsedTime < smoothRotationTime)
        {
            float frameRotationAmount = totalRotationAmount * (Time.deltaTime / smoothRotationTime);
            platform.transform.Rotate(rotationAxis, frameRotationAmount, Space.World);

            foreach (GameObject obj in objects)
            {
                // ��ü�� ������ ȸ���� �ݴ� �������� ȸ������ ���� ����(-133��)�� ����
                obj.transform.Rotate(rotationAxis, -frameRotationAmount, Space.World);
            }

            elapsedTime += Time.deltaTime;
            yield return null;
        }
        _animator.Play("Idle_A", -1, 0f);

        // ���� ��Ȯ�� ��ġ�� ���� ����
        platform.transform.rotation = platformTargetRotation;
    }

    private void createResult(string[] killLog)
    {

        Dictionary<string, KillStats> stats = new Dictionary<string, KillStats>();
        List<PlayerDeath> deaths = new List<PlayerDeath>();
        int time = 1;

        foreach (var log in killLog)
        {
            var parts = log.Split(" killed ");
            var killer = parts[0];
            var killed = parts[1];


            // 폭탄에 사람이 죽은 경우
            if(killer == "폭탄"){
                if(killed != "AI"){
                    deaths.Add(new PlayerDeath { KillerName = killer, PlayerName = killed, TimeOfDeath = time });    
                    time++;
                }
                continue;
            }
            
            // 배고파 죽은 경우
            if(killer == "Hungry"){
                deaths.Add(new PlayerDeath { KillerName = killer, PlayerName = killed, TimeOfDeath = time });    
                time++;
                continue;
            }

            // 마지막 살아 남은 사람의 정보를 저장하기 위해서 
            if(killer == killed){
                deaths.Add(new PlayerDeath { KillerName = killer, PlayerName = killed, TimeOfDeath = time });    
                time++;
                break;
            }

            // ���ο� killer ���� Ȯ��
            if (!stats.ContainsKey(killer))
            {
                stats[killer] = new KillStats { PlayerName = killer };
            }

            // AI�� ���� ���
            if (killed == "AI")
            {
                stats[killer].KilledAI++;
            }

            // Player�� ���� ���
            else
            {
                stats[killer].KilledPlayers++;
                Debug.Log("���� ��� - " + killer);
                Debug.Log("������ ���� ��� - " + killed);
                deaths.Add(new PlayerDeath { KillerName = killer, PlayerName = killed, TimeOfDeath = time });
                time++;
            }
        }
        var mostKilledAI = stats.OrderByDescending(s => s.Value.KilledAI).FirstOrDefault();
        var mostKilledPlayers = stats.OrderByDescending(s => s.Value.KilledPlayers).FirstOrDefault();
        var firstDeath = deaths.OrderBy(d => d.TimeOfDeath).FirstOrDefault();
        var lastKiller = deaths.OrderByDescending(d => d.TimeOfDeath).FirstOrDefault(d => d.KillerName != null);

        
        // 
        string swinner = lastKiller.KillerName;
        string smostKilledPlayers = (mostKilledPlayers.Value != null) ? mostKilledPlayers.Value.PlayerName : "없음";
        string smostKilledAI = (mostKilledAI.Value != null) ? mostKilledAI.Value.PlayerName : "없음";
        string sfirstDeath = firstDeath.PlayerName;

        Debug.Log("위너 - " + swinner);
        Debug.Log("가장 먼저 죽은"  + sfirstDeath);


        nicknameArray = new string[] { swinner,smostKilledPlayers, smostKilledAI, sfirstDeath};
    }


    public void clickGoToLobby()
    {
        Debug.Log("�κ�� ������!!!!!");
    }

    public void clickGoToWaitingRoom()
    {
        Debug.Log("���ο� ������ ������");
    }
}

/*public class KillStats
{
    public string PlayerName { get; set; }
    public int KilledAI { get; set; }
    public int KilledPlayers { get; set; }
}

public class PlayerDeath
{
    public string KillerName { get; set; }
    public string PlayerName { get; set; }
    public int TimeOfDeath { get; set; }
}*/