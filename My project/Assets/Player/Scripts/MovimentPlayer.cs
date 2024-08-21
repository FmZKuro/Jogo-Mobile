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
        jumpButton.onClick.AddListener(TryJump);

        // Adiciona o m�todo de Ataque ao bot�o de ataque
        attackButton.onClick.AddListener(TryAttack);
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

            // Habilita o bot�o de pulo novamente ao tocar o ch�o
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

    // M�todo para tentar realizar o pulo
    private void TryJump()
    {
        if (isGrounded && !animator.GetBool("isJumping"))
        {
            Jump();
        }
    }

    // M�todo chamado ao pressionar o bot�o de Jump
    private void Jump()
    {
        // Se o Player est� no ch�o, aplica a for�a de pulo
        velocityJump.y = Mathf.Sqrt(jumpForce * -2f * gravity);
        animator.SetBool("isJumping", true);

        // Desabilita o bot�o de pulo ap�s o pulo
        jumpButton.interactable = false;

        // Inicia uma coroutine para reabilitar o bot�o de pulo ap�s tocar o ch�o
        StartCoroutine(ReenableJumpButton());
    }

    private IEnumerator ReenableJumpButton()
    {
        // Espera at� que o jogador toque o ch�o novamente
        while (!isGrounded)
        {
            yield return null;
        }

        // Reabilita o bot�o de pulo
        jumpButton.interactable = true;
    }

    // M�todo para tentar realizar o ataque
    private void TryAttack()
    {
        if (animator.GetCurrentAnimatorStateInfo(0).IsTag("Attack"))
            return; // Evita que o jogador ataque novamente enquanto uma anima��o de ataque est� ocorrendo

        Attack();
    }

    private void Attack()
    {
        // Define o trigger de atacar no animator
        animator.SetTrigger("Attack");

        attackButton.interactable = false;

        // Inicia uma coroutine para reabilitar o bot�o de ataque ap�s a anima��o
        StartCoroutine(ReenableAttackButton());
    }

    private IEnumerator ReenableAttackButton()
    {
        // Espera at� que a anima��o de ataque termine
        yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);

        // Reabilita o bot�o de ataque
        attackButton.interactable = true;
    }
}