using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private Image healthImage;             // Refer�ncia de imagem que representa a Vida do Player
    [SerializeField] private TextMeshProUGUI healthText;    // Refer�ncia ao TextMeshPro para exibir o n�mero de HP
    [SerializeField] private float maxHealth = 100f;        // Valor m�ximo de Vida do Player
    [SerializeField] private float currentHealth;           // Valor atual de Vida do Player
    [SerializeField] private GameObject gameOverHP;


    [Header("Audio Settings")]
    public AudioClip deathSound;                            // Refer�ncia ao som de morte do Player
    public AudioClip damageSound;                           // Refer�ncia ao som de dano do Player

    private Animator animator;                              // Refer�ncia ao componente Animator
    private AudioSource audioSource;                        // Refer�ncia ao componente AudioSource
    private MovimentPlayer movimentPlayer;                  // Refer�ncia ao script de movimento do Player
    public bool isDead;                                     // Flag para verificar se o Player est� morto

    void Start()
    {
        InitializeHealth();                                 // Inicializa a sa�de do Player
        InitializeComponents();                             // Inicializa os componentes necess�rios
        UpdateHealthBar();                                  // Atualiza a barra de Vida
    }

    private void InitializeHealth()
    {
        currentHealth = maxHealth;                          // Define a Vida atual para o valor m�ximo
    }

    private void InitializeComponents()
    {
        animator = GetComponent<Animator>();                // Obt�m o componente Animator
        audioSource = GetComponent<AudioSource>();          // Obt�m o componente AudioSource
        movimentPlayer = GetComponent<MovimentPlayer>();    // Obt�m o script de movimento
    }

    private void UpdateHealthBar()
    {
        float healthRatio = currentHealth / maxHealth;              // Calcula a propor��o de Vida atual em rela��o � Vida m�xima

        // Ajusta a escala da imagem da barra de Vida de acordo com a propor��o calculada
        healthImage.rectTransform.localScale = new Vector3(healthRatio, 1, 1);

        if (healthText != null)                                     // Se o texto de Vida est� atribu�do
        {
            healthText.text = $"{currentHealth} / {maxHealth}";     // Atualiza o texto com a Vida atual e m�xima
        }
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;                                             // Se o Player j� estiver morto, ignora o dano

        currentHealth -= damage;                                        // Reduz a Vida atual pelo valor do dano
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);       // Garante que a Vida n�o seja menor que zero

        UpdateHealthBar();                                              // Atualiza a barra de Vida ap�s o dano

        PlayDamageSound();                                              // Toca o som de dano

        if (currentHealth <= 0)                                         // Verifica se o Player est� morto
        {
            Die();                                                      // Executa o m�todo de morte
            

        }
        else
        {
            TriggerDamageAnimation();                                   // Aciona a anima��o de dano
            StartCoroutine(EnableMovementAfterDamage());                // Reabilita o movimento ap�s um curto per�odo
        }
    }

    private void PlayDamageSound()
    {
        if (damageSound != null && !isDead)                 // Toca o som de dano se estiver atribu�do e o Player n�o estiver morto
        {
            audioSource.PlayOneShot(damageSound);           // Reproduz o som de dano
        }
    }

    private void TriggerDamageAnimation()
    {
        movimentPlayer.enabled = false;                     // Desabilita o movimento durante a anima��o de dano
        animator.SetTrigger("TakeHit");                     // Aciona a anima��o de dano
    }

    private void Die()
    {
        isDead = true;                                      // Define o estado de morto do Player
        animator.SetTrigger("Die");                         // Aciona a anima��o de morte
        movimentPlayer.SetIsDead(true);                     // Desabilita o movimento do Player

        PlayDeathSound();                                   // Toca o som de morte

        StartCoroutine(HandleDeathSequence());                 // Desativa todos os sons ap�s o som de morte ser tocado

    }

    private void PlayDeathSound()
    {
        if (deathSound != null)                             // Verifica se o som de morte foi atribu�do
        {
            audioSource.PlayOneShot(deathSound);            // Toca o som de morte
        }
    }

    private IEnumerator HandleDeathSequence()
    {

        if (deathSound != null)                             // Espera at� o final do som de morte
        {
            yield return new WaitForSeconds(deathSound.length);
        }      

        yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);

        gameOverHP.SetActive(true);
    }

    private IEnumerator EnableMovementAfterDamage()
    {
        // Espera at� o final da anima��o de dano
        yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);

        movimentPlayer.enabled = true;                      // Habilita o movimento do Player novamente
    }
}