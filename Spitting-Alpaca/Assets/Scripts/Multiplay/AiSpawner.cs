using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class AiSpawner : MonoBehaviourPun
{
    public GameObject aiPrefab;
    public int aiCount = 4;

    void Start()
    {   
        if (PhotonNetwork.IsMasterClient && PhotonNetwork.CurrentRoom != null)
        {
            GameObject[] spawners = GameObject.FindGameObjectsWithTag("Spawn");

            int totalAiCount = aiCount * PhotonNetwork.CurrentRoom.PlayerCount;

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
