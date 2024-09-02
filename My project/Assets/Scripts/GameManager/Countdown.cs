using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Countdown : MonoBehaviour
{
    [Header("CountDown Settings")]
    [SerializeField] TextMeshProUGUI countdownText;                                     // Referência ao componente TextMeshProUGUI que exibirá o texto da contagem regressiva
    [SerializeField] float remainingTime;                                               // Tempo restante para a contagem regressiva
    [SerializeField] float blinkStartTime = 15f;                                        // Tempo, em segundos, em que o texto começará a piscar
    [SerializeField] float blinkFrequency = 0.5f;                                       // Frequência do piscar, em segundos (intervalo entre alternância de cores)
    [SerializeField] private GameObject gameOverScreen;

    private bool isBlinking = false;                                                    // Variável para controlar se o piscar está ativo
    private float blinkTimer = 0f;                                                      // Temporizador usado para alternar as cores durante o piscar
    private Color originalColor;                                                        // Cor original do texto, salva para restaurar após o piscar

    private void Start()
    {
        originalColor = countdownText.color;                                            // Armazena a cor original do texto para referência futura
    }

    // Update is called once per frame
    void Update()
    {
        if (remainingTime > 0)                                                          // Verifica se ainda há tempo restante
        {
            remainingTime -= Time.deltaTime;                                            // Reduz o tempo restante com base no tempo decorrido desde o último frame
        }
        else if (remainingTime < 0)                                                     // Se o tempo restante for negativo (a contagem terminou)
        {
            remainingTime = 0;                                                          // Define o tempo restante como zero
            gameOverScreen.SetActive(true);
            countdownText.color = Color.red;                                            // Define a cor do texto para vermelho (indicando que o tempo acabou)
        }
                
        int minutes = Mathf.FloorToInt(remainingTime / 60);                             // Calcula os minutos restantes dividindo o tempo total por 60
        int seconds = Mathf.FloorToInt(remainingTime % 60);                             // Calcula os segundos restantes usando o módulo de 60
        countdownText.text = string.Format("{0:00}:{1:00}", minutes, seconds);          // Atualiza o texto da contagem regressiva no formato MM:SS

        if (remainingTime <= blinkStartTime && remainingTime > 0)                       // Verifica se o tempo restante está abaixo do tempo definido para iniciar o piscar
        {
            if (!isBlinking)                                                            // Se o piscar ainda não começou, inicia o controle de piscar
            {
                isBlinking = true;
                blinkTimer = 0f;                                                        // Reseta o temporizador de piscar
            }

            blinkTimer += Time.deltaTime;                                               // Incrementa o temporizador de piscar com o tempo decorrido
            if (blinkTimer >= blinkFrequency)                                           // Se o temporizador ultrapassou a frequência definida para o piscar
            {
                // Alterna a cor entre a cor original e a cor vermelha
                countdownText.color = countdownText.color == originalColor ? Color.red : originalColor;
                blinkTimer = 0f;                                                        // Reseta o temporizador para reiniciar o ciclo de piscar
            }
        }
        else
        {
            isBlinking = false;                                                         // Define o piscar como inativo
            countdownText.color = originalColor;                                        // Garante que a cor do texto é restaurada para a cor original
        }
    }
}
