
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;

public class DefendPlayer : LivingEntity
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

    private float nextAttackTime = 0f;
    private float attackCoolTime = 2f;

    public bool isAlive = true;
    public GameObject ghostPrefab;
    public GameObject followCam;

    public bool isStunned = false;
    private float stunStartTime;
    private float stunDuration = 4f;

    private float eatDistance = 1f;
    private float hunger = 100f;
    private float hungerDecreaseRate = 100f / 60f;
    Vector3 movement;

    public GameObject deathEffect;

    // ����̵�
    public GameObject tornadoPrefab; // ����̵� ������ �Ҵ�
    public bool isCaughtedByTornado = false; // ����̵��� ��ȹ�Ǿ����� ����

    // hormone item
    public bool hormoneItem = false;

    private string myItem = "";

    // Start is called before the first frame update
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

    // Update is called once per frame
    void Update()
    {
        // compare Owner, Creator
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
            if (hormoneItem)
            {
                _camera.cullingMask |= (1 << 10 | 1 << 11);
            }
            else
            {
                _camera.cullingMask = ~(1 << 10 | 1 << 11);
            }
        }


        if (UIManager.Instance != null)
        {

            if (currentScene != "WaitingScene")
            {
                hunger -= hungerDecreaseRate * Time.deltaTime;
                UIManager.Instance.UpdateHungerBar(hunger);

                UIManager.Instance.SetItem(myItem);

                if (hunger <= 0f)
                {
                    OnDamage();
                    if (currentScene != "TeamFight")
                    {
                        UIManager.Instance.createKillLog("Hungry", PhotonNetwork.NickName);
                    }
                    else
                    {
                        TeamGameManagerVer2.instance.dataSyncManager.starvation(PhotonNetwork.LocalPlayer);
                        TeamUIManager.Instance.createKillLogHungry("배고픔", PhotonNetwork.NickName);
                    }
                }
            }
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

        // eat
        if (Input.GetMouseButtonDown(1))
        {
            Eat();
        }

        // use item
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            UseItem();
        }

    }


    void UseItem()
    {
        if (myItem == "Tornado")
        {
            Vector3 spawnPosition = transform.position + transform.forward * 2;
            PhotonNetwork.Instantiate(tornadoPrefab.name, spawnPosition, Quaternion.identity);
            myItem = "";
        }

    }

    void SpawnTorando()
    {
        if (tornadoPrefab != null)
        {
            Vector3 spawnPosition = transform.position + transform.forward * 2; 
            PhotonNetwork.Instantiate(tornadoPrefab.name, spawnPosition, Quaternion.identity);
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

    [PunRPC]
    public override void OnDamage()
    {
        if (photonView.IsMine)
        {
            UIManager.Instance.SetDeadUI(true);
            _camera.cullingMask = -1;
            TeamUIManager.Instance.createKillLog("배고픔", PhotonNetwork.NickName);
        }
        

        base.OnDamage();
        //배고파 죽는 로직

        
        _animator.Play("Dead", -1, 0f);
        TeamGameManager.instance.SetPlayerAlive(false);
        

        // TeamGameManagerVer2.instance.SetPlayerAlive(false);

        isAlive = false;
    }

    [PunRPC]
    public override async void Die()
    {
        base.Die();

        StartCoroutine(DieCoroutine());
    }

    private IEnumerator DieCoroutine()
    {
        yield return new WaitForSeconds(1.5f);

        dead = true;
        gameObject.SetActive(false);
        followCam.SetActive(false);

        if (deathEffect != null && photonView.IsMine)
        {
            PhotonNetwork.Instantiate(deathEffect.name, transform.position, transform.rotation);
        }

        if (ghostPrefab != null && photonView.IsMine)
        {
            object[] instantiationData = new object[] { 10 };
            PhotonNetwork.Instantiate(ghostPrefab.name, transform.position, transform.rotation, 0, instantiationData);
        }
    }

    private void Eat()
    {
        _animator.Play("Eat", -1, 0f);
        photonView.RPC("EatProcessOnServer", RpcTarget.All);
    }

    [PunRPC]
    private void EatProcessOnServer()
    {
        _animator.Play("Eat", -1, 0f);

        Collider[] hitColliders = Physics.OverlapSphere(transform.position, eatDistance);
        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.CompareTag("Grass"))
            {
                Destroy(hitCollider.gameObject);
                hunger += 40f;

                if (hunger > 100f)
                {
                    hunger = 100f;
                }
            }

            else if (hitCollider.CompareTag("TornadoItem"))
            {
                myItem = "Tornado";
                Destroy(hitCollider.gameObject);
            }

            
        }
    }
}