
using System.Collections;
using UnityEngine;
using Photon.Pun;
using UnityEngine.AI;

public class ItemSpawner : MonoBehaviourPun
{
    public GameObject[] items;

    /*private float maxDistance = 5f;*/

    private float timeBetSpawnMax = 15f;
    private float timeBetSpawnMin = 5f;

    private float timeBetSpawn;
    private float lastSpawnTime;

    private float lastBombSpawnTime;
    private float lastItemSpawnTime;
    private GameObject itemSpawn;
    Vector3 spawnPosition;


    private void Start()
    {
        itemSpawn = GameObject.FindGameObjectWithTag("ItemSpawn");
        if (itemSpawn != null)
        {
            spawnPosition = itemSpawn.transform.position;
        }
        lastBombSpawnTime = Time.time;
        lastItemSpawnTime = Time.time;
    }

    private void Update()
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            return;
        }

        if (Time.time >= lastBombSpawnTime || Time.time >= lastItemSpawnTime)
        {
            GameObject itemToCreate = items[Random.Range(0, items.Length)];

            if (itemToCreate.tag == "Bomb" && Time.time >= lastBombSpawnTime)
            {
                lastBombSpawnTime = Time.time + Random.Range(30, 40); // 폭탄은 30~40초 사이로 생성
                Spawn(itemToCreate);
            }
            else if (itemToCreate.tag != "Bomb" && Time.time >= lastItemSpawnTime)
            {
                lastItemSpawnTime = Time.time + Random.Range(5, 10); // 비폭탄 아이템은 5~10초 사이로 생성
                Spawn(itemToCreate);
            }
        }
    }

    private void Spawn(GameObject itemToCreate)
    {

        if (itemSpawn != null)
        {
            Vector3 randomSpawnPos = spawnPosition + (Random.insideUnitSphere * 10f);
            randomSpawnPos.y = spawnPosition.y;

            GameObject item = PhotonNetwork.Instantiate(itemToCreate.name, randomSpawnPos, Quaternion.identity);

            StartCoroutine(DestroyAfter(item, 10f));

        }
        else
        {
            Vector3 randomSpawnPos = Random.insideUnitSphere * 10f;
            randomSpawnPos.y = 0f;

            if (itemToCreate.tag == "Bomb")
            {
                randomSpawnPos.y = 0f;
            }

            GameObject item = PhotonNetwork.Instantiate(itemToCreate.name, randomSpawnPos, Quaternion.identity);

            StartCoroutine(DestroyAfter(item, 10f));
        }


    }

    IEnumerator DestroyAfter(GameObject target, float delay)
    {
        yield return new WaitForSeconds(delay);

        if (target != null)
        {
            PhotonNetwork.Destroy(target);
        }
    }

    /*    private Vector3 GetRandomPointOnNavMesh(Vector3 center, float distance)
        {
            Vector3 randomPos = Random.insideUnitSphere * distance + center;

            NavMeshHit hit;

            NavMesh.SamplePosition(randomPos, out hit, distance, NavMesh.AllAreas);

            return hit.position;
        }*/
}