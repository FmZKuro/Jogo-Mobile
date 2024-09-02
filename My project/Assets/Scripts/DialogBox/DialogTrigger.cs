using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogTrigger : MonoBehaviour
{
    public Dialog dialog;  // O di�logo que voc� deseja mostrar ao colidir

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))  // Verifica se o objeto que colidiu tem a tag "Player"
        {
            TriggerDialog();
        }
    }

    private void TriggerDialog()
    {
        if (DialogManager.Instance != null)
        {
            StartCoroutine(DialogManager.Instance.ShowDialog(dialog));  // Inicia o di�logo ao colidir
        }
    }
}