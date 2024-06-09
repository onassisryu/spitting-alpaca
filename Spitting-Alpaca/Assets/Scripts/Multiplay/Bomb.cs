using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Bomb : MonoBehaviourPun
{
    public GameObject[] bombPrefabs; // list of prefabs
    public GameObject explosionPrefab; // explosion prefab
    public float explosionRadius = 1.5f; // ���߹ݰ� 
    public float spawnInterval = 0.1f;
    public float yOffset = -3.5f;
    public int initialBombCount = 6;
    public int maxBombCount = 16;
    private float timer = 0f;

    // Start is called before the first frame update
    void Start()
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            return;
        }

        if (bombPrefabs == null || bombPrefabs.Length == 0)
        {
            return;
        }


        for (int i = 0; i < initialBombCount; i++)
        {
            SpawnBombOnGround();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            return;
        }

        timer += Time.deltaTime;

        if (timer >= spawnInterval)
        {
            if (CountBomb() < maxBombCount)
            {
                SpawnBombOnGround();
            }
            timer = 0f;
        }
    }

    void SpawnBombOnGround()
    {
        GameObject[] grounds = GameObject.FindGameObjectsWithTag("Ground");
        if (grounds.Length == 0)
        {
            return;
        }

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

                if (bombPrefabs.Length == 0)
                {
                    return;
                }

                // select random grass prefab
                int randomBombIndex = Random.Range(0, bombPrefabs.Length);
                GameObject selectedBombPrefab = bombPrefabs[randomBombIndex];

                // create new grass prefab
                PhotonNetwork.Instantiate(selectedBombPrefab.name, spawnPosition, Quaternion.identity);
            }
        }
    }

    int CountBomb()
    {
        return GameObject.FindGameObjectsWithTag("Bomb").Length;
    }


    void OnDestroy()
    {
        if (!photonView.IsMine)
            return;

        // instantiate explosion at the same location as the bomb
        Vector3 explosionPosition = transform.position; // Use the bomb's current position
        PhotonNetwork.Instantiate(explosionPrefab.name, explosionPosition, Quaternion.identity);

        // Destroy nearby players and AI
        Collider[] affectedObjects = Physics.OverlapSphere(explosionPosition, explosionRadius);
        foreach (Collider affectedObject in affectedObjects)
        {
            GameObject targetObject = affectedObject.gameObject;
            PhotonView targetPhotonView = targetObject.GetComponent<PhotonView>();

            if (targetObject.CompareTag("Player"))
            {
                IDamageable damageable = targetObject.GetComponent<IDamageable>();
                if (damageable != null && targetPhotonView != null)
                {
                    string victimName = targetPhotonView.Owner.NickName;
                    UIManager.Instance.createKillLog("폭탄", victimName);

                    damageable.OnDamage();
                }
            }
            else if (targetObject.CompareTag("Ai"))
            {
                IDamageable damageable = targetObject.GetComponent<IDamageable>();
                if (damageable != null && targetPhotonView != null)
                {
                    string victimName = "AI";
                    UIManager.Instance.createKillLog("폭탄", victimName);

                    damageable.OnDamage();
                }
            }

        }
    }
}
