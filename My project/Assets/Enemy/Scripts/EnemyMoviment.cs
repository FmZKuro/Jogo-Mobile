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

    [Header("Enemy Regeneration Settings")]
    [SerializeField] private int regenerationAmount = 5;                    // Quantidade de vida que o inimigo ir� regenerar a cada intervalo
    [SerializeField] private int regenerationInterval = 5;                  // Intervalo de tempo (em segundos) entre as regenera��es

    [Header("Enemy Attack Settings")]
    [SerializeField] private GameObject axe;                                // Refer�ncia ao GameObject do machado
    [SerializeField] private BoxCollider axeCollider;                       // Refer�ncia ao BoxCollider do machado
    [SerializeField] private int axeDamage = 10;                            // Dano do machado
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

   

    [Header("Enemy Dialog Settings")]
    [SerializeField] private Dialog dialog;                                 // Declara uma vari�vel privada do tipo Dialog para acesso no editor
    private Coroutine regenerationCoroutine;                                // Armazena a refer�ncia da coroutine de regenera��o    
    private Animator animator;                                              // Declara uma vari�vel privada para representar o componente Animator
    private AudioSource audioSource;                                        // Componente AudioSource para tocar os sons
    public Transform player;                                                // obt�m o transform do Player
    private bool isDead = false;                                            // Flag para verificar se o Enemy est� morto
    private bool isAttackOnCooldown = false;                                // Flag para verificar o cooldown do ataque
    private bool alreadyAttacked = false;                                   // Flag para verificar se o Enemy ja atacou
    private bool isPlayingIdleSound = false;                                // Flag para verificar se est� executando o som de idle

    private void Awake()
    {
        player = GameObject.Find("Player").transform;                       // Encontra e armazena a refer�ncia do jogador
        animator = GetComponent<Animator>();                                // Obt�m e armazena o componente Animator do Enemy
        axeCollider.enabled = false;                                        // Ativa o BoxCollider do machado
        audioSource = GetComponent<AudioSource>();                          // Inicializa o componente AudioSource
    }


    // M�todo para interagir com o Personagem
    public void Interact()
    {
        if (!isDead)                                                        // S� pode interagir se n�o estiver morto
        {
            StartCoroutine(DialogManager.Instance.ShowDialog(dialog));      // Inicia uma coroutine para mostrar o di�logo associado ao Enemy
        }
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

        // teste
        if (Input.GetKeyDown(KeyCode.Z) && !DialogManager.Instance.dialogBox.activeInHierarchy)
        {
            StartCoroutine(DialogManager.Instance.ShowDialog(dialog));      // Inicia o di�logo se a tecla Z for pressionada
        }

        DialogManager.Instance.HandleUpdate();                              // Atualiza o di�logo, se necess�rio
        // teste
    }

    private void HandlePlayerProximity()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer <= attackDistance)
        {
            if (!alreadyAttacked && !isAttackOnCooldown)
            {
                AttackPlayer();                                             // Ataca o Player se estiver pr�ximo
            }
        }
        else
        {
            animator.SetBool("EnemyRunning", false);                        // Parar anima��o de caminhada
        }
    }

    private void AttackPlayer()
    {
        animator.SetTrigger("EnemyAttack");                                 // Aciona a anima��o de ataque
        alreadyAttacked = true;                                             // Marca como j� atacado
        isAttackOnCooldown = true;                                          // Inicia o cooldown do ataque
        axeCollider.enabled = true;

        PlaySound(attackSound);                                             // Toca o som de ataque
        Invoke(nameof(ResetAttack), 1f);                                    // Reseta o estado de ataque ap�s um segundo
        StartCoroutine(AttackCooldown());                                   // Inicia a coroutine de cooldown
    }

    private IEnumerator AttackCooldown()
    {
        yield return new WaitForSeconds(attackCooldown);                    // Aguarda o tempo de cooldown
        isAttackOnCooldown = false;                                         // Reseta o estado de cooldown
        alreadyAttacked = false;                                            // Permite novos ataques
        axeCollider.enabled = false;
    }

    private void ResetAttack()
    {
        alreadyAttacked = false;                                            // Permite novos ataques
        axeCollider.enabled = false;
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
                    audioSource.loop = true;
                    audioSource.Play();
                    isPlayingIdleSound = true;
                }
            }
            else
            {
                audioSource.PlayOneShot(clip);
            }
        }
    }
}