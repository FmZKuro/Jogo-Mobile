using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class MovimentPlayer : MonoBehaviour
{
    [Header("Player Movement Settings")]
    [SerializeField] private float velocity = 5;                // Velocidade de movimento
    [SerializeField] private float jumpForce = 1f;              // Força do pulo
    [SerializeField] private float gravity = -20f;              // Gravidade aplicada ao Player

    [Header("UI Controls")]
    [SerializeField] private Button attackButton;               // Botão de ataque
    [SerializeField] private Button jumpButton;                 // Botão de pulo

    [Header("Ground Check Settings")]
    [SerializeField] private Transform groundCheck;             // Transform para verificar se o Player está no chão
    [SerializeField] private float groundDistance = 0.4f;       // Distância para verificar o chão
    [SerializeField] private LayerMask groundMask;              // Camada do chão para verificar colisão

    [Header("Sword Settings")]
    [SerializeField] private GameObject sword;                  // Referência ao objeto da espada    
    [SerializeField] private int swordDamage = 10;              // Dano causado pela espada
    private BoxCollider swordCollider;                          // Referência ao Box Collider da espada

    [Header("Audio Settings")]
    [SerializeField] private AudioClip walkSound;               // Som de caminhar
    [SerializeField] private AudioClip jumpSound;               // Som de pulo

    private Vector2 playerInput;                                // Entrada de movimento do Player
    private Vector3 velocityJump;                               // Velocidade vertical do Jump
    private CharacterController characterController;            // Componente de controle do Player
    private Transform playerCam;                                // Transform da câmera do Player
    private Animator animator;                                  // Componente de Animação do Player
    private AudioSource audioSource;                            // Componente de Áudio para tocar sons
    private bool isGrounded;                                    // Flag para verificar se o Player está no chão
    private bool isAttacking = false;                           // Flag para verificar se o Player está atacando
    private bool isDead = false;                                // Flag para verificar se o Player está morto
    [SerializeField] public GameObject player;

    private void Awake()
    {
        // Inicialização dos componentes
        characterController = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
        playerCam = Camera.main.transform;
        swordCollider = sword.GetComponent<BoxCollider>();
        audioSource = GetComponent<AudioSource>();

        // Adiciona os método de Jump e Ataque aos botões
        jumpButton.onClick.AddListener(TryJump);
        attackButton.onClick.AddListener(TryAttack);

        sword.SetActive(true);                                  // Garante que a espada esteja ativa
        swordCollider.enabled = false;                          // Desabilita o collider da espada inicialmente
    }

    // Método para mover o Player, chamado pelo sistema de entrada
    public void MovePlayer(InputAction.CallbackContext value)
    {
        if (!isAttacking)                                       // Se o Player não estiver atacando, ele pode se mover
        {
            playerInput = value.ReadValue<Vector2>();           // Lê o valor da entrada do Player
        }
        else
        {
            playerInput = Vector2.zero;                         // Zera o input para impedir o movimento durante o ataque
        }
    }
    private void Update()
    {
        // Verifica se o Player está no chão
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);
        animator.SetBool("isGrounded", isGrounded);
                
        if (isDead)                                             // Se o Player está morto, impede movimentos, ataques e pulo
        {
            playerInput = Vector2.zero;
            return;                                             // Impede outras ações como pulo e ataque
        }
                
        if (isGrounded && velocityJump.y < 0)                   // Reseta a velocidade vertical se estiver no chão e a velocidade vertical for negativa
        {
            velocityJump.y = -2f;
            animator.SetBool("isJumping", false);
                        
            if (!isAttacking)                                   // Reabilita o botão de pulo se o Player não estiver atacando
            {
                jumpButton.interactable = true;
            }
        }

        // Verifica se há entrada de movimento do jogador (não é zero), se o jogador está no chão e se o áudio atual não está tocando
        if (playerInput != Vector2.zero && isGrounded && !audioSource.isPlaying)
        {
            PlaySound(walkSound);                               // Se todas as condições forem verdadeiras, toca o som de caminhada
        }
                
        RotatePlayer();                                         // Rotaciona o Player de acordo com a entrada do Player

        // Movimenta o Player usando o CharacterController
        Vector3 move = transform.forward * playerInput.magnitude * velocity * Time.deltaTime;
        characterController.Move(move);        

        // Aplica gravidade ao Player
        velocityJump.y += gravity * Time.deltaTime;
        characterController.Move(velocityJump * Time.deltaTime);
        
        animator.SetBool("Walk", playerInput != Vector2.zero);  // Define a animação de caminhar
    }

    public void SetIsDead(bool dead)
    {
        isDead = dead;

        if (isDead)
        {
            playerInput = Vector2.zero;                         // Impede movimento após a morte
            attackButton.interactable = false;                  // Desabilita o botão de ataque
            jumpButton.interactable = false;                    // Desabilita o botão de pulo
            swordCollider.enabled = false;                      // Desabilita o collider da espada
        }
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
        // Verifica se o Player pode pular (está no chão, não está pulando e não está atacando)
        if (isGrounded && !animator.GetBool("isJumping") && !isAttacking)
        {
            Jump();
        }
    }

    // Método chamado ao pressionar o botão de Jump
    private void Jump()
    {        
        velocityJump.y = Mathf.Sqrt(jumpForce * -2f * gravity); // Se o Player está no chão, aplica a força de pulo
        animator.SetBool("isJumping", true);                    // Define a animação de Pulo                
        jumpButton.interactable = false;                        // Desabilita o botão de pulo após o pulo
        PlaySound(jumpSound);                                   // Toca o som de pulo                                              
        StartCoroutine(ReenableJumpButton());                   // Inicia uma coroutine para reabilitar o botão de pulo após tocar o chão
    }

    private IEnumerator ReenableJumpButton()
    {        
        while (!isGrounded)                                     // Espera até que o Player toque o chão novamente
        {
            yield return null;
        }

        jumpButton.interactable = true;                         // Reabilita o botão de pulo
    }

    // Método para tentar realizar o ataque
    private void TryAttack()
    {
        // Impede o ataque se uma animação de ataque já estiver sendo executada
        if (animator.GetCurrentAnimatorStateInfo(0).IsTag("Attack"))
            return;

        Attack();
    }

    // Método chamado ao pressionar o botão de Ataque
    private void Attack()
    {
        animator.SetTrigger("Attack");                          // Define o trigger de atacar no animator

        attackButton.interactable = false;                      // Desabilita o botão de Ataque
        jumpButton.interactable = false;                        // Desabilita o botão de Pulo

        isAttacking = true;                                     // Sinaliza que o Player está atacando
        swordCollider.enabled = true;                           // Habilita o collider da espada

        StartCoroutine(ReenableAttackButton());                 // Inicia uma coroutine para reabilitar o botão de ataque após a animação
    }

    // Coroutine para reabilitar o botão de ataque após o término da animação
    private IEnumerator ReenableAttackButton()
    {
        // Espera até que a animação de ataque termine
        yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);

        attackButton.interactable = true;                       // Reabilita o botão de Ataque
        jumpButton.interactable = true;                         // Reabilita o botão de Pulo

        isAttacking = false;                                    // Sinaliza que o Player terminou o ataque
        swordCollider.enabled = false;                          // Desabilita o collider da espada
    }

    public int GetSwordDamage()
    {
        return swordDamage;                                     // Retorna o dano causado pela espada
    }

    private void OnTriggerEnter(Collider other)
    {
        // Verifica se o GameObject com o qual entrou em contato está no Layer "Enemy"
        if (other.CompareTag("Axe"))
        {
            EnemyMoviment enemy = other.GetComponentInParent<EnemyMoviment>();
            if (enemy != null)
            {
                int damage = enemy.GetAxeDamage();               // Obtém o valor do dano da espada
                GetComponent<PlayerHealth>().TakeDamage(damage); // Causa dano ao inimigo
            }
        }
    }

    // Método para tocar um som
    private void PlaySound(AudioClip clip)
    {
        // Verifica se o áudio a ser reproduzido não é nulo
        if (clip != null)
        {        
            audioSource.PlayOneShot(clip);                      // Toca o áudio especificado usando o AudioSource, reproduzindo-o apenas uma vez
        }
    }
}