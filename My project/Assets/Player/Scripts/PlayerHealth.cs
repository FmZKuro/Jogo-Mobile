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

    [Header("Audio Settings")]
    public AudioClip deathSound;                            // Referência ao som de morte do Player
    public AudioClip damageSound;                           // Referência ao som de dano do Player

    private Animator animator;                              // Referência ao componente Animator
    private AudioSource audioSource;                        // Referência ao componente AudioSource
    private MovimentPlayer movimentPlayer;                  // Referência ao script de movimento do Player
    private bool isDead;                                    // Flag para verificar se o Player está morto

    void Start()
    {
        currentHealth = maxHealth;                          // Define a Vida atual com máxima
        animator = GetComponent<Animator>();                // Obtém o componente Animator
        movimentPlayer = GetComponent<MovimentPlayer>();    // Obtém o script de movimento
        audioSource = GetComponent<AudioSource>();          // Obtém o componente AudioSource
        UpdateHealthBar();                                  // Atualiza a barra de Vida
    }

    void UpdateHealthBar()
    {        
        float healthRatio = currentHealth / maxHealth;      // Calcula a proporção de Vida atual em reação a Vida máxima

        // Ajusta a escala da imagem da Vida de acordo com a proporção calculada
        healthImage.rectTransform.localScale = new Vector3(healthRatio, 1, 1);
                
        if (healthText != null)                             // Atualiza o texto com a vida atual e máxima
        {
            healthText.text = $"{currentHealth} / {maxHealth}";
        }
    }

    public void TakeDamage(int damage)
    {        
        if (isDead) return;                                 // Se o Player já estiver morto, não executa mais ações

        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth); // Garante que a saúde não fique abaixo de zero

        UpdateHealthBar();                                  // Atualiza a barra de saúde
                
        if (damageSound != null)                            // Toca o som de dano se não estiver morto e o som estiver atribuído
        {
            audioSource.PlayOneShot(damageSound);
        }

        if (currentHealth <= 0)
        {
            Die();                                          // Executa o método de morte
        }
        else
        {
            movimentPlayer.enabled = false;                 // Desabilita o movimento
            animator.SetTrigger("TakeHit");                 // Aciona a animação de dano        
            StartCoroutine(EnableMovimentAfterDamage());    // Reabilita o movimento após um curto período
        }
    }

    private void Die()
    {
        isDead = true;                                      // Define o estado de morto
        animator.SetTrigger("Die");                         // Aciona a animação de morte como um trigger
        movimentPlayer.SetIsDead(true);                     // Desabilita o movimento

        if (deathSound != null)                             // Verifica se o som de morte foi atribuído
        {
            audioSource.PlayOneShot(deathSound);            // Toca o som de morte
        }
        
        StartCoroutine(DisableAllSounds());                 // Desabilita todos os sons após o som de morte ser tocado
    }

    private IEnumerator DisableAllSounds()
    {        
        if (deathSound != null)                             // Espera até o final do som de morte
        {
            yield return new WaitForSeconds(deathSound.length);
        }

        // Desativa o componente AudioSource para parar todos os sons
        audioSource.Stop();
        audioSource.enabled = false;
    }

    private IEnumerator EnableMovimentAfterDamage()
    {
        // Espera até o final da animação de dano
        yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);
                
        movimentPlayer.enabled = true;                      // Habilita o movimento do Player novamente
    }

    // Teste de cura
    public void Heal(float amount)
    {        
        if (isDead) return;                                 // Se o Player já estiver morto, não permite curar
        
        currentHealth += amount;                            // Aumenta a Vida atual pelo valor da Cura e garante que não exceda a vida máxima
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