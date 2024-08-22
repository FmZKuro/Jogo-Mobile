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
    [SerializeField] private Button attackButton;               // Bot�o de ataque
    [SerializeField] private Button jumpButton;                 // Bot�o de pulo
    [SerializeField] private Transform groundCheck;             // Transform para verificar se o Player est� no ch�o
    [SerializeField] private float groundDistance = 0.4f;       // Dist�ncia para verificar o ch�o
    [SerializeField] private LayerMask groundMask;              // Camada do ch�o para verificar colis�o

    private Vector2 playerInput;                                // Entrada de movimento do Player
    private CharacterController characterController;            // Componente de controle do Player
    private Transform playerCam;                                // Transform da c�mera do Player
    private Animator animator;                                  // Componente de Anima��o do Player
    private Vector3 velocityJump;                               // Velocidade vertical do Jump
    private bool isGrounded;                                    // Flag para verificar se o Player est� no ch�o
    private bool isAttacking = false;                           // Flag para verificar se o Player est� atacando


    private void Awake()
    {
        // Inicializa��o dos componentes
        characterController = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
        playerCam = Camera.main.transform;

        // Adiciona os m�todo de Jump e Ataque aos bot�es
        jumpButton.onClick.AddListener(TryJump);
        attackButton.onClick.AddListener(TryAttack);
    }

    // M�todo para mover o Player, chamado pelo sistema de entrada
    public void MovePlayer(InputAction.CallbackContext value)
    {        
        if (!isAttacking)                                       // Se o Player n�o estiver atacando, ele pode se mover
        {            
            playerInput = value.ReadValue<Vector2>();           // L� o valor da entrada do Player
        }
        else
        {            
            playerInput = Vector2.zero;                         // Zera o input para impedir o movimento durante o ataque
        }
    }
    private void Update()
    {
        // Verifica se o Player est� no ch�o
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);
        animator.SetBool("isGrounded", isGrounded);

        // Reseta a velocidade vertical se estiver no ch�o e a velocidade vertical for negativa
        if (isGrounded && velocityJump.y < 0)
        {
            velocityJump.y = -2f;
            animator.SetBool("isJumping", false);

            // Reabilita o bot�o de pulo se o Player n�o estiver atacando
            if (!isAttacking)
            {
                jumpButton.interactable = true;
            }
        }

        // Rotaciona o Player de acordo com a entrada do Player
        RotatePlayer();

        // Movimenta o Player usando o CharacterController
        Vector3 move = transform.forward * playerInput.magnitude * velocity * Time.deltaTime;
        characterController.Move(move);

        // Aplica gravidade ao Player
        velocityJump.y += gravity * Time.deltaTime;
        characterController.Move(velocityJump * Time.deltaTime);

        // Define a anima��o de caminhar
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
        // Verifica se o Player pode pular (est� no ch�o, n�o est� pulando e n�o est� atacando)
        if (isGrounded && !animator.GetBool("isJumping") && !isAttacking)
        {
            Jump();
        }
    }

    // M�todo chamado ao pressionar o bot�o de Jump
    private void Jump()
    {
        // Se o Player est� no ch�o, aplica a for�a de pulo
        velocityJump.y = Mathf.Sqrt(jumpForce * -2f * gravity);
        animator.SetBool("isJumping", true);                   // Define a anima��o de Pulo                
        jumpButton.interactable = false;                       // Desabilita o bot�o de pulo ap�s o pulo        
        StartCoroutine(ReenableJumpButton());                  // Inicia uma coroutine para reabilitar o bot�o de pulo ap�s tocar o ch�o
    }

    private IEnumerator ReenableJumpButton()
    {
        // Espera at� que o Player toque o ch�o novamente
        while (!isGrounded)
        {
            yield return null;
        }        

        jumpButton.interactable = true;                        // Reabilita o bot�o de pulo
    }

    // M�todo para tentar realizar o ataque
    private void TryAttack()
    {
        // Impede o ataque se uma anima��o de ataque j� estiver sendo executada
        if (animator.GetCurrentAnimatorStateInfo(0).IsTag("Attack"))
            return;

        Attack();
    }

    // M�todo chamado ao pressionar o bot�o de Ataque
    private void Attack()
    {        
        animator.SetTrigger("Attack");                         // Define o trigger de atacar no animator

        attackButton.interactable = false;                     // Desabilita o bot�o de Ataque
        jumpButton.interactable = false;                       // Desabilita o bot�o de  Pulo
        
        isAttacking = true;                                    // Sinaliza que o Player est� atacando
        
        StartCoroutine(ReenableAttackButton());                // Inicia uma coroutine para reabilitar o bot�o de ataque ap�s a anima��o
    }

    // Coroutine para reabilitar o bot�o de ataque ap�s o t�rmino da anima��o
    private IEnumerator ReenableAttackButton()
    {
        // Espera at� que a anima��o de ataque termine
        yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);

        attackButton.interactable = true;                      // Reabilita o bot�o de Ataque
        jumpButton.interactable = true;                        // Reabilita o bot�o de Pulo
                
        isAttacking = false;                                   // Sinaliza que o Player terminou o ataque
    }
}