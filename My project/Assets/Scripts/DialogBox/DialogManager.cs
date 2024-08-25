using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DialogManager : MonoBehaviour
{
    [SerializeField] public GameObject dialogBox;                           // Referência ao GameObject da caixa de diálogo, que será atribuída no inspetor do Unity
    [SerializeField] private Text dialogText;                               // Referência ao componente Text que exibirá o texto do diálogo
    [SerializeField] private int lettersPerSecond;                          // Velocidade de exibição das letras por segundo durante a digitação do diálogo

    public event Action OnShowDialog;                                       // Evento que é acionado quando o diálogo é exibido
    public event Action OnCloseDialog;                                      // Evento que é acionado quando o diálogo é fechado

    public static DialogManager Instance { get; private set; }              // Instância Static do DialogManager para garantir que exista apenas uma instância no jogo

    private void Awake()
    {
        if (Instance == null)                                               // Verifica se não há outra instância do DialogManager
        {
            Instance = this;                                                // Se não houver, define essa instância como a atual
        }
        else
        {
            Destroy(gameObject);                                            // Se já existir outra instância, destrói esse GameObject para garantir que haja apenas um DialogManager
        }
    }

    private Dialog dialog;                                                  // Referência ao diálogo atual que está sendo exibido
    private int currentLine = 0;                                            // Índice da linha atual do diálogo que está sendo exibida
    private bool isTyping;                                                  // Booleano que indica se o texto do diálogo está sendo digitado no momento

    public IEnumerator ShowDialog(Dialog dialog)                            // Coroutine para exibir o diálogo na tela
    {
        yield return new WaitForEndOfFrame();                               // Aguarda até o final do frame para iniciar a exibição do diálogo

        OnShowDialog?.Invoke();                                             // Aciona o evento OnShowDialog

        this.dialog = dialog;                                               // Armazena o diálogo recebido como o diálogo atual
        dialogBox.SetActive(true);                                          // Ativa a caixa de diálogo
        currentLine = 0;                                                    // Define a linha atual como a primeira linha do diálogo
        StartCoroutine(TypeDialog(dialog.Lines[currentLine]));              // Inicia a exibição da primeira linha do diálogo
    }

    // Método que deve ser chamado a cada frame para lidar com a atualização do diálogo
    public void HandleUpdate()
    {
        // Verifica se a tecla "Z" foi pressionada, se o texto não está sendo digitado no momento e se a caixa de diálogo está ativa
        if (Input.GetKeyDown(KeyCode.Z) && !isTyping && dialogBox.activeInHierarchy)
        {
            AdvanceDialog();                                                // Avança para a próxima linha do diálogo
        }
    }

    // Método privado para avançar para a próxima linha do diálogo ou fechar o diálogo se todas as linhas tiverem sido exibidas
    private void AdvanceDialog()
    {
        if (currentLine < dialog.Lines.Count - 1)                           // Verifica se ainda há mais linhas para exibir
        {
            currentLine++;                                                  // Avança para a próxima linha
            StopAllCoroutines();                                            // Para todas as coroutines em execução para garantir que apenas a nova linha seja exibida
            StartCoroutine(TypeDialog(dialog.Lines[currentLine]));          // Inicia a exibição da nova linha do diálogo
        }
        else
        {
            CloseDialog();                                                  // Se todas as linhas foram exibidas, fecha o diálogo
        }
    }

    // Método privado para fechar o diálogo
    private void CloseDialog()
    {
        StopAllCoroutines();                                                // Para todas as coroutines em execução
        currentLine = 0;                                                    // Reseta o índice da linha atual para zero
        dialogBox.SetActive(false);                                         // Desativa a caixa de diálogo
        OnCloseDialog?.Invoke();                                            // Aciona o evento OnCloseDialog
    }

    // Coroutine responsável por exibir uma linha do diálogo, letra por letra
    public IEnumerator TypeDialog(string line)
    {
        isTyping = true;                                                    // Marca que o texto está sendo digitado
        dialogText.text = "";                                               // Limpa o texto atual na caixa de diálogo
        foreach (var letter in line.ToCharArray())                          // Para cada letra na linha de texto, adiciona-a ao dialogText e espera um intervalo antes de exibir a próxima
        {
            dialogText.text += letter;
            yield return new WaitForSeconds(1f / lettersPerSecond);         // Intervalo baseado na velocidade configurada
        }
        isTyping = false;                                                   // Marca que o texto terminou de ser digitado
    }
}
