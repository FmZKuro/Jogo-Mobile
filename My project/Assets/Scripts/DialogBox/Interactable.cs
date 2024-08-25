using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Define uma interface chamada Interactable
public interface Interactable
{
    // M�todo que deve ser implementado por qualquer classe que herdar dessa interface
    // O objetivo � que qualquer objeto que implemente essa interface tenha uma fun��o de intera��o
    void Interact();
}