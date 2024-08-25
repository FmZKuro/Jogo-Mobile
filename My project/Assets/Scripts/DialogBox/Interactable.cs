using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Define uma interface chamada Interactable
public interface Interactable
{
    // Método que deve ser implementado por qualquer classe que herdar dessa interface
    // O objetivo é que qualquer objeto que implemente essa interface tenha uma função de interação
    void Interact();
}