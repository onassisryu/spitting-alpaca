using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class PlayerReady : MonoBehaviourPun, IPunObservable {
    public PhotonView pv;


    
    public bool isReady { get; set; } = false;

    private void Start(){
        pv = GetComponent<PhotonView>();
    }

    public void SetReady(bool readyState) {
        if (!photonView.IsMine) return;
        
        isReady = readyState;
        Player localPlayer = PhotonNetwork.LocalPlayer;
        Hashtable customProperties = localPlayer.CustomProperties;

       customProperties["isReady"] = readyState;
       localPlayer.SetCustomProperties(customProperties);

        photonView.RPC("UpdateReadyStateOnAllClients", RpcTarget.AllBuffered, isReady);
    }

    [PunRPC]
    public void UpdateReadyStateOnAllClients(bool readyState) {
        isReady = readyState;
        WaitingRoomManager.instance.CheckAllPlayersReady();
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {
        if (stream.IsWriting) {
            stream.SendNext(isReady);
        } else {
            isReady = (bool)stream.ReceiveNext();
        }
    }
}

