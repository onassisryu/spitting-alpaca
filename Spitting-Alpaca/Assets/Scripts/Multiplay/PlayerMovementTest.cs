
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;

public class PlayerMovementTest : LivingEntity
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
    public GameObject hormoneEffectPrefab;  // 추가: 호르몬 이펙트 프리팹

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
            _camera.cullingMask = ~(1 << 10 | 1 << 11);
        }

        if (UIManager.Instance != null)
        {
            float cooltimeLeft = nextAttackTime - Time.time;
            float stuntimeLeft = stunStartTime + stunDuration - Time.time;

            UIManager.Instance.UpdateCooltimeText(cooltimeLeft, attackCoolTime);
            if (isStunned)
            {
                UIManager.Instance.UpdateStunStatus(stuntimeLeft);
            }
            else
            {
                UIManager.Instance.UpdateStunStatus(0f);
            }

            if (currentScene != "WaitingScene")
            {
                hunger -= hungerDecreaseRate * Time.deltaTime;
                UIManager.Instance.UpdateHungerBar(hunger);

                UIManager.Instance.SetItem(myItem);

                if (hunger <= 0f)
                {
                    OnDamage();

                    
                    // TeamGameManagerVer2.instance.dataSyncManager.starvation(PhotonNetwork.LocalPlayer);    
                    // TeamUIManager.Instance.createKillLog("배고픔",PhotonNetwork.NickName);
                    UIManager.Instance.createKillLog("Hungry", PhotonNetwork.NickName);
                    
                }
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

        // ����̵� �۾��� 
        if (isCaughtedByTornado == true)  // ���̰� 2 �̻��� ���
        {
            _animator.SetBool("isSpinned", true);  // 'Swing' �ִϸ��̼� Ȱ��ȭ
        }
        else
        {
            _animator.SetBool("isSpinned", false);  // 
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

        // eat
        if (Input.GetMouseButtonDown(1))
        {
            Eat();
        }

        // use item
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            /*SpawnTorando();*/
            UseItem();
        }

    }

    // ������ ���
    void UseItem()
    {
        if (myItem == "Tornado")
        {
            Vector3 spawnPosition = transform.position + transform.forward * 2;
            PhotonNetwork.Instantiate(tornadoPrefab.name, spawnPosition, Quaternion.identity);
            myItem = "";
        }
        else if (myItem == "Hormone")
        {
            /*hormoneItem = true;*/
            myItem = "";
            /*StartCoroutine(SetHormoneItemFalseAfterDelay(10f));*/

            // Tag가 "player"인 모든 객체를 찾기
            GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
            foreach (GameObject player in players)
            {
                // 로컬 플레이어가 아닌 경우에만 이펙트를 생성
                if (player != this.gameObject)
                {
                    // 이펙트를 생성하고 플레이어 객체를 따라다니게 함
                    GameObject effect = Instantiate(hormoneEffectPrefab, player.transform.position, Quaternion.identity);
                    effect.transform.SetParent(player.transform);
                    // 이펙트를 플레이어의 자식으로 설정하여 따라다니게 함
                    effect.GetComponent<FollowTarget>().SetTarget(player.transform);
                }
            }
        }

    }

    /*IEnumerator SetHormoneItemFalseAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        hormoneItem = false;
    }*/

    void SpawnTorando()
    {
        if (tornadoPrefab != null)
        {
            Vector3 spawnPosition = transform.position + transform.forward * 2; // �÷��̾� �տ� ����̵� ����
            PhotonNetwork.Instantiate(tornadoPrefab.name, spawnPosition, Quaternion.identity);
        }
    }

    void FixedUpdate()
    {
        if (photonView.IsMine && !isStunned && isAlive && !UIManager.Instance.menuActiveSelf)
        {
            // ���� �÷��̾��� �Է� ó��
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

    // �ٴڿ� ��Ҵ��� ���� 
    //�ٸ� ��ü�� �浹�� ������ �� �ѹ� ȣ���ϴ� �޼���
    void OnCollisionEnter(Collision collision)
    {
        CheckGroundStatus(collision);
    }

    // �ٸ� ��ü�� ����ؼ� �浹�ϰ� ���� �� �� �����Ӹ��� ȣ��
    void OnCollisionStay(Collision collision)
    {
        CheckGroundStatus(collision);
    }

    void CheckGroundStatus(Collision collision)
    {
        // �ٴڰ��� �浹 Ȯ��
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
            isGrounded = false;  // ���� ���� �ʾ����Ƿ� ���� ���� ���� ���·� ����
        }
    }

    void Jump()
    {
        if (isGrounded && _animator.GetBool("isJumping") == false) // �̹� ���� ���� �ƴ� ���� ���� ����
        {
            _rigidbody.AddForce(Vector3.up * Mathf.Sqrt(jumpHeight * -1f * Physics.gravity.y), ForceMode.Impulse);
            _animator.SetBool("isJumping", true);
        }


    }

    // ���뼱 

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
        

        base.OnDamage();

        _animator.Play("Dead", -1, 0f);
        isAlive = false;

        // 모든 클라이언트에서 CheckAlivePlayers 호출
        GameManager.instance.photonView.RPC("CheckAlivePlayers", RpcTarget.All);
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

    public void Stun()
    {
        if (photonView.IsMine)
        {
            isStunned = true;
            stunStartTime = Time.time;
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

            // ������ ȹ�� 
            else if (hitCollider.CompareTag("TornadoItem"))
            {
                if (Random.value < 0.6f)
                {
                    myItem = "Tornado";
                }
                else
                {
                    myItem = "Hormone";
                   
                }
                Destroy(hitCollider.gameObject);
            }

            else if (hitCollider.CompareTag("onlyT"))
            {
                myItem = "Tornado";
                Destroy(hitCollider.gameObject);
            }

            else if (hitCollider.CompareTag("onlyH"))
            {
                myItem = "Hormone";
                Destroy(hitCollider.gameObject);
            }

            
        }
    }
}