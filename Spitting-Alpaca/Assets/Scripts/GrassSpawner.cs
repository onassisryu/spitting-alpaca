using UnityEngine;

public class GrassSpawner : MonoBehaviour
{
    public GameObject[] grassPrefabs; // list of grass prefabs
    public float spawnInterval = 5f;
    public float yOffset = 1f;
    public float maxGrassCount = 10; // maximum of grass
    private float timer = 0f;

    private void Update()
    {
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
        GameObject[] grounds = GameObject.FindGameObjectsWithTag("Ground");
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
                Instantiate(selectedGrassPrefab, spawnPosition, Quaternion.identity);
            }
        }
    }

    int CountGrass()
    {
        return GameObject.FindGameObjectsWithTag("Grass").Length;
    }
}
