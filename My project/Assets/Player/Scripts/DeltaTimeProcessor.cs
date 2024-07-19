using UnityEngine;
using UnityEngine.InputSystem;
using UnityEditor;

// Atributo que indica que a classe deve ser inicializada quando o editor de Unity carrega
[InitializeOnLoad]
public class DeltaTimeProcessor : InputProcessor<Vector2>
{
    // Método estático que é chamado quando a classe é carregada pela primeira vez
    static DeltaTimeProcessor()
    {
        Initialize(); // Chama o método Initialize para registrar o processador
    }

    // Método que é chamado antes de qualquer cena ser carregada
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Initialize()
    {
        // Registra o DeltaTimeProcessor no sistema de entrada da Unity
        InputSystem.RegisterProcessor<DeltaTimeProcessor>();
    }

    // Sobrescreve o método Process para aplicar deltaTime ao valor de entrada
    public override Vector2 Process(Vector2 value, InputControl control)
    {
        // Multiplica o valor de entrada pelo deltaTime para ajustar a entrada ao tempo do frame
        return value * Time.deltaTime;
    }
}
