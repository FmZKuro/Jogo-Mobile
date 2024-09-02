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
    [SerializeField] private int HP = 100;                                  // Declara uma variável privada para representar a vida atual no editor
    [SerializeField] private int maxHP = 100;                               // Declara uma variável privada para representar a vida máxima no editor
    [SerializeField] private float rotationSpeed = 5f;                      // Velocidade de rotação do Enemy
    [SerializeField] private float rotationDistance = 5f;                   // Distância para iniciar rotação do Enemy
    [SerializeField] private float moveSpeed = 3f;                          // Velocidade de movimento do Enemy
    [SerializeField] private float moveDistance = 4f;                       // Distância para o Enemy mover em direção ao Player

    [Header("Enemy Regeneration Settings")]
    [SerializeField] private int regenerationAmount = 2;                    // Quantidade de vida que o inimigo irá regenerar a cada intervalo
    [SerializeField] private int regenerationInterval = 5;                  // Intervalo de tempo (em segundos) entre as regenerações

    [Header("Enemy Attack Settings")]
    [SerializeField] private GameObject axe;                                // Referência ao GameObject do machado
    [SerializeField] private BoxCollider axeCollider;                       // Referência ao BoxCollider do machado
    [SerializeField] private int axeDamage = 4;                             // Dano do machado
    [SerializeField] private float attackDistance = 2f;                     // Distância de ataque
    [SerializeField] private float attackCooldown = 2f;                     // Tempo de cooldown entre ataques

    [Header("Enemy UI Settings")]
    [SerializeField] private Slider healthBar;                              // Declara uma variável para representar a barra de vida do enemy
    [SerializeField] private TextMeshProUGUI healthText;                    // Referência ao TextMeshPro para exibir o número

    [Header("Enemy Audio Settings")]
    [SerializeField] private AudioClip attackSound;                         // Som do ataque
    [SerializeField] private AudioClip damageSound;                         // Som de dano
    [SerializeField] private AudioClip deathSound;                          // Som de morte

    [Header("Enemy Idle Settings")]
    [SerializeField] private AudioClip idleSound;                           // Som de idle
    [SerializeField] float idleSoundRange = 10f;                            // Distância minima para ouvir o som de idle   

    private CharacterController characterController;                        // Referência ao Character Controller do Enemy
    private PlayerHealth playerHealth;                                      // Referência ao script de saúde do jogador
    private Coroutine regenerationCoroutine;                                // Armazena a referência da coroutine de regeneração    
    private Animator animator;                                              // Declara uma variável privada para representar o componente Animator
    private AudioSource audioSource;                                        // Componente AudioSource para tocar os sons
    public Transform player;                                                // obtêm o transform do Player
    private bool isDead = false;                                            // Flag para verificar se o Enemy está morto
    private bool isAttackOnCooldown = false;                                // Flag para verificar o cooldown do ataque
    private bool alreadyAttacked = false;                                   // Flag para verificar se o Enemy ja atacou
    private bool isPlayingIdleSound = false;                                // Flag para verificar se está executando o som de idle
    private bool isAttacking = false;                                       // Flag para verificar se está atacando

    private void Awake()
    {
        player = GameObject.Find("Player").transform;                       // Encontra e armazena a referência do jogador
        animator = GetComponent<Animator>();                                // Obtém e armazena o componente Animator do Enemy
        audioSource = GetComponent<AudioSource>();                          // Inicializa o componente AudioSource
        playerHealth = player.GetComponent<PlayerHealth>();                 // Encontre o componente PlayerHealth do jogador
        axeCollider.enabled = false;                                        // Ativa o BoxCollider do machado
        characterController = GetComponent<CharacterController>();          // Obtém o CharacterController do Enemy
    }

    // Start is called before the first frame update
    void Start()
    {
        PlaySound(idleSound, true);                                         // Inicia o som de idle em loop
        healthBar.maxValue = maxHP;                                         // Define o valor máximo da barra de vida com base no HP máximo do Enemy
        healthBar.value = HP;                                               // Define o valor atual da barra de vida com base no HP atual do Enemy

        if (healthText != null)
        {
            healthText.text = $" {HP} / {maxHP} ";                          // Atualiza o texto da barra de vida
        }

        regenerationCoroutine = StartCoroutine(RegenerateHealth());         // Inicia a coroutine de regeneração de vida        
    }

    // Update is called once per frame
    void Update()
    {
        if (isDead) return;                                                 // Se o Enemy está morto, não faz nada

        UpdateHealthUI();                                                   // Atualiza a UI da saúde
        HandlePlayerProximity();                                            // Lida com a proximidade do Player

        // Verifica a distância entre o Enemy e o Player
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer <= idleSoundRange)                             // Se a distância for menor que o alcance do som de idle e o som não está tocando, comece a tocar
        {
            if (!isPlayingIdleSound)
            {
                PlaySound(idleSound, true);                                 // Começa a tocar o som de idle em loop
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
        if (isAttacking)                                                    // Se o Enemy está atacando, não faz nada
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
                AttackPlayer();                                             // Ataca o Player se estiver próximo
            }
        }
    }

    private void RotateTowardsPlayer()
    {
        // Calcula a direção do Player
        Vector3 direction = (player.position - transform.position).normalized;
        direction.y = 0;                                                    // Impede rotação no eixo Y para que o Enemy não incline

        Quaternion lookRotation = Quaternion.LookRotation(direction);       // Calcula a rotação desejada em direção ao Player

        // Interpola suavemente para a rotação desejada
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * rotationSpeed);
    }

    private void MoveTowardsPlayer()
    {
        // Calcula a direção do Player
        Vector3 direction = (player.position - transform.position).normalized;

        Vector3 move = direction * moveSpeed * Time.deltaTime;              // Move o Enemy em direção ao Player usando o CharacterController
        move.y += Physics.gravity.y * Time.deltaTime;                       // Adiciona a gravidade manualmente para garantir que o inimigo se mova corretamente ao longo do eixo Y
        characterController.Move(move);                                     // Usa o CharacterController para mover o Enemy
        animator.SetBool("EnemyRunning", true);
    }

    // Método chamado pelo evento da animação para ativar o collider do machado
    public void EnableAxeCollider()
    {
        axeCollider.enabled = true;
    }

    // Método chamado pelo evento da animação para desativar o collider do machado
    public void DisableAxeCollider()
    {
        axeCollider.enabled = false;
    }

    private void AttackPlayer()
    {
        if (playerHealth != null && playerHealth.isDead)                    // Verificar se o Player está morto
        {
            return;
        }

        animator.SetTrigger("EnemyAttack");                                 // Aciona a animação de ataque
        alreadyAttacked = true;                                             // Marca como já atacado
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
        isAttacking = false;                                                // Marca o Enemy como não atacando
    }

    public int GetAxeDamage()
    {
        return axeDamage;                                                   // Retorna o dano causado pelo machado
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isDead) return;                                                 // Se o Enemy está morto, não faz nada

        // Verifica se o Enemy colidiu com o colisor da espada
        if (other.CompareTag("Sword"))
        {
            MovimentPlayer player = other.GetComponentInParent<MovimentPlayer>();
            if (player != null)
            {
                int swordDamage = player.GetSwordDamage();                  // Obtém o dano da espada do Player
                TakeDamage(swordDamage);                                    // Aplica o dano ao Enemy
            }
        }
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;                                                 // Se o Enemy está morto, não faz nada

        HP -= damage;
        HP = Mathf.Clamp(HP, 0, maxHP);                                     // Garante que HP esteja dentro dos limites

        UpdateHealthUI();                                                   // Atualiza a UI da saúde

        if (animator != null)
        {
            animator.SetTrigger("EnemyHit");                                // Aciona a animação de dano
        }

        PlaySound(damageSound);                                             // Toca o som de dano

        if (HP <= 0)
        {
            Die();                                                          // Se HP é 0 ou menos, chama o método Die()
        }
    }

    private void Die()
    {
        if (isDead) return;                                                 // Se já estiver morto, não faz nada
        isDead = true;                                                      // Marca o Enemy como morto

        if (animator != null)                                               // Se o componente Animator estiver presente, ativa a animação de morte
        {
            animator.SetTrigger("EnemyDeath");
        }

        if (isPlayingIdleSound)
        {
            audioSource.Stop();                                             // Para o som de idle
            isPlayingIdleSound = false;
        }

        PlaySound(deathSound);                                              // Toca o som de morte

        if (regenerationCoroutine != null)                                  // Se a coroutine de regeneração estiver rodando, pare-a
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
                HP += regenerationAmount;                                   // Regenera a saúde
                HP = Mathf.Clamp(HP, 0, maxHP);                             // Garante que HP esteja dentro dos limites
                UpdateHealthUI();                                           // Atualiza a UI da saúde
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
                if (!isPlayingIdleSound)                                    // Verifica se o som de idle já está tocando
                {
                    audioSource.clip = clip;
                    audioSource.loop = true;                                // Configura o áudio para tocar em loop
                    audioSource.Play();                                     // Começa a tocar o som
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
        // Desenha o gizmo para a distância de rotação (cor amarela)
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, rotationDistance);

        // Desenha o gizmo para a distância de movimento (cor azul)
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, moveDistance);

        // Desenha o gizmo para a distância de ataque (cor vermelha)
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackDistance);
    }
}