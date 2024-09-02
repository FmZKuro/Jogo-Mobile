using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Countdown : MonoBehaviour
{
    [Header("CountDown Settings")]
    [SerializeField] TextMeshProUGUI countdownText;                                     // Refer�ncia ao componente TextMeshProUGUI que exibir� o texto da contagem regressiva
    [SerializeField] float remainingTime;                                               // Tempo restante para a contagem regressiva
    [SerializeField] float blinkStartTime = 15f;                                        // Tempo, em segundos, em que o texto come�ar� a piscar
    [SerializeField] float blinkFrequency = 0.5f;                                       // Frequ�ncia do piscar, em segundos (intervalo entre altern�ncia de cores)
    [SerializeField] private GameObject gameOverScreen;

    private bool isBlinking = false;                                                    // Vari�vel para controlar se o piscar est� ativo
    private float blinkTimer = 0f;                                                      // Temporizador usado para alternar as cores durante o piscar
    private Color originalColor;                                                        // Cor original do texto, salva para restaurar ap�s o piscar

    private void Start()
    {
        originalColor = countdownText.color;                                            // Armazena a cor original do texto para refer�ncia futura
    }

    // Update is called once per frame
    void Update()
    {
        if (remainingTime > 0)                                                          // Verifica se ainda h� tempo restante
        {
            remainingTime -= Time.deltaTime;                                            // Reduz o tempo restante com base no tempo decorrido desde o �ltimo frame
        }
        else if (remainingTime < 0)                                                     // Se o tempo restante for negativo (a contagem terminou)
        {
            remainingTime = 0;                                                          // Define o tempo restante como zero
            gameOverScreen.SetActive(true);
            countdownText.color = Color.red;                                            // Define a cor do texto para vermelho (indicando que o tempo acabou)
        }
                
        int minutes = Mathf.FloorToInt(remainingTime / 60);                             // Calcula os minutos restantes dividindo o tempo total por 60
        int seconds = Mathf.FloorToInt(remainingTime % 60);                             // Calcula os segundos restantes usando o m�dulo de 60
        countdownText.text = string.Format("{0:00}:{1:00}", minutes, seconds);          // Atualiza o texto da contagem regressiva no formato MM:SS

        if (remainingTime <= blinkStartTime && remainingTime > 0)                       // Verifica se o tempo restante est� abaixo do tempo definido para iniciar o piscar
        {
            if (!isBlinking)                                                            // Se o piscar ainda n�o come�ou, inicia o controle de piscar
            {
                isBlinking = true;
                blinkTimer = 0f;                                                        // Reseta o temporizador de piscar
            }

            blinkTimer += Time.deltaTime;                                               // Incrementa o temporizador de piscar com o tempo decorrido
            if (blinkTimer >= blinkFrequency)                                           // Se o temporizador ultrapassou a frequ�ncia definida para o piscar
            {
                // Alterna a cor entre a cor original e a cor vermelha
                countdownText.color = countdownText.color == originalColor ? Color.red : originalColor;
                blinkTimer = 0f;                                                        // Reseta o temporizador para reiniciar o ciclo de piscar
            }
        }
        else
        {
            isBlinking = false;                                                         // Define o piscar como inativo
            countdownText.color = originalColor;                                        // Garante que a cor do texto � restaurada para a cor original
        }
    }
}
