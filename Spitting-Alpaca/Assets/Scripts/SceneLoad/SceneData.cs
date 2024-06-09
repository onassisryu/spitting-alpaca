// using System;
// using System.Collections.Generic;
// using System.Data.Common;
// using Photon.Realtime;
// using UnityEngine;
// [Serializable]
// public class SceneData {
//     private Team attack;
//     private Team defend;

//     // attack의 게터와 세터
//     public Team Attack {
//         get { return attack; }
//         set { attack = value; }
//     }

//     // defend의 게터와 세터
//     public Team Defend {
//         get { return defend; }
//         set { defend = value; }
//     }

//     Dictionary<string,  List<string>> killLog = new Dictionary<string, List<string>>();
    
//     public void kill(string killer, string defender){
//         write(killer,defender);
//         defend.AliveCount -=1;
//         Debug.Log($"{killer} - ${defender}");
//         Debug.Log($"살아남은 수 : {defend.AliveCount}");

//     }
    
//     private void write(string killer, string defender){
//          if (!killLog.ContainsKey(killer))
//         {
//             killLog[killer] = new List<string>();
//         }
//         killLog[killer].Add(defender);
//     }
//     public void hungry(string nickname){
//         defend.AliveCount -=1;
//         Debug.Log($"배고파 죽은 알파카 이름 : " + nickname);
//     }

   
//     public void LeftPlayer(Player player){
//         // attack이 나갔을 경우 
//         int left = attack.Players.IndexOf(player);
//         if(left != -1 ){
//             Debug.Log("attack player이 방을 떠납니다.");   
//             attack.AliveCount -= 1;
//             return;
//         }
        
//         left = defend.Players.IndexOf(player);
//         if(left != -1){
//             Debug.Log("defend player이 방을 떠납니다.");
//             foreach (var key in killLog.Keys)
//             {
//                 // 죽었는지 확인
//                 int index = killLog[key].IndexOf(player.NickName);

//                 // 인덱스가 있으면 죽었다는 것.
//                 // -1 == 플레이어가 살아있다.
//                 // -1 != 플레이어가 죽었다. 
//                 if (index != -1) 
//                 {
//                     defend.AliveCount-=1;
//                 }
//             }
//         }
//     }

//     public bool isGameover(){
//         return attack.AliveCount == 0 || defend.AliveCount == 0;
//     }
//     public string winTeamName(){
//         return attack.AliveCount == 0 ? defend.TeamName : attack.TeamName;
//     }

//     public List<Player> getWinTeam(){
//         return attack.AliveCount == 0 ? defend.Players : attack.Players;
//     }
//     public List<Player> getLoseTeam(){
//         return attack.AliveCount == 0 ? attack.Players : defend.Players;
//     }
// }
using System;
using System.Collections.Generic;
using Photon.Realtime;
using UnityEngine;

[Serializable]
public class SceneData
{
    public Team attack;
    public Team defend;

    public Team Attack
    {
        get { return attack; }
        set { attack = value; }
    }

    public Team Defend
    {
        get { return defend; }
        set { defend = value; }
    }

    [Serializable]
    public class KillLogEntry
    {
        public string Killer;
        public string Defender;
    }

    public List<KillLogEntry> killLogEntries = new List<KillLogEntry>();

    public void Kill(string killer, string defender)
    {
        WriteKillLog(killer, defender);
        if(defender != "AI"){
            defend.AliveCount -= 1;
        }

        Debug.Log($"{killer} - {defender}");
        Debug.Log($"살아남은 수: {defend.AliveCount}");
        log();
    }

    private void WriteKillLog(string killer, string defender)
    {
        killLogEntries.Add(new KillLogEntry { Killer = killer, Defender = defender });
    }

    public void Hungry(Player player)
    {
        defend.AliveCount -= 1;
         WriteKillLog("배고픔", player.NickName);
        Debug.Log($"배고파 죽은 알파카 이름: {player.NickName}");
        log();

    }

    public void LeftPlayer(Player player)
    {
        if (attack.Players.Contains(player))
        {
            attack.AliveCount -= 1;
            Debug.Log($"{player.NickName}이(가) 공격 팀을 떠났습니다.");
        }
        else if (defend.Players.Contains(player))
        {
            bool isDead = false;

            foreach (var entry in killLogEntries)
            {
                if (entry.Defender == player.NickName)
                {
                    isDead = true;
                    break;
                }
            }

            if (!isDead)
            {
                defend.AliveCount -= 1;
            }

            Debug.Log($"{player.NickName}이(가) 방어 팀을 떠났습니다.");
        }

       log();
        
    }

    private void log(){
        Debug.Log($"{attack.TeamName} 남은 플레이어: {attack.AliveCount}");
        Debug.Log($"{defend.TeamName} 남은 플레이어: {defend.AliveCount}");
        Debug.Log($"gameOver : {IsGameOver()}");

    }

    public bool IsGameOver()
    {
        return attack.AliveCount == 0 || defend.AliveCount == 0;
    }

    public string WinTeamName()
    {
        return attack.AliveCount == 0 ? defend.TeamName : attack.TeamName;
    }

    public List<Player> GetWinTeam()
    {
        return attack.AliveCount == 0 ? defend.Players : attack.Players;
    }

    public List<Player> GetLoseTeam()
    {
        return attack.AliveCount == 0 ? attack.Players : defend.Players;
    }
}
