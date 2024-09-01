using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public AudioSource backgroundMusic;         // Referência ao componente AudioSource que será usado para tocar a música de fundo

    void Start()
    {
        if (backgroundMusic != null)            // Verifica se o AudioSource de música de fundo não é nulo (ou seja, se foi atribuído no Inspector)
        {
            backgroundMusic.loop = true;        // Define o áudio para repetir continuamente
            backgroundMusic.Play();             // Inicia a reprodução do áudio
        }
    }
}
