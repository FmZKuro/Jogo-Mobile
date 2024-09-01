using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuMain : MonoBehaviour
{

    //=== Variáveis ===
    //serializadas para controle do menu
    [SerializeField] private GameObject menu;
    [SerializeField] private GameObject menuInGame;
    [SerializeField] private GameObject setting;
    [SerializeField] private GameObject historia;
    [SerializeField] private GameObject gameOver;

    //Controle do caregamento de cenas
    private string scenaAtual;

    // Pausar o jogo
    private bool isPaused = false;


    //=== Metodo de botoes ===
    public void Menu()
    {
        menu.SetActive(true);
        setting.SetActive(false);
        historia.SetActive(false);
    }

    public void Setting()
    {
        menu.SetActive(false);
        setting.SetActive(true);
        historia.SetActive(false);
    }
    public void Historia()
    {
        menu.SetActive(false);
        setting.SetActive(false);
        historia.SetActive(true);
    }

    public void GameOver()
    {
        gameOver.SetActive(true);
    }

    public void Exit()
    {
        Application.Quit();
        Debug.Log("Exit - o jogo fechou");
    }

   

    //pausar o jogo
    public void Pause()
    {

        // Se o jogo não estiver pausado, pausamos o tempo do jogo.
       
       Time.timeScale = 0f;
       isPaused = true;
        
       menuInGame.SetActive(true);

    }

    // retornar do pause
    public void ResumeGame()
    {
        if (isPaused)
        {
            Time.timeScale = 1f; // Retoma o tempo normal do jogo
            isPaused = false;
        }
        menuInGame.SetActive(false); // Fecha o menu
    }


    //=== Caregar  de cena ===

    // Menu
    public void LoadMenu()
    {
        SceneManager.LoadScene("Menu");
    }

    public void LoadFase()
    {
        SceneManager.LoadScene("Fase01");
    }

    // Game play - Atualização
    public void LoadScene(string sceneName)
    {
        scenaAtual = sceneName;
        if (scenaAtual == "Menu")
        {
            LoadScene("Fase01");
        }

        else if (scenaAtual == "Fase01")
        {
            LoadScene("Fase02");
        }
        else if (scenaAtual == "Fase02")
        {
            LoadScene("Fase03");
        }
        else
        {
            LoadScene("Menu");
        }
    }


    // Game play - Vitoria
    public void Vitoria()
    {
        LoadScene("Vitoria");
    }

    // Game play - Restart fase
    public void ReStart()
    {
        LoadScene(scenaAtual);
    }
}
