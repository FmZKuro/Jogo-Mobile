using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    public Image healthImage;                               // Refer�ncia de imagem que representa a Vida do Player
    public float maxHealth = 100f;                          // Valor m�ximo de Vida do Player

    [SerializeField] private float currentHealth;           // Valor atual de Vida do Player
    private Animator animator;                              // Refer�ncia ao componente Animator
    private bool isDead;                                    // Flag para verificar se o Player est� morto

    private MovimentPlayer movimentPlayer;                  // Refer�ncia ao script de movimento do Player

    void Start()
    {
        currentHealth = maxHealth;                          // Define a Vida atual com m�xima
        animator = GetComponent<Animator>();                // Obt�m o componente Animator
        movimentPlayer = GetComponent<MovimentPlayer>();    // Obt�m o script de movimento
        UpdateHealthBar();                                  // Atualiza a barra de Vida
    }

    void UpdateHealthBar()
    {
        // Calcula a propor��o de Vida atual em rea��o a Vida m�xima
        float healthRatio = currentHealth / maxHealth;

        // Ajusta a escala da imagem da Vida de acordo com a propor��o calculada
        healthImage.rectTransform.localScale = new Vector3(healthRatio, 1, 1);
    }

    public void TakeDamage(float damage)
    {
        // Se o Player j� estiver morto, n�o executa mais a��es
        if (isDead) return;

        // Reduz a Vida atual pelo valor do dano e garante q ela n�o fique abaixo de zero
        currentHealth -= damage;
        if (currentHealth < 0)
        {
            currentHealth = 0;
        }

        UpdateHealthBar();                                  // Atualiza a barra de Vida

        // Verifica se a vida chegou a 0 para acionar a anima��o de morte
        if (currentHealth == 0)
        {
            Die();                                          // Executa o m�todo de morte
        }
        else
        {
            movimentPlayer.enabled = false;                 // Desabilita o movimento

            animator.SetTrigger("TakeHit");                 // Aciona a anima��o de dano        
            StartCoroutine(EnableMovimentAfterDamage());    // Reabilita o movimento ap�s um curto per�odo de tempo
        }
    }

    private void Die()
    {
        isDead = true;                                      // Define o estado de morto
        animator.SetTrigger("Die");                         // Aciona a anima��o de morte como um trigger
        movimentPlayer.enabled = false;                     // Desabilita o movimento
    }

    private IEnumerator EnableMovimentAfterDamage()
    {
        // Espera at� o final da anima��o de dano
        yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);

        // Habilita o movimento do Player novamente
        movimentPlayer.enabled = true;
    }

    // Teste de cura
    public void Heal(float amount)
    {
        // Se o Player j� estiver morto, n�o permite curar
        if (isDead) return;

        // Aumenta a Vida atual pelo valor da Cura e garante que n�o exceda a vida m�xima
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