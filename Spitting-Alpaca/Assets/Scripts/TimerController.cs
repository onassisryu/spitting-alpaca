using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System;

public class TimerController : MonoBehaviour
{
    
    Timer timer;
    Action action {get; set;}
    void Awake(){

    }

    void Start(){
        timer = GetComponentInChildren<Timer>();
        
    }

    void Update(){
        
        if(!PhotonNetwork.IsMasterClient) return ;

        timer.time -= Time.deltaTime;
        if(timer != null && timer.time <= 0){
            Execute();
        }
    }

    void Execute(){
        if(action == null) return ;
        action();
    }
}
