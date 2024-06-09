using System.Collections.Generic;
using UnityEngine;

public class DT_Tornado : MonoBehaviour
{
    [Tooltip("Distance after which the rotation physics starts")]
    public float maxDistance = 5;

    [Tooltip("The axis that the caught objects will rotate around")]
    public Vector3 rotationAxis = new Vector3(0, 1, 0);

    [Tooltip("Angle that is added to the object's velocity (higher lift -> quicker on top)")]
    [Range(0, 90)]
    public float lift = 45;

    [Tooltip("The force that will drive the caught objects around the tornado's center")]
    public float rotationStrength = 50;

    [Tooltip("Tornado pull force")]
    public float tornadoStrength = 2;

    Rigidbody r;

    List<DT_Caught> caughtObject = new List<DT_Caught>();

    // Start is called before the first frame update
    void Start()
    {
        //Normalize the rotation axis given by the user
        rotationAxis.Normalize();

        r = GetComponent<Rigidbody>();
        r.isKinematic = true;
        Destroy(gameObject, 5f);
    }

    void FixedUpdate()
    {
        //Apply force to caught objects
        for (int i = 0; i < caughtObject.Count; i++)
        {
            if (caughtObject[i] != null)
            {
                Vector3 pull = transform.position - caughtObject[i].transform.position;
                if (pull.magnitude > maxDistance)
                {
                    caughtObject[i].rigid.AddForce(pull.normalized * pull.magnitude, ForceMode.Force);
                    caughtObject[i].enabled = false;
                }
                else
                {
                    caughtObject[i].enabled = true;
                }
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && other.gameObject.layer == LayerMask.NameToLayer("Ghost"))
        {
            return;
        }

        if (!other.attachedRigidbody) return;
        if (other.attachedRigidbody.isKinematic) return;

        //Add caught object to the list
        DT_Caught caught = other.GetComponent<DT_Caught>();
        if (!caught)
        {
            caught = other.gameObject.AddComponent<DT_Caught>();
        }

        caught.Init(this, r, tornadoStrength);

        if (!caughtObject.Contains(caught))
        {
            caughtObject.Add(caught);

            if (other.CompareTag("Player"))
            {
                PlayerMovementTest player = other.GetComponent<PlayerMovementTest>();
                if (player != null)
                {
                    player.isCaughtedByTornado = true;
                }
            }

            else if (other.CompareTag("Ai"))
            {
                Ai ai = other.GetComponent<Ai>();
                if (ai != null)
                {
                    ai.isCaughtedByTornado = true;
                }
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        //Release caught object
        DT_Caught caught = other.GetComponent<DT_Caught>();
        if (caught)
        {
            caught.Release();

            if (caughtObject.Contains(caught))
            {
                caughtObject.Remove(caught);

                if (other.CompareTag("Player"))
                {
                    PlayerMovementTest player = other.GetComponent<PlayerMovementTest>();
                    if (player != null)
                    {
                        player.isCaughtedByTornado = false;
                    }
                }
                else if (other.CompareTag("Ai"))
                {
                    Ai ai = other.GetComponent<Ai>();
                    if (ai != null)
                    {
                        ai.isCaughtedByTornado = false;
                    }
                }
            }
        }
    }

    public float GetStrength()
    {
        return rotationStrength;
    }

    //The axis the caught objects rotate around
    public Vector3 GetRotationAxis()
    {
        return rotationAxis;
    }

    //Draw tornado radius circle in Editor
    void OnDrawGizmosSelected()
    {
        Vector3[] positions = new Vector3[30];
        Vector3 centrePos = transform.position;
        for (int pointNum = 0; pointNum < positions.Length; pointNum++)
        {
            // "i" now represents the progress around the circle from 0-1
            // we multiply by 1.0 to ensure we get a fraction as a result.
            float i = (float)(pointNum * 2) / positions.Length;

            // get the angle for this step (in radians, not degrees)
            float angle = i * Mathf.PI * 2;

            // the X & Y position for this angle are calculated using Sin & Cos
            float x = Mathf.Sin(angle) * maxDistance;
            float z = Mathf.Cos(angle) * maxDistance;

            Vector3 pos = new Vector3(x, 0, z) + centrePos;
            positions[pointNum] = pos;
        }

        Gizmos.color = Color.cyan;
        for (int i = 0; i < positions.Length; i++)
        {
            if (i == positions.Length - 1)
            {
                Gizmos.DrawLine(positions[0], positions[positions.Length - 1]);
            }
            else
            {
                Gizmos.DrawLine(positions[i], positions[i + 1]);
            }
        }
    }

    void OnDestroy()
    {
        // 토네이도가 파괴되기 전에 실행할 코드
        foreach (DT_Caught caught in caughtObject)
        {
            if (caught != null && caught.gameObject != null)
            {
                // 플레이어 태그 확인 후 isCaughtedByTornado 해제
                if (caught.gameObject.CompareTag("Player"))
                {
                    PlayerMovementTest player = caught.gameObject.GetComponent<PlayerMovementTest>();
                    if (player != null)
                    {
                        player.isCaughtedByTornado = false;
                    }
                }

                else if (caught.gameObject.CompareTag("Ai"))
                {
                    Ai ai = caught.gameObject.GetComponent<Ai>();
                    if (ai != null)
                    {
                        ai.isCaughtedByTornado = false;
                    }
                }
            }
        }
    }
}
