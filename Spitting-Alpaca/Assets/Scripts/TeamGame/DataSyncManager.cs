using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class DataSyncManager : MonoBehaviourPun
{
    public SceneData sceneData;

    void Start()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            InitializeSceneData();
        }
    }

    public void InitializeSceneData()
    {
        if(!PhotonNetwork.IsMasterClient) return ;
        sceneData = new SceneData
        {
            Attack = new Team { TeamId = 1, TeamName = "Attackers" },
            Defend = new Team { TeamId = 2, TeamName = "Defenders" }
        };
        SyncData();
    }

    public void SyncData()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            photonView.RPC("SyncSceneData", RpcTarget.Others, JsonUtility.ToJson(sceneData));
        }
    }

    [PunRPC]
    void SyncSceneData(string json)
    {
        sceneData = JsonUtility.FromJson<SceneData>(json);
        Debug.Log("SceneData 동기화 완료");
    }

    public Team GetAttackTeam(){
        return sceneData.Attack;
    }

    public Team GetDefendTeam(){
        return sceneData.Defend;
    }

    public void setKill(string killer, string defender){
        sceneData.Kill(killer, defender);
        SyncData();
    }

    public void starvation(Player player){
        photonView.RPC("starvationRPC",RpcTarget.MasterClient, player);
    }
    [PunRPC]
    public void starvationRPC(Player player){
        if(PhotonNetwork.IsMasterClient){
            sceneData.Hungry(player);
            SyncData();
        }
    }
}
