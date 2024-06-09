using UnityEngine;

public class FollowTarget : MonoBehaviour
{
    private Transform target;
    private float lifeTime = 10f; // 이펙트의 수명 (10초)

    void Start()
    {
        // 10초 후에 Destroy 호출
        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        if (target != null)
        {
            transform.position = target.position;
        }
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }
}
