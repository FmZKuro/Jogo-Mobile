using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class MovimentPlayer : MonoBehaviour
{
    [SerializeField] private float velocity = 4;                // Velocidade de movimento
    [SerializeField] private float jumpForce = 1f;              // For�a do pulo
    [SerializeField] private float gravity = -20f;              // Gravidade aplicada ao Player
    [SerializeField] private Button jumpButton;                 // Bot�o de pulo
    [SerializeField] private Button attackButton;               // Bot�o de ataque
    [SerializeField] private Transform groundCheck;             // Transform para verificar se o Player est� no ch�o
    [SerializeField] private float groundDistance = 0.4f;       // Dist�ncia para verificar o ch�o
    [SerializeField] private LayerMask groundMask;              // Camada do ch�o para verificar colis�o

    private Vector2 playerInput;                                // Entrada de movimento do Player
    private CharacterController characterController;            // Componente de controle do Player
    private Transform playerCam;                                // Transform da c�mera do Player
    private Animator animator;                                  // Componente de Anima��o do Player
    private Vector3 velocityJump;                               // Velocidade vertical do Jump
    private bool isGrounded;                                    // Flag para verificar se o Player est� no ch�o

    private void Awake()
    {
        // Inicializa��o dos componentes
        characterController = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
        playerCam = Camera.main.transform;

        // Adiciona o m�todo de Jump ao bot�o de pulo
        jumpButton.onClick.AddListener(Jump);

        // Adiciona o m�todo de Ataque ao bot�o de ataque
        jumpButton.onClick.AddListener(Attack);
    }

    // M�todo para mover o Player, chamado pelo sistema de entrada
    public void MovePlayer(InputAction.CallbackContext value)
    {
        // L� o valor da entrada do Player
        playerInput = value.ReadValue<Vector2>();               
    }
    private void Update()
    {
        // Verifica se o Player est� no ch�o
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);
        animator.SetBool("isGrounded", isGrounded);             

        // Se o Player est� no ch�o e a velocidade vertical � menor que 0, redefine a velocidade vertical
        if (isGrounded && velocityJump.y < 0)
        {
            velocityJump.y = -2f;
            animator.SetBool("isJumping", false);
        }

        // Rotaciona o Player de acordo com a entrada do Player
        RotatePlayer();

        // Calcula o movimento do Player e aplica usando o CharacterController
        Vector3 move = transform.forward * playerInput.magnitude * velocity * Time.deltaTime;
        characterController.Move(move);

        // Aplica a gravidade na velocidade vertical e move o Player verticalmente
        velocityJump.y += gravity * Time.deltaTime;
        characterController.Move(velocityJump * Time.deltaTime);

        // Define a anima��o de caminhar se o Player est� se movendo
        animator.SetBool("Walk", playerInput != Vector2.zero);
    }

    // M�todo para rotacionar o Player de acordo com a dire��o de entrada
    private void RotatePlayer()
    {
        // Calcula a dire��o para frente e para a direita da c�mera
        Vector3 forward = playerCam.TransformDirection(Vector3.forward);
        Vector3 right = playerCam.TransformDirection(Vector3.right);
        Vector3 targetDirection = playerInput.x * right + playerInput.y * forward;

        // Se h� entrada do Player e a dire��o de destino � significativa, rotaciona o player
        if (playerInput != Vector2.zero && targetDirection.magnitude > 0.1f)
        {
            Quaternion freeRotation = Quaternion.LookRotation(targetDirection.normalized);
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(new Vector3(transform.eulerAngles.x, freeRotation.eulerAngles.y, transform.eulerAngles.z)), 10 * Time.deltaTime);
        }
    }

    // M�todo chamado ao pressionar o bot�o de Jump
    private void Jump()
    {
        // Se o Player est� no ch�o, aplica a for�a de pulo
        if (isGrounded)
        {
            velocityJump.y = Mathf.Sqrt(jumpForce * -2f * gravity);
            animator.SetBool("isJumping", true);
        }
    }

    private void Attack()
    {
        // Define o trigger de atacar no animator
        animator.SetTrigger("attack");
    }
}
