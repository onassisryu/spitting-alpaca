// using System;
// using System.Collections.Generic;
// using Photon.Realtime;
// using UnityEngine;


// [Serializable]
// public class Team {
//     private int teamId;
//     private string teamName;
//     private List<Player> players = new List<Player>();
//     private int aliveCount = 0;

//     // teamId의 게터와 세터
//     public int TeamId {
//         get { return teamId; }
//         set { teamId = value; }
//     }

//     // teamName의 게터와 세터
//     public string TeamName {
//         get { return teamName; }
//         set { teamName = value; }
//     }

//     // players 배열의 게터와 세터
//     public List<Player> Players {
//         get { return players; }
//     }

//     public void PlayerAdd(Player player){
//         this.players.Add(player);
//         this.aliveCount = this.players.Count;
//         Debug.Log($"{TeamName}팀 플레이어 추가 : {player.NickName}");
//         Debug.Log($"총 인원: {this.AliveCount}");

//     }

//     public int AliveCount{
//         get { return aliveCount; }
//         set { aliveCount = value; }
//     }
// }

using System;
using System.Collections.Generic;
using Photon.Realtime;
using UnityEngine;

[Serializable]
public class Team
{
    public int teamId;
    public string teamName;
    public List<Player> players = new List<Player>();
    public int aliveCount = 0;

    public int TeamId
    {
        get { return teamId; }
        set { teamId = value; }
    }

    public string TeamName
    {
        get { return teamName; }
        set { teamName = value; }
    }

    public List<Player> Players
    {
        get { return players; }
    }

    public void PlayerAdd(Player player)
    {
        this.players.Add(player);
        this.aliveCount = this.players.Count;
        Debug.Log($"{TeamName} 팀 플레이어 추가: {player.NickName}");
        Debug.Log($"총 인원: {this.AliveCount}");
    }

    public int AliveCount
    {
        get { return aliveCount; }
        set { aliveCount = value; }
    }
}