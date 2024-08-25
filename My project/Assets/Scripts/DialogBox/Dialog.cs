using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]

public class Dialog
{
    // Lista privada de strings que contém as linhas de diálogo; 
    // [SerializeField] permite que essa lista seja visível e editável no inspetor do Unity, mas permanece privada no código
    [SerializeField] private List<string> lines;

    // Propriedade pública para acessar a lista de linhas de diálogo fora da classe
    // Somente a leitura da lista é permitida, mas a modificação direta é protegida
    public List<string> Lines
    {
        // Retorna a lista de linhas de diálogo
        get { return lines; }
    }
}
