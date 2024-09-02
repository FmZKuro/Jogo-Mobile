using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AI;
using System.Security.Cryptography;

public class EnemyMoviment : MonoBehaviour
{
    [Header("Enemy Movement Settings")]
    [SerializeField] private int HP = 100;                                  // Declara uma vari�vel privada para representar a vida atual no editor
    [SerializeField] private int maxHP = 100;                               // Declara uma vari�vel privada para representar a vida m�xima no editor
    [SerializeField] private float rotationSpeed = 5f;                      // Velocidade de rota��o do Enemy
    [SerializeField] private float rotationDistance = 5f;                   // Dist�ncia para iniciar rota��o do Enemy
    [SerializeField] private float moveSpeed = 3f;                          // Velocidade de movimento do Enemy
    [SerializeField] private float moveDistance = 4f;                       // Dist�ncia para o Enemy mover em dire��o ao Player

    [Header("Enemy Regeneration Settings")]
    [SerializeField] private int regenerationAmount = 2;                    // Quantidade de vida que o inimigo ir� regenerar a cada intervalo
    [SerializeField] private int regenerationInterval = 5;                  // Intervalo de tempo (em segundos) entre as regenera��es

    [Header("Enemy Attack Settings")]
    [SerializeField] private GameObject axe;                                // Refer�ncia ao GameObject do machado
    [SerializeField] private BoxCollider axeCollider;                       // Refer�ncia ao BoxCollider do machado
    [SerializeField] private int axeDamage = 4;                             // Dano do machado
    [SerializeField] private float attackDistance = 2f;                     // Dist�ncia de ataque
    [SerializeField] private float attackCooldown = 2f;                     // Tempo de cooldown entre ataques

    [Header("Enemy UI Settings")]
    [SerializeField] private Slider healthBar;                              // Declara uma vari�vel para representar a barra de vida do enemy
    [SerializeField] private TextMeshProUGUI healthText;                    // Refer�ncia ao TextMeshPro para exibir o n�mero

    [Header("Enemy Audio Settings")]
    [SerializeField] private AudioClip attackSound;                         // Som do ataque
    [SerializeField] private AudioClip damageSound;                         // Som de dano
    [SerializeField] private AudioClip deathSound;                          // Som de morte

    [Header("Enemy Idle Settings")]
    [SerializeField] private AudioClip idleSound;                           // Som de idle
    [SerializeField] float idleSoundRange = 10f;                            // Dist�ncia minima para ouvir o som de idle   

    private CharacterController characterController;                        // Refer�ncia ao Character Controller do Enemy
    private PlayerHealth playerHealth;                                      // Refer�ncia ao script de sa�de do jogador
    private Coroutine regenerationCoroutine;                                // Armazena a refer�ncia da coroutine de regenera��o    
    private Animator animator;                                              // Declara uma vari�vel privada para representar o componente Animator
    private AudioSource audioSource;                                        // Componente AudioSource para tocar os sons
    public Transform player;                                                // obt�m o transform do Player
    private bool isDead = false;                                            // Flag para verificar se o Enemy est� morto
    private bool isAttackOnCooldown = false;                                // Flag para verificar o cooldown do ataque
    private bool alreadyAttacked = false;                                   // Flag para verificar se o Enemy ja atacou
    private bool isPlayingIdleSound = false;                                // Flag para verificar se est� executando o som de idle
    private bool isAttacking = false;                                       // Flag para verificar se est� atacando

    private void Awake()
    {
        player = GameObject.Find("Player").transform;                       // Encontra e armazena a refer�ncia do jogador
        animator = GetComponent<Animator>();                                // Obt�m e armazena o componente Animator do Enemy
        audioSource = GetComponent<AudioSource>();                          // Inicializa o componente AudioSource
        playerHealth = player.GetComponent<PlayerHealth>();                 // Encontre o componente PlayerHealth do jogador
        axeCollider.enabled = false;                                        // Ativa o BoxCollider do machado
        characterController = GetComponent<CharacterController>();          // Obt�m o CharacterController do Enemy
    }

