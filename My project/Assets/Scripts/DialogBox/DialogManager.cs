using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DialogManager : MonoBehaviour
{
    [SerializeField] public GameObject dialogBox;                           // Refer�ncia ao GameObject da caixa de di�logo, que ser� atribu�da no inspetor do Unity
    [SerializeField] private Text dialogText;                               // Refer�ncia ao componente Text que exibir� o texto do di�logo
    [SerializeField] private int lettersPerSecond;                          // Velocidade de exibi��o das letras por segundo durante a digita��o do di�logo

    public event Action OnShowDialog;                                       // Evento que � acionado quando o di�logo � exibido
    public event Action OnCloseDialog;                                      // Evento que � acionado quando o di�logo � fechado

    private Dialog dialog;                                                  // Refer�ncia ao di�logo atual que est� sendo exibido
    private int currentLine = 0;                                            // �ndice da linha atual do di�logo que est� sendo exibida
    private bool isTyping;                                                  // Booleano que indica se o texto do di�logo est� sendo digitado no momento
    private bool dialogShown = false;                                       // Flag para controlar se o di�logo j� foi exibido

    public static DialogManager Instance { get; private set; }              // Inst�ncia Static do DialogManager para garantir que exista apenas uma inst�ncia no jogo

    private void Awake()
    {
        if (Instance == null)                                               // Verifica se n�o h� outra inst�ncia do DialogManager
        {
            Instance = this;                                                // Se n�o houver, define essa inst�ncia como a atual
        }
        else
        {
            Destroy(gameObject);                                            // Se j� existir outra inst�ncia, destr�i esse GameObject para garantir que haja apenas um DialogManager
        }
    }


    public IEnumerator ShowDialog(Dialog dialog)                            // Coroutine para exibir o di�logo na tela
    {
        if (dialogShown) yield break;                                       // Se o di�logo j� foi exibido, n�o faz nada

        dialogShown = true;
        yield return new WaitForEndOfFrame();

        OnShowDialog?.Invoke();

        this.dialog = dialog;                                               // Armazena o di�logo recebido como o di�logo atual
        dialogBox.SetActive(true);                                          // Ativa a caixa de di�logo
        currentLine = 0;                                                    // Define a linha atual como a primeira linha do di�logo
        StartCoroutine(TypeDialog(dialog.Lines[currentLine]));              // Inicia a exibi��o da primeira linha do di�logo
    }

    // M�todo que deve ser chamado a cada frame para lidar com a atualiza��o do di�logo
    public void HandleUpdate()
    {
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began && !isTyping && dialogBox.activeInHierarchy)
        {
            AdvanceDialog();                                                // Avan�a para a pr�xima linha do di�logo
        }



        // Teste
        // Verifica se o bot�o do mouse foi clicado
        else if (Input.GetMouseButtonDown(0) && !isTyping && dialogBox.activeInHierarchy)
        {
            AdvanceDialog();                                                // Avan�a para a pr�xima linha do di�logo
        }
    }
    // Teste



    // M�todo privado para avan�ar para a pr�xima linha do di�logo ou fechar o di�logo se todas as linhas tiverem sido exibidas
    private void AdvanceDialog()
    {
        if (currentLine < dialog.Lines.Count - 1)                           // Verifica se ainda h� mais linhas para exibir
        {
            currentLine++;                                                  // Avan�a para a pr�xima linha
            StopAllCoroutines();                                            // Para todas as coroutines em execu��o para garantir que apenas a nova linha seja exibida
            StartCoroutine(TypeDialog(dialog.Lines[currentLine]));          // Inicia a exibi��o da nova linha do di�logo
        }
        else
        {
            CloseDialog();                                                  // Se todas as linhas foram exibidas, fecha o di�logo
        }
    }

    // M�todo privado para fechar o di�logo
    private void CloseDialog()
    {
        StopAllCoroutines();                                                // Para todas as coroutines em execu��o
        currentLine = 0;                                                    // Reseta o �ndice da linha atual para zero
        dialogBox.SetActive(false);                                         // Desativa a caixa de di�logo
        OnCloseDialog?.Invoke();                                            // Aciona o evento OnCloseDialog
    }

    // Coroutine respons�vel por exibir uma linha do di�logo, letra por letra
    public IEnumerator TypeDialog(string line)
    {
        isTyping = true;                                                    // Marca que o texto est� sendo digitado
        dialogText.text = "";                                               // Limpa o texto atual na caixa de di�logo
        foreach (var letter in line.ToCharArray())                          // Para cada letra na linha de texto, adiciona-a ao dialogText e espera um intervalo antes de exibir a pr�xima
        {
            dialogText.text += letter;
            yield return new WaitForSeconds(1f / lettersPerSecond);         // Intervalo baseado na velocidade configurada
        }
        isTyping = false;                                                   // Marca que o texto terminou de ser digitado
    }
}
