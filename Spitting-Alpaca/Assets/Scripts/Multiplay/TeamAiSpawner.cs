using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class TeamAiSpawner : MonoBehaviourPun
{
    public GameObject aiPrefab;
    public int aiCount = 10;

    void Start()
    {   
        if (PhotonNetwork.IsMasterClient && PhotonNetwork.CurrentRoom != null)
        {
            GameObject[] spawners = GameObject.FindGameObjectsWithTag("DefendSpawn");

            int defendAiCount = aiCount * PhotonNetwork.CurrentRoom.PlayerCount;
            int totalAiCount = Mathf.CeilToInt((float)defendAiCount / 2);


            if (spawners != null)
            {
                foreach(GameObject spawner in spawners){
                    Vector3 spawnerPosition = spawner.transform.position;

                    for (int i = 0; i < totalAiCount /spawners.Length ; i++)
                    {
                        Vector3 randomSpawnPos = spawnerPosition + (Random.insideUnitSphere * 10f);
                        randomSpawnPos.y = spawnerPosition.y;

                        PhotonNetwork.Instantiate(aiPrefab.name, randomSpawnPos, Quaternion.identity);
                    }
                }
            }
            else
            {
                for (int i = 0; i < totalAiCount; i++)
                {
                    Vector3 randomSpawnPos = Random.insideUnitSphere * 10f;
                    randomSpawnPos.y = 0f;

                    PhotonNetwork.Instantiate(aiPrefab.name, randomSpawnPos, Quaternion.identity);
                }
            }
        }
    }
}
