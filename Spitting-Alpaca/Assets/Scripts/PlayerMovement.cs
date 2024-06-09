
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerMovement : MonoBehaviour
{
    Animator _animator;
    Camera _camera;
    CharacterController _controller;

    public float speed = 2f;
    public float runSpeed = 8f;
    public float finalSpeed;
    public bool toggleCameraRotation;
    public bool run;
    public float jumpHeight = 1f;
    public float gravity = -9.81f;
    public float rotateSpeed = 3f;

    private Vector3 _velocity;
    private bool isGrounded;
    private bool isJumping;

    // player status 
    public bool isAlive = true;

    // Start is called before the first frame update
    void Start()
    {
        _animator = this.GetComponent<Animator>();
        _camera = Camera.main;
        _controller = this.GetComponent<CharacterController>();
        isJumping = false;
    }

    // Update is called once per frame
    void Update()
    {
        // alive or dead
        if (isAlive )
        {


            isGrounded = _controller.isGrounded;
            if (isGrounded && _velocity.y < 0)
            {
                _velocity.y = -2f;
                isJumping = false; // after jump
                _animator.SetBool("isGrounded", true); // stop animation
            }
            else
            {
                _animator.SetBool("isGrounded", false);  // start animation
            }

            // run
            if (Input.GetKey(KeyCode.LeftShift))
            {
                run = true;
            }
            else
            {
                run = false;
            }

            // jump
            if (Input.GetKeyDown(KeyCode.Space) && isGrounded && !isJumping)
            {
                _velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
                isJumping = true;
            }

            InputMovement();

            _velocity.y += gravity * Time.deltaTime;
            _controller.Move(_velocity * Time.deltaTime);
        }
    }

    void InputMovement()
    {
        finalSpeed = (run) ? runSpeed : speed;

        // perspective switching
        Vector3 cameraForward = Camera.main.transform.forward;
        Vector3 cameraRight = Camera.main.transform.right;
        cameraForward.y = 0;
        cameraRight.y = 0;
        cameraForward.Normalize();
        cameraRight.Normalize();

        // direction by input
        float horizontalInput = Input.GetAxisRaw("Horizontal");
        float verticalInput = Input.GetAxisRaw("Vertical");
        Vector3 inputDirection = (cameraRight * horizontalInput + cameraForward * verticalInput).normalized;

        if (inputDirection != Vector3.zero)
        {
            // rotate character to input direction
            Quaternion targetRotation = Quaternion.LookRotation(inputDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotateSpeed * Time.deltaTime);

            // execution
            _controller.Move(inputDirection * finalSpeed * Time.deltaTime);

            // activate animation on the move
            _animator.SetFloat("Blend", run ? 1.0f : 0.5f, 0.1f, Time.deltaTime);
        }
        else
        {
            // stop animation when there is no input
            _animator.SetFloat("Blend", 0, 0.1f, Time.deltaTime);
        }
    }
}