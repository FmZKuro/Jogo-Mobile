using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    public Image healthImage;               // Refer�ncia de imagem que representa a Vida do Player
    public float maxHealth = 100f;          // Valor m�ximo de Vida do Player
    private float currentHealth;            // Valor atual de Vida do Player

    void Start()
    {
        currentHealth = maxHealth;          // Define a Vida atual com m�xima
        UpdateHealthBar();                  // Atualiza a barra de Vida
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
        // Reduz a Vida atual pelo valor do dano e garante q ela n�o fique abaixo de zero
        currentHealth -= damage;
        if (currentHealth < 0)
        {
            currentHealth = 0;
        }

        // Atualiza a barra de Vida
        UpdateHealthBar();
    }

    // Teste de cura
    public void Heal(float amount)
    {
        // Aumenta a Vida atual pelo valor da Cura e garante que n�o exceda a vida m�xima
        currentHealth += amount;
        if (currentHealth > maxHealth)
        {
            currentHealth = maxHealth;
        }

        // Atualiza a barra de Vida
        UpdateHealthBar();
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