using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuControllerr : MonoBehaviour
{   
    
    //=== Variáveis ===
    //serializadas para controle do menu
    [SerializeField] private GameObject menu;
    [SerializeField] private GameObject play;
    [SerializeField] private GameObject setting;
    [SerializeField] private GameObject gameOver;

    //Controle do caregamento de cenas
    private string scenaAtual;


    //=== Metodo de botoes ===
    public void Menu()
    {
        menu.SetActive(true);
        setting.SetActive(false);
    }

    public void Setting()
    {
        menu.SetActive(false);
        setting.SetActive(true);
    }

    public void GameOver()
    {
        gameOver.SetActive(true);
    }

    public void Exit()
    {
        Application.Quit();
    }




    //=== Caregar  de cena ===

    // Menu
    public void LoadMenu()
    {
        SceneManager.LoadScene("Menu");
    }

    // Game play - Atualização
    public void LoadScene(string sceneName)
    {
        scenaAtual = sceneName;
        SceneManager.LoadScene(sceneName);
    }

    // Game play - Caregar 
    public void levelComplete()
    {
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
