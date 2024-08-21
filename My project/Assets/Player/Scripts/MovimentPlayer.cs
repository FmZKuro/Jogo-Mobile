using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class MovimentPlayer : MonoBehaviour
{
    [SerializeField] private float velocity = 4;                // Velocidade de movimento
    [SerializeField] private float jumpForce = 1f;              // Força do pulo
    [SerializeField] private float gravity = -20f;              // Gravidade aplicada ao Player
    [SerializeField] private Button jumpButton;                 // Botão de pulo
    [SerializeField] private Button attackButton;               // Botão de ataque
    [SerializeField] private Transform groundCheck;             // Transform para verificar se o Player está no chão
    [SerializeField] private float groundDistance = 0.4f;       // Distância para verificar o chão
    [SerializeField] private LayerMask groundMask;              // Camada do chão para verificar colisão

    private Vector2 playerInput;                                // Entrada de movimento do Player
    private CharacterController characterController;            // Componente de controle do Player
    private Transform playerCam;                                // Transform da câmera do Player
    private Animator animator;                                  // Componente de Animação do Player
    private Vector3 velocityJump;                               // Velocidade vertical do Jump
    private bool isGrounded;                                    // Flag para verificar se o Player está no chão

    private void Awake()
    {
        // Inicialização dos componentes
        characterController = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
        playerCam = Camera.main.transform;

        // Adiciona o método de Jump ao botão de pulo
        jumpButton.onClick.AddListener(TryJump);

        // Adiciona o método de Ataque ao botão de ataque
        attackButton.onClick.AddListener(TryAttack);
    }

    // Método para mover o Player, chamado pelo sistema de entrada
    public void MovePlayer(InputAction.CallbackContext value)
    {
        // Lê o valor da entrada do Player
        playerInput = value.ReadValue<Vector2>();
    }
    private void Update()
    {
        // Verifica se o Player está no chão
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);
        animator.SetBool("isGrounded", isGrounded);

        // Se o Player está no chão e a velocidade vertical é menor que 0, redefine a velocidade vertical
        if (isGrounded && velocityJump.y < 0)
        {
            velocityJump.y = -2f;
            animator.SetBool("isJumping", false);

            // Habilita o botão de pulo novamente ao tocar o chão
            jumpButton.interactable = true;
        }

        // Rotaciona o Player de acordo com a entrada do Player
        RotatePlayer();

        // Calcula o movimento do Player e aplica usando o CharacterController
        Vector3 move = transform.forward * playerInput.magnitude * velocity * Time.deltaTime;
        characterController.Move(move);

        // Aplica a gravidade na velocidade vertical e move o Player verticalmente
        velocityJump.y += gravity * Time.deltaTime;
        characterController.Move(velocityJump * Time.deltaTime);

        // Define a animação de caminhar se o Player está se movendo
        animator.SetBool("Walk", playerInput != Vector2.zero);
    }

    // Método para rotacionar o Player de acordo com a direção de entrada
    private void RotatePlayer()
    {
        // Calcula a direção para frente e para a direita da câmera
        Vector3 forward = playerCam.TransformDirection(Vector3.forward);
        Vector3 right = playerCam.TransformDirection(Vector3.right);
        Vector3 targetDirection = playerInput.x * right + playerInput.y * forward;

        // Se há entrada do Player e a direção de destino é significativa, rotaciona o player
        if (playerInput != Vector2.zero && targetDirection.magnitude > 0.1f)
        {
            Quaternion freeRotation = Quaternion.LookRotation(targetDirection.normalized);
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(new Vector3(transform.eulerAngles.x, freeRotation.eulerAngles.y, transform.eulerAngles.z)), 10 * Time.deltaTime);
        }
    }

    // Método para tentar realizar o pulo
    private void TryJump()
    {
        if (isGrounded && !animator.GetBool("isJumping"))
        {
            Jump();
        }
    }

    // Método chamado ao pressionar o botão de Jump
    private void Jump()
    {
        // Se o Player está no chão, aplica a força de pulo
        velocityJump.y = Mathf.Sqrt(jumpForce * -2f * gravity);
        animator.SetBool("isJumping", true);

        // Desabilita o botão de pulo após o pulo
        jumpButton.interactable = false;

        // Inicia uma coroutine para reabilitar o botão de pulo após tocar o chão
        StartCoroutine(ReenableJumpButton());
    }

    private IEnumerator ReenableJumpButton()
    {
        // Espera até que o jogador toque o chão novamente
        while (!isGrounded)
        {
            yield return null;
        }

        // Reabilita o botão de pulo
        jumpButton.interactable = true;
    }

    // Método para tentar realizar o ataque
    private void TryAttack()
    {
        if (animator.GetCurrentAnimatorStateInfo(0).IsTag("Attack"))
            return; // Evita que o jogador ataque novamente enquanto uma animação de ataque está ocorrendo

        Attack();
    }

    private void Attack()
    {
        // Define o trigger de atacar no animator
        animator.SetTrigger("Attack");

        attackButton.interactable = false;

        // Inicia uma coroutine para reabilitar o botão de ataque após a animação
        StartCoroutine(ReenableAttackButton());
    }

    private IEnumerator ReenableAttackButton()
    {
        // Espera até que a animação de ataque termine
        yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);

        // Reabilita o botão de ataque
        attackButton.interactable = true;
    }
}