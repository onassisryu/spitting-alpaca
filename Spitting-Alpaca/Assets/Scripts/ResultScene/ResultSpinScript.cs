using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using System.Linq;

public class ResultSpinScript : MonoBehaviour
{
    public GameObject platform;  // 회전할 원판
    public GameObject[] objects;  // 원판 위의 오브젝트 배열
    Animator _animator;

    public float rotationTime = 3.0f;  // 회전 간격 시간
    public float smoothRotationTime = 1.0f;  // 부드러운 회전에 소요되는 시간
    private Quaternion targetRotation;  // 원판의 목표 회전
    public int rotateNumber = 3; // 원판이 돌 횟수

    public TextMeshProUGUI titleText; // 칭호 TMP
    public TextMeshProUGUI nicknameText; // 닉네임 TMP

    public Button mainBtn;  // 메인으로 버튼 참조
    public Button preGameBtn;  // 대기실로 버튼 참조

    private readonly string[] titleArray = {"가장 오래 살아남은 알파카!", "플레이어를 가장 많이 찾은 알파카!", "가장 많이 죽인 알파카!", "먼저 죽은 알파카!"}; // 타이틀 text 모음
    private string[] nicknameArray;

    void Start()
    {
        if (platform == null) {
            platform = gameObject;  // 원판이 지정되지 않았다면 이 스크립트가 붙은 오브젝트를 사용
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

        targetRotation = platform.transform.rotation;  // 초기 목표 회전 설정  
        StartCoroutine(RotateObjects());
    }

    IEnumerator RotateObjects()
    {   
        int num = 0;
        // 3번만 회전할 수 있도록
        while (num < rotateNumber)
        {
            // 해당 알파카의 타이틀, 닉네임, 애니메이션 설정
            titleText.text = titleArray[num];
            nicknameText.text = nicknameArray[num];
            _animator = objects[num].GetComponent<Animator>();
            _animator.Play("Spin", -1, 0f);

            yield return new WaitForSeconds(rotationTime - smoothRotationTime);
            UpdateTargetRotation();
            num++;
            yield return StartCoroutine(SmoothRotate());
        }
        // 마지막 회전시 타이틀, 닉네임만 변경
        if (num == rotateNumber) {
            // 해당 알파카의 타이틀, 닉네임, 애니메이션 설정
            titleText.text = titleArray[num];
            nicknameText.text = nicknameArray[num];
            _animator = objects[num].GetComponent<Animator>();
            _animator.Play("Death", -1, 0f);

            yield return new WaitForSeconds(3);  // 마지막 회전 후 3초 대기
            titleText.gameObject.SetActive(false);  // 텍스트 숨기기
            nicknameText.gameObject.SetActive(false);  // 텍스트 숨기기
            mainBtn.gameObject.SetActive(true);  // 버튼1 표시
            preGameBtn.gameObject.SetActive(true);  // 버튼2 표시

             // 마우스 커서를 활성화하고 잠금을 해제합니다.
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;


            ExitGames.Client.Photon.Hashtable propsToRemove = new ExitGames.Client.Photon.Hashtable
            {
                { "isAlive", null } // 키에 해당하는 값을 null로 설정하여 제거
            };

            PhotonNetwork.LocalPlayer.SetCustomProperties(propsToRemove);            
        }
    }

    void UpdateTargetRotation()
    {
        float angle = 360f / objects.Length;  // 각 오브젝트 간의 각도
        targetRotation *= Quaternion.Euler(0, angle, 0);  // 목표 회전 갱신
    }

    IEnumerator SmoothRotate()
    {
        float elapsedTime = 0;
        float totalRotationAmount = 360f / objects.Length;  // 전체 회전해야 할 각도
        Vector3 rotationAxis = platform.transform.up;  // 원판의 회전 축

        while (elapsedTime < smoothRotationTime)
        {
            float frameRotationAmount = totalRotationAmount * (Time.deltaTime / smoothRotationTime);
            platform.transform.Rotate(rotationAxis, frameRotationAmount, Space.World);

            foreach (GameObject obj in objects)
            {
                // 객체를 원판의 회전과 반대 방향으로 회전시켜 원래 방향(-133도)을 유지
                obj.transform.Rotate(rotationAxis, -frameRotationAmount, Space.World);
            }

            elapsedTime += Time.deltaTime;
            yield return null;
        }
        _animator.Play("Idle_A", -1, 0f);

        // 최종 정확한 위치와 각도 보정
        platform.transform.rotation = targetRotation;
    }

    private void createResult(string [] killLog){
        
        Dictionary<string, KillStats> stats = new Dictionary<string, KillStats>();
        List<PlayerDeath> deaths = new List<PlayerDeath>();
        int time = 1;

        foreach(var log in killLog){
            var parts = log.Split(" killed ");
            var killer = parts[0];
            var killed  = parts[1];

            // 새로운 killer 인지 확인
            if (!stats.ContainsKey(killer)){
                stats[killer] = new KillStats { PlayerName = killer };
            }

            // AI를 죽인 경우
            if (killed == "AI"){
                stats[killer].KilledAI++;
            }

            // Player를 죽인 경우
            else{
                stats[killer].KilledPlayers++;
                Debug.Log("죽인 사람 - " + killer);
                Debug.Log("죽임을 당한 사람 - " + killed);
                deaths.Add(new PlayerDeath { KillerName = killer, PlayerName = killed, TimeOfDeath = time });
                time++;
            }
        }
            Debug.Log("list의 크기는 = " + deaths.Count);
            var mostKilledAI = stats.OrderByDescending(s => s.Value.KilledAI).FirstOrDefault();
            var mostKilledPlayers = stats.OrderByDescending(s => s.Value.KilledPlayers).FirstOrDefault();
            var firstDeath = deaths.OrderBy(d => d.TimeOfDeath).FirstOrDefault();
            var lastKiller = deaths.OrderByDescending(d => d.TimeOfDeath).FirstOrDefault(d => d.KillerName != null);


            Debug.Log("AI를 가장 많이 죽인 사람 - " + mostKilledAI);
            Debug.Log("플레이어를 가장 많이 죽인 사람 - " + mostKilledPlayers);
            Debug.Log("가장 먼저 죽인 사람 - " + (firstDeath != null ? firstDeath.PlayerName : "없음"));
            Debug.Log("생존왕 - " + (lastKiller != null ? lastKiller.KillerName : "없음"));

        
            nicknameArray = new string [] {lastKiller.KillerName , mostKilledPlayers.Value.PlayerName, mostKilledAI.Value.PlayerName, firstDeath.PlayerName };

    }
}

public class KillStats
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
}
