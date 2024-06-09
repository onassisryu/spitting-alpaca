using UnityEngine;
using Photon.Pun;

public class NewGrassSpawner : MonoBehaviourPun
{
    public GameObject[] grassPrefabs; // list of grass prefabs
    public float spawnInterval = 4f;
    public float yOffset = 0.8f;
    public int initialGrassCount = 12;
    public int maxGrassCount = 24; // maximum of grass
    private float timer = 0f;

    private GameObject spawn;
    Vector3 spawnPosition;

    private void Start()
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            return;
        }

        spawn = GameObject.FindGameObjectWithTag("Spawn");
        if (spawn != null )
        {
            spawnPosition = spawn.transform.position;
        }

        for (int i = 0; i < initialGrassCount; i++)
        {
            SpawnGrassOnGround();
        }
    }

    private void Update()
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            return;
        }

        timer += Time.deltaTime;

        if (timer >= spawnInterval)
        {
            if (CountGrass() < maxGrassCount)
            {
                SpawnGrassOnGround();
            }
            timer = 0f;
        }
    }

    void SpawnGrassOnGround()
    {
        if (spawn != null)
        {
            Vector3 randomSpawnPos = spawnPosition + (Random.insideUnitSphere * 18f);
            randomSpawnPos.y = spawnPosition.y - 0.4f;

            int randomGrassIndex = Random.Range(0, grassPrefabs.Length);
            GameObject selectedGrassPrefab = grassPrefabs[randomGrassIndex];

            Collider[] colliders = Physics.OverlapSphere(randomSpawnPos, selectedGrassPrefab.transform.localScale.x / 2f);
            bool hasNonGroundObject = false;
            foreach (Collider collider in colliders)
            {
                if (collider.gameObject.tag != "Ground")
                {
                    hasNonGroundObject = true;
                    break;
                }
            }

            if (!hasNonGroundObject)
            {
                PhotonNetwork.Instantiate(selectedGrassPrefab.name, randomSpawnPos, Quaternion.identity);
            }
            else
            {
                SpawnGrassOnGround();
            }
        }

        /*GameObject[] grounds = GameObject.FindGameObjectsWithTag("Ground");
        if (grounds.Length > 0)
        {
            int randomIndex = Random.Range(0, grounds.Length);
            GameObject selectedGround = grounds[randomIndex];
            Collider groundCollider = selectedGround.GetComponent<MeshCollider>();

            if (groundCollider != null)
            {
                Vector3 spawnPosition = new Vector3(
                    Random.Range(groundCollider.bounds.min.x, groundCollider.bounds.max.x),
                    groundCollider.bounds.max.y - yOffset,
                    Random.Range(groundCollider.bounds.min.z, groundCollider.bounds.max.z)
                );

                // select random grass prefab
                int randomGrassIndex = Random.Range(0, grassPrefabs.Length);
                GameObject selectedGrassPrefab = grassPrefabs[randomGrassIndex];

                // create new grass prefab
                PhotonNetwork.Instantiate(selectedGrassPrefab.name, spawnPosition, Quaternion.identity);
            }
        }*/
    }

    int CountGrass()
    {
        return GameObject.FindGameObjectsWithTag("Grass").Length;
    }
}
