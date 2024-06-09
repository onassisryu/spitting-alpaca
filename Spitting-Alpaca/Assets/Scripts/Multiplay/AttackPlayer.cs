
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;

public class AttackPlayer : LivingEntity
{
    Animator _animator;
    Camera _camera;
    Rigidbody _rigidbody;

    public float speed = 3.5f;
    public float runSpeed = 8f;
    public float finalSpeed;
    public bool toggleCameraRotation;
    public bool run;
    public float jumpHeight = 0.7f;
    public float rotateSpeed = 3f;

    private Vector3 _velocity;
    private bool isGrounded;
    private bool isJumping;

    public GameObject spitPrefab;
    private float spitForce = 8f;

    private float nextAttackTime = 0f;
    public float attackCoolTime = 2f;

    public bool isAlive = true;
    public GameObject followCam;

    public bool isStunned = false;
    private float stunStartTime;
    private float stunDuration = 4f;

    Vector3 movement;

    public GameObject tornadoPrefab;
    public bool isCaughtedByTornado = false;

    void Start()
    {
        _animator = this.GetComponent<Animator>();
        if (_animator == null)
        {
            Debug.LogError("Animator component is not attached to " + gameObject.name);
        }
        _camera = Camera.main;
        _rigidbody = this.GetComponent<Rigidbody>();
        isJumping = false;

        if (photonView.IsMine)
        {
            _camera.cullingMask = ~(1 << 10);
        }
    }

    void Update()
    {
        if (photonView.OwnerActorNr != photonView.CreatorActorNr)
        {
            PhotonNetwork.Destroy(gameObject);
        }

        if (!photonView.IsMine || !isAlive || UIManager.Instance.menuActiveSelf)
        {
            return;
        }

        string currentScene = SceneManager.GetActiveScene().name;

        if (currentScene != "WaitingScene")
        {
            _camera.cullingMask = ~(1 << 10 | 1 << 11);
        }

        if (UIManager.Instance != null)
        {
            float cooltimeLeft = nextAttackTime - Time.time;
            float stuntimeLeft = stunStartTime + stunDuration - Time.time;

            float adjustedCooltime = Mathf.Max(0, attackCoolTime - cooltimeLeft); // ���� ����
            UIManager.Instance.UpdateCooltimeText(adjustedCooltime, attackCoolTime); // ������ �κ�
            if (isStunned)
            {
                UIManager.Instance.UpdateStunStatus(stuntimeLeft);
            }
            else
            {
                UIManager.Instance.UpdateStunStatus(0f);
            }
        }

        if (isStunned)
        {
            _animator.SetBool("isStunned", true);

            if (Time.time - stunStartTime >= stunDuration)
            {
                isStunned = false;
                _animator.SetBool("isStunned", false);
                UIManager.Instance.SetStun(false);
            }

            return;
        }

        if (isCaughtedByTornado == true)  
        {
            _animator.SetBool("isSpinned", true);  
        }
        else
        {
            _animator.SetBool("isSpinned", false); 
        }

        // jump
        if (Input.GetKey(KeyCode.Space) && isJumping == false && _animator.GetBool("isJumping") == false)
        {
            Jump();
        }

        // spit
        if (Input.GetMouseButtonDown(0))
        {
            Ready();
        }

    }

    void FixedUpdate()
    {
        if (photonView.IsMine && !isStunned && isAlive && !UIManager.Instance.menuActiveSelf)
        {
            ProcessInputs();
            isGrounded = Physics.Raycast(transform.position, Vector3.down, 1.1f); 
        }
    }
    //
    void ProcessInputs()
    {
        if (Input.GetKey(KeyCode.LeftShift))
        {
            run = true;
        }
        else
        {
            run = false;
        }

        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            _rigidbody.AddForce(Vector3.up * Mathf.Sqrt(jumpHeight * -2f * Physics.gravity.y), ForceMode.Impulse);
        }

        InputMovement();
    }

    void InputMovement()
    {
        finalSpeed = (run) ? runSpeed : speed;

        Vector3 cameraForward = Camera.main.transform.forward;
        Vector3 cameraRight = Camera.main.transform.right;
        cameraForward.y = 0;
        cameraRight.y = 0;
        cameraForward.Normalize();
        cameraRight.Normalize();

        float horizontalInput = Input.GetAxisRaw("Horizontal");
        float verticalInput = Input.GetAxisRaw("Vertical");
        Vector3 inputDirection = (cameraRight * horizontalInput + cameraForward * verticalInput).normalized;

        if (inputDirection != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(inputDirection);
            _rigidbody.MoveRotation(Quaternion.Slerp(transform.rotation, targetRotation, rotateSpeed * Time.fixedDeltaTime));

            _velocity = inputDirection * finalSpeed;
            _rigidbody.MovePosition(_rigidbody.position + _velocity * Time.fixedDeltaTime);
            _animator.SetFloat("Blend", run ? 5.0f : 0.1f, 0, Time.fixedDeltaTime);
        }
        else
        {
            _rigidbody.angularVelocity = Vector3.zero;
            _velocity = Vector3.zero;
            _animator.SetFloat("Blend", 0, 0.1f, Time.fixedDeltaTime);
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        CheckGroundStatus(collision);
    }

    void OnCollisionStay(Collision collision)
    {
        CheckGroundStatus(collision);
    }

    void CheckGroundStatus(Collision collision)
    {
        if (collision.contacts.Length > 0 && collision.contacts[0].normal.y > 0.5)
        {
            if (photonView.IsMine)
            {
                isGrounded = true;
                _animator.SetBool("isJumping", false);
            }
        }
        else
        {
            isGrounded = false; 
        }
    }

    void Jump()
    {
        if (isGrounded && _animator.GetBool("isJumping") == false)
        {
            _rigidbody.AddForce(Vector3.up * Mathf.Sqrt(jumpHeight * -1f * Physics.gravity.y), ForceMode.Impulse);
            _animator.SetBool("isJumping", true);
        }


    }

    public void Ready()
    {
        if (Time.time >= nextAttackTime)
        {
            Shot();
            nextAttackTime = Time.time + attackCoolTime;
        }
    }

    private void Shot()
    {
        _animator.Play("Attack", -1, 0f);

        Vector3 spitPosition = transform.position + transform.forward * 1f;
        spitPosition.y += 0.95f;

        GameObject newSpit = PhotonNetwork.Instantiate(spitPrefab.name, spitPosition, Quaternion.identity);
        newSpit.transform.LookAt(transform.forward);
        Rigidbody spitRb = newSpit.GetComponent<Rigidbody>();
        if (spitRb != null)
        {
            spitRb.AddForce(transform.forward * spitForce, ForceMode.Impulse);
        }
    }

    [PunRPC]
    public override void OnDamage()
    {
        if (photonView.IsMine)
        {
            UIManager.Instance.SetDeadUI(true);
            _camera.cullingMask = -1;
        }
    }

    public void Stun()
    {
        if (photonView.IsMine)
        {
            isStunned = true;
            stunStartTime = Time.time;
        }
        
    }

    public void IncreaseCooldown()
    {
        attackCoolTime += 2f;
        nextAttackTime = Time.time + attackCoolTime; // ��Ÿ�� �缳��
        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateCooltimeText(0, attackCoolTime); // UI ������Ʈ
        }
    }
}