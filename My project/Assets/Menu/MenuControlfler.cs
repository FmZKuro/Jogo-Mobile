using UnityEngine;
using UnityEngine.SceneManagement;

public class ActivateObjects : MonoBehaviour
{
    // Variáveis serializadas para controle do menu
    [SerializeField] private GameObject menu;
    [SerializeField] private GameObject play;
    [SerializeField] private GameObject setting;



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


    //metodo para caregar  de cena 
    public void LoadScene()
    {
        SceneManager.LoadScene("Teste");
    }

    public void LoadMenu()
    {
        SceneManager.LoadScene("Menu");
    }


    // metodo para fechar o jogo
    public void Exit()
    {
        Application.Quit();

    }



}
