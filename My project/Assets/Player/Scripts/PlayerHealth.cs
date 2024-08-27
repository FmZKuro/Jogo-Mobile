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

    [Header("Audio Settings")]
    public AudioClip deathSound;                            // Refer�ncia ao som de morte do Player
    public AudioClip damageSound;                           // Refer�ncia ao som de dano do Player

    private Animator animator;                              // Refer�ncia ao componente Animator
    private AudioSource audioSource;                        // Refer�ncia ao componente AudioSource
    private MovimentPlayer movimentPlayer;                  // Refer�ncia ao script de movimento do Player
    private bool isDead;                                    // Flag para verificar se o Player est� morto

    void Start()
    {
        currentHealth = maxHealth;                          // Define a Vida atual com m�xima
        animator = GetComponent<Animator>();                // Obt�m o componente Animator
        movimentPlayer = GetComponent<MovimentPlayer>();    // Obt�m o script de movimento
        audioSource = GetComponent<AudioSource>();          // Obt�m o componente AudioSource
        UpdateHealthBar();                                  // Atualiza a barra de Vida
    }

    void UpdateHealthBar()
    {        
        float healthRatio = currentHealth / maxHealth;      // Calcula a propor��o de Vida atual em rea��o a Vida m�xima

        // Ajusta a escala da imagem da Vida de acordo com a propor��o calculada
        healthImage.rectTransform.localScale = new Vector3(healthRatio, 1, 1);
                
        if (healthText != null)                             // Atualiza o texto com a vida atual e m�xima
        {
            healthText.text = $"{currentHealth} / {maxHealth}";
        }
    }

    public void TakeDamage(int damage)
    {        
        if (isDead) return;                                 // Se o Player j� estiver morto, n�o executa mais a��es

        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth); // Garante que a sa�de n�o fique abaixo de zero

        UpdateHealthBar();                                  // Atualiza a barra de sa�de
                
        if (damageSound != null)                            // Toca o som de dano se n�o estiver morto e o som estiver atribu�do
        {
            audioSource.PlayOneShot(damageSound);
        }

        if (currentHealth <= 0)
        {
            Die();                                          // Executa o m�todo de morte
        }
        else
        {
            movimentPlayer.enabled = false;                 // Desabilita o movimento
            animator.SetTrigger("TakeHit");                 // Aciona a anima��o de dano        
            StartCoroutine(EnableMovimentAfterDamage());    // Reabilita o movimento ap�s um curto per�odo
        }
    }

    private void Die()
    {
        isDead = true;                                      // Define o estado de morto
        animator.SetTrigger("Die");                         // Aciona a anima��o de morte como um trigger
        movimentPlayer.SetIsDead(true);                     // Desabilita o movimento

        if (deathSound != null)                             // Verifica se o som de morte foi atribu�do
        {
            audioSource.PlayOneShot(deathSound);            // Toca o som de morte
        }
        
        StartCoroutine(DisableAllSounds());                 // Desabilita todos os sons ap�s o som de morte ser tocado
    }

    private IEnumerator DisableAllSounds()
    {        
        if (deathSound != null)                             // Espera at� o final do som de morte
        {
            yield return new WaitForSeconds(deathSound.length);
        }

        // Desativa o componente AudioSource para parar todos os sons
        audioSource.Stop();
        audioSource.enabled = false;
    }

    private IEnumerator EnableMovimentAfterDamage()
    {
        // Espera at� o final da anima��o de dano
        yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);
                
        movimentPlayer.enabled = true;                      // Habilita o movimento do Player novamente
    }

    // Teste de cura
    public void Heal(float amount)
    {        
        if (isDead) return;                                 // Se o Player j� estiver morto, n�o permite curar
        
        currentHealth += amount;                            // Aumenta a Vida atual pelo valor da Cura e garante que n�o exceda a vida m�xima
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        UpdateHealthBar();                                  // Atualiza a barra de Vida
    }
    // Fim teste de Cura

    // Teste de dano
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            TakeDamage(10);
        }

        if (Input.GetKeyDown(KeyCode.H))
        {
            Heal(10);
        }
    }
    // Fim teste de dano
}