using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private Image healthImage;             // Referência de imagem que representa a Vida do Player
    [SerializeField] private TextMeshProUGUI healthText;    // Referência ao TextMeshPro para exibir o número de HP
    [SerializeField] private float maxHealth = 100f;        // Valor máximo de Vida do Player
    [SerializeField] private float currentHealth;           // Valor atual de Vida do Player
    [SerializeField] private GameObject gameOverHP;


    [Header("Audio Settings")]
    public AudioClip deathSound;                            // Referência ao som de morte do Player
    public AudioClip damageSound;                           // Referência ao som de dano do Player

    private Animator animator;                              // Referência ao componente Animator
    private AudioSource audioSource;                        // Referência ao componente AudioSource
    private MovimentPlayer movimentPlayer;                  // Referência ao script de movimento do Player
    public bool isDead;                                     // Flag para verificar se o Player está morto

    void Start()
    {
        InitializeHealth();                                 // Inicializa a saúde do Player
        InitializeComponents();                             // Inicializa os componentes necessários
        UpdateHealthBar();                                  // Atualiza a barra de Vida
    }

    private void InitializeHealth()
    {
        currentHealth = maxHealth;                          // Define a Vida atual para o valor máximo
    }

    private void InitializeComponents()
    {
        animator = GetComponent<Animator>();                // Obtém o componente Animator
        audioSource = GetComponent<AudioSource>();          // Obtém o componente AudioSource
        movimentPlayer = GetComponent<MovimentPlayer>();    // Obtém o script de movimento
    }

    private void UpdateHealthBar()
    {
        float healthRatio = currentHealth / maxHealth;              // Calcula a proporção de Vida atual em relação à Vida máxima

        // Ajusta a escala da imagem da barra de Vida de acordo com a proporção calculada
        healthImage.rectTransform.localScale = new Vector3(healthRatio, 1, 1);

        if (healthText != null)                                     // Se o texto de Vida está atribuído
        {
            healthText.text = $"{currentHealth} / {maxHealth}";     // Atualiza o texto com a Vida atual e máxima
        }
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;                                             // Se o Player já estiver morto, ignora o dano

        currentHealth -= damage;                                        // Reduz a Vida atual pelo valor do dano
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);       // Garante que a Vida não seja menor que zero

        UpdateHealthBar();                                              // Atualiza a barra de Vida após o dano

        PlayDamageSound();                                              // Toca o som de dano

        if (currentHealth <= 0)                                         // Verifica se o Player está morto
        {
            Die();                                                      // Executa o método de morte
            

        }
        else
        {
            TriggerDamageAnimation();                                   // Aciona a animação de dano
            StartCoroutine(EnableMovementAfterDamage());                // Reabilita o movimento após um curto período
        }
    }

    private void PlayDamageSound()
    {
        if (damageSound != null && !isDead)                 // Toca o som de dano se estiver atribuído e o Player não estiver morto
        {
            audioSource.PlayOneShot(damageSound);           // Reproduz o som de dano
        }
    }

    private void TriggerDamageAnimation()
    {
        movimentPlayer.enabled = false;                     // Desabilita o movimento durante a animação de dano
        animator.SetTrigger("TakeHit");                     // Aciona a animação de dano
    }

    private void Die()
    {
        isDead = true;                                      // Define o estado de morto do Player
        animator.SetTrigger("Die");                         // Aciona a animação de morte
        movimentPlayer.SetIsDead(true);                     // Desabilita o movimento do Player

        PlayDeathSound();                                   // Toca o som de morte

        StartCoroutine(HandleDeathSequence());                 // Desativa todos os sons após o som de morte ser tocado

    }

    private void PlayDeathSound()
    {
        if (deathSound != null)                             // Verifica se o som de morte foi atribuído
        {
            audioSource.PlayOneShot(deathSound);            // Toca o som de morte
        }
    }

    private IEnumerator HandleDeathSequence()
    {

        if (deathSound != null)                             // Espera até o final do som de morte
        {
            yield return new WaitForSeconds(deathSound.length);
        }      

        yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);

        gameOverHP.SetActive(true);
    }

    private IEnumerator EnableMovementAfterDamage()
    {
        // Espera até o final da animação de dano
        yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);

        movimentPlayer.enabled = true;                      // Habilita o movimento do Player novamente
    }
}