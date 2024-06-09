using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Wall : MonoBehaviour
{
    public float delayTIme = 30;
    private Transform target;
    public GameObject cube1;
    public GameObject cube2;
    public GameObject cube3;
    public GameObject cube4;
    public Vector3 centerPosition;// 중앙 좌표
    public float speed = 5.0f; // 이동 속도
    public float range = 2.0f;

    private bool shouldExcute = false;

    // Start is called before the first frame update
    void Start()
    {
        target = GameObject.Find("Center").transform;
        if (target != null)
        {
            centerPosition = target.transform.position;
            Invoke("StartUpdate", delayTIme);
        }
    }

    void StartUpdate(){
        shouldExcute = true;
    }

    void FixedUpdate()
    {
        if(!shouldExcute) return ;
        if(cube1) Move(cube1);
        if(cube2) Move(cube2);
        if(cube3) Move(cube3);
        if(cube4) Move(cube4);
    }

    void Move(GameObject gameObject){
        Vector3 gameObjectPosition = gameObject.transform.position;
        float distance = Vector3.Distance(gameObjectPosition, centerPosition);
    
        if(distance <= range) return ;
        gameObject.transform.position = Vector3.MoveTowards(gameObjectPosition, target.position, speed * Time.deltaTime);
    }
}
