using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    public Image healthImage;                               // Referência de imagem que representa a Vida do Player
    public float maxHealth = 100f;                          // Valor máximo de Vida do Player

    [SerializeField] private float currentHealth;           // Valor atual de Vida do Player
    private Animator animator;                              // Referência ao componente Animator
    private bool isDead;                                    // Flag para verificar se o Player está morto

    private MovimentPlayer movimentPlayer;                  // Referência ao script de movimento do Player

    void Start()
    {
        currentHealth = maxHealth;                          // Define a Vida atual com máxima
        animator = GetComponent<Animator>();                // Obtém o componente Animator
        movimentPlayer = GetComponent<MovimentPlayer>();    // Obtém o script de movimento
        UpdateHealthBar();                                  // Atualiza a barra de Vida
    }

    void UpdateHealthBar()
    {
        // Calcula a proporção de Vida atual em reação a Vida máxima
        float healthRatio = currentHealth / maxHealth;

        // Ajusta a escala da imagem da Vida de acordo com a proporção calculada
        healthImage.rectTransform.localScale = new Vector3(healthRatio, 1, 1);
    }

    public void TakeDamage(float damage)
    {
        // Se o Player já estiver morto, não executa mais ações
        if (isDead) return;

        // Reduz a Vida atual pelo valor do dano e garante q ela não fique abaixo de zero
        currentHealth -= damage;
        if (currentHealth < 0)
        {
            currentHealth = 0;
        }

        UpdateHealthBar();                                  // Atualiza a barra de Vida

        // Verifica se a vida chegou a 0 para acionar a animação de morte
        if (currentHealth == 0)
        {
            Die();                                          // Executa o método de morte
        }
        else
        {
            movimentPlayer.enabled = false;                 // Desabilita o movimento

            animator.SetTrigger("TakeHit");                 // Aciona a animação de dano        
            StartCoroutine(EnableMovimentAfterDamage());    // Reabilita o movimento após um curto período de tempo
        }
    }

    private void Die()
    {
        isDead = true;                                      // Define o estado de morto
        animator.SetTrigger("Die");                         // Aciona a animação de morte como um trigger
        movimentPlayer.enabled = false;                     // Desabilita o movimento
    }

    private IEnumerator EnableMovimentAfterDamage()
    {
        // Espera até o final da animação de dano
        yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);

        // Habilita o movimento do Player novamente
        movimentPlayer.enabled = true;
    }

    // Teste de cura
    public void Heal(float amount)
    {
        // Se o Player já estiver morto, não permite curar
        if (isDead) return;

        // Aumenta a Vida atual pelo valor da Cura e garante que não exceda a vida máxima
        currentHealth += amount;
        if (currentHealth > maxHealth)
        {
            currentHealth = maxHealth;
        }
        
        UpdateHealthBar();                          // Atualiza a barra de Vida
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