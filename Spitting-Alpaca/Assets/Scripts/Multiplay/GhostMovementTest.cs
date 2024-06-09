
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GhostMovementTest : MonoBehaviour
{
    Animator _animator;
    Camera _camera;
    Rigidbody _rigidbody;

    public float speed = 8f;
    public bool toggleCameraRotation;
    public float rotateSpeed = 3f;

    private Vector3 _velocity;

    void Start()
    {
        _animator = this.GetComponent<Animator>();
        _camera = Camera.main;
        _rigidbody = this.GetComponent<Rigidbody>();
    }

    void Update()
    {

        /*      InputMovement();*/

        /*_velocity.y += gravity * Time.deltaTime;*/
        /*_controller.Move(_velocity * Time.deltaTime);*/

    }

    void FixedUpdate()
    {
        InputMovement();
        _rigidbody.MovePosition(_rigidbody.position + _velocity * Time.fixedDeltaTime);
    }

    void InputMovement()
    {

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

            _velocity = inputDirection * speed;
        }
        else
        {
            _velocity = Vector3.zero;  // 입력이 없을 때 속도를 0으로 설정
        }
    }
}