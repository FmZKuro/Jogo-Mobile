using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DialogManager : MonoBehaviour
{
    [SerializeField] public GameObject dialogBox;
    [SerializeField] private Text dialogText;
    [SerializeField] private int lettersPerSecond;

    public event Action OnShowDialog;
    public event Action OnCloseDialog;

    public static DialogManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private Dialog dialog;
    private int currentLine = 0;
    private bool isTyping;

    public IEnumerator ShowDialog(Dialog dialog)
    {
        yield return new WaitForEndOfFrame();

        OnShowDialog?.Invoke();

        this.dialog = dialog;
        dialogBox.SetActive(true);
        currentLine = 0;
        StartCoroutine(TypeDialog(dialog.Lines[currentLine]));
    }

    public void HandleUpdate()
    {
        if (Input.GetKeyDown(KeyCode.Z) && !isTyping && dialogBox.activeInHierarchy)
        {
            AdvanceDialog();
        }
    }

    private void AdvanceDialog()
    {
        if (currentLine < dialog.Lines.Count - 1)
        {
            currentLine++;
            StopAllCoroutines();
            StartCoroutine(TypeDialog(dialog.Lines[currentLine]));
        }
        else
        {
            CloseDialog();
        }
    }

    private void CloseDialog()
    {
        StopAllCoroutines();
        currentLine = 0;
        dialogBox.SetActive(false);
        OnCloseDialog?.Invoke();
    }

    public IEnumerator TypeDialog(string line)
    {
        isTyping = true;
        dialogText.text = "";
        foreach (var letter in line.ToCharArray())
        {
            dialogText.text += letter;
            yield return new WaitForSeconds(1f / lettersPerSecond);
        }
        isTyping = false;
    }
}
