
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GhostMovement : MonoBehaviour
{
    Animator _animator;
    Camera _camera;
    CharacterController _controller;

    public float speed = 3f;
    public float finalSpeed;
    public bool toggleCameraRotation;
    public float gravity = -9.81f;
    public float rotateSpeed = 3f;

    private Vector3 _velocity;

    void Start()
    {
        _animator = this.GetComponent<Animator>();
        _camera = Camera.main;
        _controller = this.GetComponent<CharacterController>();
    }

    void Update()
    {

        InputMovement();

        _velocity.y += gravity * Time.deltaTime;
        _controller.Move(_velocity * Time.deltaTime);

    }

    void InputMovement()
    {
        finalSpeed = speed;

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
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotateSpeed * Time.deltaTime);

            _controller.Move(inputDirection * finalSpeed * Time.deltaTime);
        }
    }
}