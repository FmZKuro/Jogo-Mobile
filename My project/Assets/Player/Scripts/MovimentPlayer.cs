using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class MovimentPlayer : MonoBehaviour
{
    [Header("Player Movement Settings")]
    [SerializeField] private float velocity = 5;                        // Velocidade de movimento do Player
    [SerializeField] private float jumpForce = 1f;                      // For�a de pulo do Player
    [SerializeField] private float gravity = -20f;                      // Gravidade aplicada ao Player

    [Header("UI Controls")]
    [SerializeField] private Button attackButton;                       // Bot�o de ataque da interface de usu�rio
    [SerializeField] private Button jumpButton;                         // Bot�o de pulo da interface de usu�rio

    [Header("Ground Check Settings")]
    [SerializeField] private Transform groundCheck;                     // Transform para verificar se o Player est� no ch�o
    [SerializeField] private float groundDistance = 0.4f;               // Dist�ncia para verificar o ch�o
    [SerializeField] private LayerMask groundMask;                      // Camada do ch�o para verificar colis�o

    [Header("Sword Settings")]
    [SerializeField] private GameObject sword;                          // Refer�ncia ao objeto da espada    
    [SerializeField] private int swordDamage = 10;                      // Dano causado pela espada
    private BoxCollider swordCollider;                                  // Refer�ncia ao Box Collider da espada

    [Header("Audio Settings")]
    [SerializeField] private AudioClip walkSound;                       // Som de caminhar
    [SerializeField] private AudioClip jumpSound;                       // Som de pulo

    private Vector2 playerInput;                                        // Entrada de movimento do Player
    private Vector3 velocityJump;                                       // Velocidade vertical do Jump
    private CharacterController characterController;                    // Componente de controle do Player
    private Transform playerCam;                                        // Transform da c�mera do Player
    private Animator animator;                                          // Componente de Anima��o do Player
    private AudioSource audioSource;                                    // Componente de �udio para tocar sons
    private bool isGrounded;                                            // Flag para verificar se o Player est� no ch�o
    private bool isAttacking = false;                                   // Flag para verificar se o Player est� atacando
    private bool isDead = false;                                        // Flag para verificar se o Player est� morto

    private void Awake()
    {
        InitializeComponents();                                         // Inicializa os componentes necess�rios
        AssignButtonListeners();                                        // Atribui os m�todos de pulo e ataque aos bot�es
        ConfigureSword();                                               // Configura a espada
    }
    private void InitializeComponents()
    {
        animator = GetComponent<Animator>();                            // Obt�m o componente de anima��o
        playerCam = Camera.main.transform;                              // Obt�m o transform da c�mera principal
        audioSource = GetComponent<AudioSource>();                      // Obt�m o componente de �udio
        swordCollider = sword.GetComponent<BoxCollider>();              // Obt�m o BoxCollider da espada
        characterController = GetComponent<CharacterController>();      // Obt�m o componente de controle de personagem
    }

    private void AssignButtonListeners()
    {
        jumpButton.onClick.AddListener(TryJump);                        // Atribui o m�todo TryJump ao bot�o de pulo
        attackButton.onClick.AddListener(TryAttack);                    // Atribui o m�todo TryAttack ao bot�o de ataque
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
            HandlePlayerDeath();                                        // Se o Player est� morto, trata a morte
        }
        else
        {
            HandlePlayerMovementAndActions();                          // Caso contr�rio, trata o movimento e as a��es do Player
        }

        DialogManager.Instance?.HandleUpdate();                        // Atualiza o estado do di�logo
    }

    private void HandlePlayerDeath()
    {
        playerInput = Vector2.zero;                                     // Impede o movimento
        DisableUIButtons();                                             // Desativa os bot�es da interface
    }

    private void HandlePlayerMovementAndActions()
    {
        CheckIfGrounded();                                              // Verifica se o Player est� no ch�o
        ApplyGravity();                                                 // Aplica a gravidade ao Player
        HandleMovement();                                               // Gerencia o movimento do Player
        RotatePlayer();                                                 // Rotaciona o Player de acordo com a entrada
        HandleWalkingSound();                                           // Gerencia o som de caminhada
        SetWalkingAnimation();                                          // Define a anima��o de caminhada
    }

    private void CheckIfGrounded()
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);     // Verifica se o Player est� no ch�o
        animator.SetBool("isGrounded", isGrounded);                                             // Define a vari�vel de anima��o

        if (isGrounded && velocityJump.y < 0)                                                   // Se estiver no ch�o e a velocidade vertical for negativa
        {
            velocityJump.y = -2f;                                                               // Reseta a velocidade vertical
            animator.SetBool("isJumping", false);                                               // Reseta a anima��o de pulo
            if (!isAttacking)
            {
                EnableJumpButton();                                                             // Reativa o bot�o de pulo se n�o estiver atacando
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
        if (playerInput == Vector2.zero) return;                                                // Se n�o h� movimento, retorna
        Vector3 move = transform.forward * playerInput.magnitude * velocity * Time.deltaTime;   // Calcula o movimento
        characterController.Move(move);                                                         // Move o Player
    }

    private void RotatePlayer()
    {
        if (playerInput == Vector2.zero) return;                                     // Se n�o h� movimento, retorna

        Vector3 forward = playerCam.TransformDirection(Vector3.forward);             // Obt�m a dire��o para frente da c�mera
        Vector3 right = playerCam.TransformDirection(Vector3.right);                 // Obt�m a dire��o para a direita da c�mera
        Vector3 targetDirection = playerInput.x * right + playerInput.y * forward;   // Calcula a dire��o de destino

        if (targetDirection.magnitude > 0.1f)                                        // Se a magnitude da dire��o de destino for significativa
        {
            Quaternion targetRotation = Quaternion.LookRotation(targetDirection.normalized); // Calcula a rota��o de destino
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(0, targetRotation.eulerAngles.y, 0), 10 * Time.deltaTime); // Suaviza a rota��o
        }
    }

    private void HandleWalkingSound()
    {
        if (playerInput != Vector2.zero && isGrounded && !audioSource.isPlaying)     // Se h� movimento e o som de caminhada n�o est� tocando
        {
            PlaySound(walkSound);                                                    // Toca o som de caminhada
        }
    }

    private void SetWalkingAnimation()
    {
        animator.SetBool("Walk", playerInput != Vector2.zero);          // Define a anima��o de caminhada
    }

    public void SetIsDead(bool dead)
    {
        isDead = dead;                                                  // Define o estado de morte do Player
        if (isDead)
        {
            DisablePlayerActions();                                     // Se est� morto, desativa as a��es do Player
        }
    }

    private void DisablePlayerActions()
    {
        playerInput = Vector2.zero;                                     // Impede o movimento
        DisableUIButtons();                                             // Desativa os bot�es da interface
        swordCollider.enabled = false;                                  // Desativa o collider da espada
    }

    private void DisableUIButtons()
    {
        attackButton.interactable = false;                              // Desativa o bot�o de ataque
        jumpButton.interactable = false;                                // Desativa o bot�o de pulo
    }

    private void EnableJumpButton()
    {
        jumpButton.interactable = true;                                 // Reativa o bot�o de pulo
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
        velocityJump.y = Mathf.Sqrt(jumpForce * -2f * gravity);         // Calcula a for�a do pulo
        animator.SetBool("isJumping", true);                            // Define a anima��o de pulo
        DisableJumpButton();                                            // Desativa o bot�o de pulo
        PlaySound(jumpSound);                                           // Toca o som de pulo
        StartCoroutine(ReenableJumpButton());                           // Inicia a coroutine para reativar o bot�o de pulo
    }

    private void DisableJumpButton()
    {
        jumpButton.interactable = false;                                // Desativa o bot�o de pulo
    }

    private IEnumerator ReenableJumpButton()
    {
        yield return new WaitUntil(() => isGrounded);                   // Espera at� que o Player esteja no ch�o
        EnableJumpButton();                                             // Reativa o bot�o de pulo
    }

    private void TryAttack()
    {
        if (animator.GetCurrentAnimatorStateInfo(0).IsTag("Attack")) return;    // Se j� est� atacando, retorna
        Attack();                                                               // Executa o ataque
    }

    private void Attack()
    {
        animator.SetTrigger("Attack");                                  // Define o trigger de ataque na anima��o
        DisableUIButtonsDuringAttack();                                 // Desativa os bot�es da interface durante o ataque
        swordCollider.enabled = true;                                   // Ativa o collider da espada
        isAttacking = true;                                             // Define o estado de ataque
        StartCoroutine(ReenableAttackButton());                         // Inicia a coroutine para reativar os bot�es ap�s o ataque
    }

    private void DisableUIButtonsDuringAttack()
    {
        attackButton.interactable = false;                              // Desativa o bot�o de ataque
        jumpButton.interactable = false;                                // Desativa o bot�o de pulo
    }

    private IEnumerator ReenableAttackButton()
    {
        yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);    // Espera a dura��o da anima��o de ataque
        EnableUIButtonsAfterAttack();                                                       // Reativa os bot�es da interface ap�s o ataque
        isAttacking = false;                                                                // Redefine o estado de ataque
        swordCollider.enabled = false;                                                      // Desativa o collider da espada
    }

    private void EnableUIButtonsAfterAttack()
    {
        attackButton.interactable = true;                               // Reativa o bot�o de ataque
        jumpButton.interactable = true;                                 // Reativa o bot�o de pulo
    }

    public int GetSwordDamage()
    {
        return swordDamage;                                             // Retorna o valor do dano da espada
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Axe"))                                                // Se colide com o machado
        {
            EnemyMoviment enemy = other.GetComponentInParent<EnemyMoviment>();      // Obt�m o script de movimenta��o do Enemy
            if (enemy != null)
            {
                int damage = enemy.GetAxeDamage();                                  // Obt�m o dano do machado do Enemy
                GetComponent<PlayerHealth>().TakeDamage(damage);                    // Aplica o dano ao Player
            }
        }

        else if (other.CompareTag("DialogTrigger"))                                 // Verifica se colidiu com um trigger de di�logo
        {
            DialogTrigger dialogTrigger = other.GetComponent<DialogTrigger>();
            if (dialogTrigger != null)
            {
                StartCoroutine(DialogManager.Instance.ShowDialog(dialogTrigger.dialog)); // Mostra o di�logo
            }
        }
    }

    private void PlaySound(AudioClip clip)
    {
        if (clip != null)                                               // Se o clip de �udio n�o for nulo
        {
            audioSource.PlayOneShot(clip);                              // Reproduz o som
        }
    }
}