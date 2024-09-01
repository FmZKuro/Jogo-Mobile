using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public AudioSource backgroundMusic;         // Refer�ncia ao componente AudioSource que ser� usado para tocar a m�sica de fundo

    void Start()
    {
        if (backgroundMusic != null)            // Verifica se o AudioSource de m�sica de fundo n�o � nulo (ou seja, se foi atribu�do no Inspector)
        {
            backgroundMusic.loop = true;        // Define o �udio para repetir continuamente
            backgroundMusic.Play();             // Inicia a reprodu��o do �udio
        }
    }
}
