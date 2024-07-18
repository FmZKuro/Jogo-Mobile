using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class MovimentPlayer : MonoBehaviour
{
    [SerializeField] private float velocity = 4;

    private Vector2 playerInput;
    private CharacterController characterController;
    private Transform playerCam;
    private Animator animator;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
        playerCam = Camera.main.transform;
    }

    public void MovePlayer(InputAction.CallbackContext value)
    {
        playerInput = value.ReadValue<Vector2>();
    }
    private void Update()
    {
        RotatePlayer();
        characterController.Move(transform.forward * playerInput.magnitude * velocity * Time.deltaTime);
        characterController.Move(Vector3.down * 9.81f * Time.deltaTime);

        animator.SetBool("Walk", playerInput != Vector2.zero);
    }

    private void RotatePlayer()
    {
        Vector3 forward = playerCam.TransformDirection(Vector3.forward);

        Vector3 right = playerCam.TransformDirection(Vector3.right);

        Vector3 targetDirection = playerInput.x * right + playerInput.y * forward;

        if (playerInput != Vector2.zero && targetDirection.magnitude > 0.1f)
        {
            Quaternion freeRotation = Quaternion.LookRotation(targetDirection.normalized);
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(new Vector3(transform.eulerAngles.x, freeRotation.eulerAngles.y, transform.eulerAngles.z)), 10 * Time.deltaTime);
        }
    }
}
