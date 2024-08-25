using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]

public class Dialog
{
    // Lista privada de strings que cont�m as linhas de di�logo; 
    // [SerializeField] permite que essa lista seja vis�vel e edit�vel no inspetor do Unity, mas permanece privada no c�digo
    [SerializeField] private List<string> lines;

    // Propriedade p�blica para acessar a lista de linhas de di�logo fora da classe
    // Somente a leitura da lista � permitida, mas a modifica��o direta � protegida
    public List<string> Lines
    {
        // Retorna a lista de linhas de di�logo
        get { return lines; }
    }
}