    // Start is called before the first frame update
    void Start()
    {
        PlaySound(idleSound, true);                                         // Inicia o som de idle em loop
        healthBar.maxValue = maxHP;                                         // Define o valor m�ximo da barra de vida com base no HP m�ximo do Enemy
        healthBar.value = HP;                                               // Define o valor atual da barra de vida com base no HP atual do Enemy

        if (healthText != null)
        {
            healthText.text = $" {HP} / {maxHP} ";                          // Atualiza o texto da barra de vida
        }

        regenerationCoroutine = StartCoroutine(RegenerateHealth());         // Inicia a coroutine de regenera��o de vida        
    }

    // Update is called once per frame
    void Update()
    {
        if (isDead) return;                                                 // Se o Enemy est� morto, n�o faz nada

        UpdateHealthUI();                                                   // Atualiza a UI da sa�de
        HandlePlayerProximity();                                            // Lida com a proximidade do Player

        // Verifica a dist�ncia entre o Enemy e o Player
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer <= idleSoundRange)                             // Se a dist�ncia for menor que o alcance do som de idle e o som n�o est� tocando, comece a tocar
        {
            if (!isPlayingIdleSound)
            {
                PlaySound(idleSound, true);                                 // Come�a a tocar o som de idle em loop
            }
        }
        else
        {
            if (isPlayingIdleSound)
            {
                audioSource.Stop();                                         // Para o som de idle
                isPlayingIdleSound = false;
            }
        }
    }

    private void HandlePlayerProximity()
    {
        if (isAttacking)                                                    // Se o Enemy est� atacando, n�o faz nada
        {
            return;
        }

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer <= rotationDistance)
        {
            RotateTowardsPlayer();
        }

        if (distanceToPlayer <= moveDistance && distanceToPlayer > attackDistance)
        {
            MoveTowardsPlayer();
        }
        else
        {
            animator.SetBool("EnemyRunning", false);
        }

        if (distanceToPlayer <= attackDistance)
        {
            if (!alreadyAttacked && !isAttackOnCooldown)
            {
                AttackPlayer();                                             // Ataca o Player se estiver pr�ximo
            }
        }
    }

    private void RotateTowardsPlayer()
    {
        // Calcula a dire��o do Player
        Vector3 direction = (player.position - transform.position).normalized;
        direction.y = 0;                                                    // Impede rota��o no eixo Y para que o Enemy n�o incline

        Quaternion lookRotation = Quaternion.LookRotation(direction);       // Calcula a rota��o desejada em dire��o ao Player

        // Interpola suavemente para a rota��o desejada
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * rotationSpeed);
    }

    private void MoveTowardsPlayer()
    {
        // Calcula a dire��o do Player
        Vector3 direction = (player.position - transform.position).normalized;

        Vector3 move = direction * moveSpeed * Time.deltaTime;              // Move o Enemy em dire��o ao Player usando o CharacterController
        move.y += Physics.gravity.y * Time.deltaTime;                       // Adiciona a gravidade manualmente para garantir que o inimigo se mova corretamente ao longo do eixo Y
        characterController.Move(move);                                     // Usa o CharacterController para mover o Enemy
        animator.SetBool("EnemyRunning", true);
    }

    // M�todo chamado pelo evento da anima��o para ativar o collider do machado
    public void EnableAxeCollider()
    {
        axeCollider.enabled = true;
    }

    // M�todo chamado pelo evento da anima��o para desativar o collider do machado
    public void DisableAxeCollider()
    {
        axeCollider.enabled = false;
    }

    private void AttackPlayer()
    {
        if (playerHealth != null && playerHealth.isDead)                    // Verificar se o Player est� morto
        {
            return;
        }

        animator.SetTrigger("EnemyAttack");                                 // Aciona a anima��o de ataque
        alreadyAttacked = true;                                             // Marca como j� atacado
        isAttackOnCooldown = true;                                          // Inicia o cooldown do ataque
        isAttacking = true;                                                 // Marca o Enemy como atacando

        PlaySound(attackSound);                                             // Toca o som de ataque
        StartCoroutine(AttackCooldown());                                   // Inicia a coroutine de cooldown
    }

    private IEnumerator AttackCooldown()
    {
        yield return new WaitForSeconds(attackCooldown);                    // Aguarda o tempo de cooldown
        isAttackOnCooldown = false;                                         // Reseta o estado de cooldown
        alreadyAttacked = false;                                            // Permite novos ataques
        isAttacking = false;                                                // Marca o Enemy como n�o atacando
    }

    public int GetAxeDamage()
    {
        return axeDamage;                                                   // Retorna o dano causado pelo machado
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isDead) return;                                                 // Se o Enemy est� morto, n�o faz nada

        // Verifica se o Enemy colidiu com o colisor da espada
        if (other.CompareTag("Sword"))
        {
            MovimentPlayer player = other.GetComponentInParent<MovimentPlayer>();
            if (player != null)
            {
                int swordDamage = player.GetSwordDamage();                  // Obt�m o dano da espada do Player
                TakeDamage(swordDamage);                                    // Aplica o dano ao Enemy
            }
        }
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;                                                 // Se o Enemy est� morto, n�o faz nada

        HP -= damage;
        HP = Mathf.Clamp(HP, 0, maxHP);                                     // Garante que HP esteja dentro dos limites

        UpdateHealthUI();                                                   // Atualiza a UI da sa�de

        if (animator != null)
        {
            animator.SetTrigger("EnemyHit");                                // Aciona a anima��o de dano
        }

        PlaySound(damageSound);                                             // Toca o som de dano

        if (HP <= 0)
        {
            Die();                                                          // Se HP � 0 ou menos, chama o m�todo Die()
        }
    }

    private void Die()
    {
        if (isDead) return;                                                 // Se j� estiver morto, n�o faz nada
        isDead = true;                                                      // Marca o Enemy como morto

        if (animator != null)                                               // Se o componente Animator estiver presente, ativa a anima��o de morte
        {
            animator.SetTrigger("EnemyDeath");
        }

        if (isPlayingIdleSound)
        {
            audioSource.Stop();                                             // Para o som de idle
            isPlayingIdleSound = false;
        }

        PlaySound(deathSound);                                              // Toca o som de morte

        if (regenerationCoroutine != null)                                  // Se a coroutine de regenera��o estiver rodando, pare-a
        {
            StopCoroutine(regenerationCoroutine);
        }
    }

    private IEnumerator RegenerateHealth()
    {
        while (!isDead)
        {
            yield return new WaitForSeconds(regenerationInterval);

            if (HP < maxHP)
            {
                HP += regenerationAmount;                                   // Regenera a sa�de
                HP = Mathf.Clamp(HP, 0, maxHP);                             // Garante que HP esteja dentro dos limites
                UpdateHealthUI();                                           // Atualiza a UI da sa�de
            }
        }
    }

    private void UpdateHealthUI()
    {
        if (healthBar != null)
        {
            healthBar.value = HP;                                           // Atualiza o valor da barra de vida
        }

        if (healthText != null)
        {
            healthText.text = $"{HP} / {maxHP}";                            // Atualiza o texto da barra de vida
        }
    }

    private void PlaySound(AudioClip clip, bool loop = false)
    {
        if (clip != null && audioSource != null)
        {
            if (loop)
            {
                if (!isPlayingIdleSound)                                    // Verifica se o som de idle j� est� tocando
                {
                    audioSource.clip = clip;
                    audioSource.loop = true;                                // Configura o �udio para tocar em loop
                    audioSource.Play();                                     // Come�a a tocar o som
                    isPlayingIdleSound = true;                              // Marca o som de idle como ativo
                }
            }
            else
            {
                audioSource.PlayOneShot(clip);                              // Toca o som uma vez, sem loop
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        // Desenha o gizmo para a dist�ncia de rota��o (cor amarela)
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, rotationDistance);

        // Desenha o gizmo para a dist�ncia de movimento (cor azul)
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, moveDistance);

        // Desenha o gizmo para a dist�ncia de ataque (cor vermelha)
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackDistance);
    }
}