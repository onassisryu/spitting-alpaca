using System.Collections;
using UnityEngine;

public class AiScript : MonoBehaviour
{
    public float finalSpeed;
    public float speed = 3.5f;
    public float runSpeed = 5f;
    public float rotateSpeed = 3f;
    public float directionChangeInterval = 2f;
    public float jumpHeight = 2.3f;
    public float gravity = -9.81f;

    public bool isLive = true;
    public float reactionDelay = 1f;  // 반응 지연 시간 추가

    private Rigidbody rb;
    private Vector3 movement;
    private float nextDirectionChangeTime;
    private bool isMoving = true;
  
   
 
    

    private Animator animator;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
        nextDirectionChangeTime = Time.time + directionChangeInterval;
        ChangeDirection(); // reset direction
    }

    void Update()
    {
        if (!isLive)
        {
            isMoving = false;
            animator.SetBool("isWalking", false);
            animator.SetBool("isRunning", false);
            return;
        }
        if (Time.time >= nextDirectionChangeTime)
        {
            float actionChance = Random.value;
            if (actionChance <= 0.1) // 10%확률로 점프
            {
                animator.SetBool("isWalking", false);
                animator.SetBool("isRunning", false);
                Jump(); // 점프 실행
                
            }

            else if (actionChance <= 0.2) 
            {
                if (isMoving)
                {
                    isMoving = false;
                    animator.SetBool("isWalking", false);
                    animator.SetBool("isRunning", false);
                    nextDirectionChangeTime += Random.Range(0, 3f); // stop 0~3 seconds
                }
                else
                {
                    ChangeDirection();
                    isMoving = true;
                    animator.SetBool("isRunning", true);
                }
            }

            else if (actionChance <= 0.3)
            {
                Eat();
            }

            else
            {
                if (isMoving)
                {
                    isMoving = false;
                    animator.SetBool("isRunning", false);
                    animator.SetBool("isWalking", false);
                    nextDirectionChangeTime += Random.Range(0, 3f); // stop 0~3 seconds
                }
                else
                {
                    ChangeDirection();
                    isMoving = true;
                    animator.SetBool("isWalking", true);
                }
            }
            nextDirectionChangeTime += directionChangeInterval;
        }
    }

    void FixedUpdate()
    {
        if(!isMoving || !isLive)
        {
            return;
        }

        if(animator.GetBool("isWalking") == true)
        {
            Move();
        }
        else if (animator.GetBool("isRunning") == true)
        {
            Run();
            
        }
        
    }

    void ChangeDirection(Vector3? specificDirection = null)
    {
        // check specificDirection
        if (specificDirection.HasValue)
        {
            movement = specificDirection.Value.normalized;
        }
        else
        {
            float horizontalMove = Random.Range(-1f, 1f);
            float verticalMove = Random.Range(-1f, 1f);
            movement = new Vector3(horizontalMove, 0, verticalMove).normalized;
        }
    }

    void Move()
    {
        Vector3 movePosition = transform.position + movement * speed * Time.fixedDeltaTime;
        rb.MovePosition(movePosition);

        if (movement != Vector3.zero)
        {
            Quaternion newRotation = Quaternion.LookRotation(movement);
            rb.rotation = Quaternion.Slerp(rb.rotation, newRotation, rotateSpeed * Time.fixedDeltaTime);
        }
    }

    void Jump()
    {
        if (!animator.GetBool("isJumping")) // 이미 점프 중이 아닐 때만 점프 실행
        {
            rb.AddForce(Vector3.up * Mathf.Sqrt(jumpHeight * -2f * Physics.gravity.y), ForceMode.Impulse);
            animator.SetBool("isJumping", true);
        }

    }

    void Run()
    {
        Vector3 movePosition = transform.position + movement * runSpeed * Time.fixedDeltaTime;
        rb.MovePosition(movePosition);

        if (movement != Vector3.zero)
        {
            Quaternion newRotation = Quaternion.LookRotation(movement);
            rb.rotation = Quaternion.Slerp(rb.rotation, newRotation, rotateSpeed * Time.fixedDeltaTime);
        }
    }

    void Eat()
    {
        animator.Play("Eat", -1, 0f);
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, 1f);
        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.CompareTag("Grass"))
            {
                Destroy(hitCollider.gameObject);
            }
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.contacts[0].normal.y > 0.5) // 바닥과 충돌했다면
        {
            animator.SetBool("isJumping", false); // 점프 상태 해제
            animator.SetBool("isRunning", false);
        }
        else
        {
            StartCoroutine(DelayedDirectionChange()); // 코루틴을 사용하여 지연된 방향 변경 실행
        }
    }

    IEnumerator DelayedDirectionChange()
    {
        isMoving = false; // 이동 정지
        animator.SetBool("isWalking", false); // 걷는 애니메이션 비활성화
        animator.SetBool("isRunning", false);

        yield return new WaitForSeconds(reactionDelay); // reactionDelay 만큼 대기

        Vector3 newDirection = -movement; // 충돌 반대 방향으로 새 방향 결정
        ChangeDirection(newDirection);

        isMoving = true; // 이동 재개
        animator.SetBool("isWalking", true); // 걷는 애니메이션 활성화

        // 다음 방향 변경까지의 시간 설정
        nextDirectionChangeTime = Time.time + Random.Range(1f, 3f);
    }
}
