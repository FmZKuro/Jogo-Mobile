using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class MovimentPlayer : MonoBehaviour
{
    [Header("Player Movement Settings")]
    [SerializeField] private float velocity = 5;                        // Velocidade de movimento do Player
    [SerializeField] private float jumpForce = 1f;                      // Força de pulo do Player
    [SerializeField] private float gravity = -20f;                      // Gravidade aplicada ao Player

    [Header("UI Controls")]
    [SerializeField] private Button attackButton;                       // Botão de ataque da interface de usuário
    [SerializeField] private Button jumpButton;                         // Botão de pulo da interface de usuário

    [Header("Ground Check Settings")]
    [SerializeField] private Transform groundCheck;                     // Transform para verificar se o Player está no chão
    [SerializeField] private float groundDistance = 0.4f;               // Distância para verificar o chão
    [SerializeField] private LayerMask groundMask;                      // Camada do chão para verificar colisão

    [Header("Sword Settings")]
    [SerializeField] private GameObject sword;                          // Referência ao objeto da espada    
    [SerializeField] private int swordDamage = 10;                      // Dano causado pela espada
    private BoxCollider swordCollider;                                  // Referência ao Box Collider da espada

    [Header("Audio Settings")]
    [SerializeField] private AudioClip walkSound;                       // Som de caminhar
    [SerializeField] private AudioClip jumpSound;                       // Som de pulo

    private Vector2 playerInput;                                        // Entrada de movimento do Player
    private Vector3 velocityJump;                                       // Velocidade vertical do Jump
    private CharacterController characterController;                    // Componente de controle do Player
    private Transform playerCam;                                        // Transform da câmera do Player
    private Animator animator;                                          // Componente de Animação do Player
    private AudioSource audioSource;                                    // Componente de Áudio para tocar sons
    private bool isGrounded;                                            // Flag para verificar se o Player está no chão
    private bool isAttacking = false;                                   // Flag para verificar se o Player está atacando
    private bool isDead = false;                                        // Flag para verificar se o Player está morto

    private void Awake()
    {
        InitializeComponents();                                         // Inicializa os componentes necessários
        AssignButtonListeners();                                        // Atribui os métodos de pulo e ataque aos botões
        ConfigureSword();                                               // Configura a espada
    }
    private void InitializeComponents()
    {
        animator = GetComponent<Animator>();                            // Obtém o componente de animação
        playerCam = Camera.main.transform;                              // Obtém o transform da câmera principal
        audioSource = GetComponent<AudioSource>();                      // Obtém o componente de áudio
        swordCollider = sword.GetComponent<BoxCollider>();              // Obtém o BoxCollider da espada
        characterController = GetComponent<CharacterController>();      // Obtém o componente de controle de personagem
    }

    private void AssignButtonListeners()
    {
        jumpButton.onClick.AddListener(TryJump);                        // Atribui o método TryJump ao botão de pulo
        attackButton.onClick.AddListener(TryAttack);                    // Atribui o método TryAttack ao botão de ataque
    }

    private void ConfigureSword()
    {
        sword.SetActive(true);                                          // Garante que a espada esteja ativa
        swordCollider.enabled = false;                                  // Desativa o collider da espada inicialmente
    }

    public void MovePlayer(InputAction.CallbackContext value)
    {
        playerInput = isAttacking ? Vector2.zero : value.ReadValue<Vector2>(); // Se estiver atacando, impede o movimento
    }

    private void Update()
    {
        if (isDead)
        {
            HandlePlayerDeath();                                        // Se o Player está morto, trata a morte
        }
        else
        {
            HandlePlayerMovementAndActions();                          // Caso contrário, trata o movimento e as ações do Player
        }

        DialogManager.Instance?.HandleUpdate();                        // Atualiza o estado do diálogo
    }

    private void HandlePlayerDeath()
    {
        playerInput = Vector2.zero;                                     // Impede o movimento
        DisableUIButtons();                                             // Desativa os botões da interface
    }

    private void HandlePlayerMovementAndActions()
    {
        CheckIfGrounded();                                              // Verifica se o Player está no chão
        ApplyGravity();                                                 // Aplica a gravidade ao Player
        HandleMovement();                                               // Gerencia o movimento do Player
        RotatePlayer();                                                 // Rotaciona o Player de acordo com a entrada
        HandleWalkingSound();                                           // Gerencia o som de caminhada
        SetWalkingAnimation();                                          // Define a animação de caminhada
    }

    private void CheckIfGrounded()
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);     // Verifica se o Player está no chão
        animator.SetBool("isGrounded", isGrounded);                                             // Define a variável de animação

        if (isGrounded && velocityJump.y < 0)                                                   // Se estiver no chão e a velocidade vertical for negativa
        {
            velocityJump.y = -2f;                                                               // Reseta a velocidade vertical
            animator.SetBool("isJumping", false);                                               // Reseta a animação de pulo
            if (!isAttacking)
            {
                EnableJumpButton();                                                             // Reativa o botão de pulo se não estiver atacando
            }
        }
    }

    private void ApplyGravity()
    {
        velocityJump.y += gravity * Time.deltaTime;                     // Aplica a gravidade
        characterController.Move(velocityJump * Time.deltaTime);        // Move o Player para baixo
    }

    private void HandleMovement()
    {
        if (playerInput == Vector2.zero) return;                                                // Se não há movimento, retorna
        Vector3 move = transform.forward * playerInput.magnitude * velocity * Time.deltaTime;   // Calcula o movimento
        characterController.Move(move);                                                         // Move o Player
    }

    private void RotatePlayer()
    {
        if (playerInput == Vector2.zero) return;                                     // Se não há movimento, retorna

        Vector3 forward = playerCam.TransformDirection(Vector3.forward);             // Obtém a direção para frente da câmera
        Vector3 right = playerCam.TransformDirection(Vector3.right);                 // Obtém a direção para a direita da câmera
        Vector3 targetDirection = playerInput.x * right + playerInput.y * forward;   // Calcula a direção de destino

        if (targetDirection.magnitude > 0.1f)                                        // Se a magnitude da direção de destino for significativa
        {
            Quaternion targetRotation = Quaternion.LookRotation(targetDirection.normalized); // Calcula a rotação de destino
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(0, targetRotation.eulerAngles.y, 0), 10 * Time.deltaTime); // Suaviza a rotação
        }
    }

    private void HandleWalkingSound()
    {
        if (playerInput != Vector2.zero && isGrounded && !audioSource.isPlaying)     // Se há movimento e o som de caminhada não está tocando
        {
            PlaySound(walkSound);                                                    // Toca o som de caminhada
        }
    }

    private void SetWalkingAnimation()
    {
        animator.SetBool("Walk", playerInput != Vector2.zero);          // Define a animação de caminhada
    }

    public void SetIsDead(bool dead)
    {
        isDead = dead;                                                  // Define o estado de morte do Player
        if (isDead)
        {
            DisablePlayerActions();                                     // Se está morto, desativa as ações do Player
        }
    }

    private void DisablePlayerActions()
    {
        playerInput = Vector2.zero;                                     // Impede o movimento
        DisableUIButtons();                                             // Desativa os botões da interface
        swordCollider.enabled = false;                                  // Desativa o collider da espada
    }

    private void DisableUIButtons()
    {
        attackButton.interactable = false;                              // Desativa o botão de ataque
        jumpButton.interactable = false;                                // Desativa o botão de pulo
    }

    private void EnableJumpButton()
    {
        jumpButton.interactable = true;                                 // Reativa o botão de pulo
    }

    private void TryJump()
    {
        if (isGrounded && !animator.GetBool("isJumping") && !isAttacking)   // Se pode pular
        {
            Jump();                                                         // Executa o pulo
        }
    }

    private void Jump()
    {
        velocityJump.y = Mathf.Sqrt(jumpForce * -2f * gravity);         // Calcula a força do pulo
        animator.SetBool("isJumping", true);                            // Define a animação de pulo
        DisableJumpButton();                                            // Desativa o botão de pulo
        PlaySound(jumpSound);                                           // Toca o som de pulo
        StartCoroutine(ReenableJumpButton());                           // Inicia a coroutine para reativar o botão de pulo
    }

    private void DisableJumpButton()
    {
        jumpButton.interactable = false;                                // Desativa o botão de pulo
    }

    private IEnumerator ReenableJumpButton()
    {
        yield return new WaitUntil(() => isGrounded);                   // Espera até que o Player esteja no chão
        EnableJumpButton();                                             // Reativa o botão de pulo
    }

    private void TryAttack()
    {
        if (animator.GetCurrentAnimatorStateInfo(0).IsTag("Attack")) return;    // Se já está atacando, retorna
        Attack();                                                               // Executa o ataque
    }

    private void Attack()
    {
        animator.SetTrigger("Attack");                                  // Define o trigger de ataque na animação
        DisableUIButtonsDuringAttack();                                 // Desativa os botões da interface durante o ataque
        swordCollider.enabled = true;                                   // Ativa o collider da espada
        isAttacking = true;                                             // Define o estado de ataque
        StartCoroutine(ReenableAttackButton());                         // Inicia a coroutine para reativar os botões após o ataque
    }

    private void DisableUIButtonsDuringAttack()
    {
        attackButton.interactable = false;                              // Desativa o botão de ataque
        jumpButton.interactable = false;                                // Desativa o botão de pulo
    }

    private IEnumerator ReenableAttackButton()
    {
        yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);    // Espera a duração da animação de ataque
        EnableUIButtonsAfterAttack();                                                       // Reativa os botões da interface após o ataque
        isAttacking = false;                                                                // Redefine o estado de ataque
        swordCollider.enabled = false;                                                      // Desativa o collider da espada
    }

    private void EnableUIButtonsAfterAttack()
    {
        attackButton.interactable = true;                               // Reativa o botão de ataque
        jumpButton.interactable = true;                                 // Reativa o botão de pulo
    }

    public int GetSwordDamage()
    {
        return swordDamage;                                             // Retorna o valor do dano da espada
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Axe"))                                                // Se colide com o machado
        {
            EnemyMoviment enemy = other.GetComponentInParent<EnemyMoviment>();      // Obtém o script de movimentação do Enemy
            if (enemy != null)
            {
                int damage = enemy.GetAxeDamage();                                  // Obtém o dano do machado do Enemy
                GetComponent<PlayerHealth>().TakeDamage(damage);                    // Aplica o dano ao Player
            }
        }

        else if (other.CompareTag("DialogTrigger"))                                 // Verifica se colidiu com um trigger de diálogo
        {
            DialogTrigger dialogTrigger = other.GetComponent<DialogTrigger>();
            if (dialogTrigger != null)
            {
                StartCoroutine(DialogManager.Instance.ShowDialog(dialogTrigger.dialog)); // Mostra o diálogo
            }
        }
    }

    private void PlaySound(AudioClip clip)
    {
        if (clip != null)                                               // Se o clip de áudio não for nulo
        {
            audioSource.PlayOneShot(clip);                              // Reproduz o som
        }
    }
}