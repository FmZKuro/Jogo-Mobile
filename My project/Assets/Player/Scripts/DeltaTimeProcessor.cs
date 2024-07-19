using UnityEngine;
using UnityEngine.InputSystem;
using UnityEditor;

// Atributo que indica que a classe deve ser inicializada quando o editor de Unity carrega
[InitializeOnLoad]
public class DeltaTimeProcessor : InputProcessor<Vector2>
{
    // M�todo est�tico que � chamado quando a classe � carregada pela primeira vez
    static DeltaTimeProcessor()
    {
        Initialize(); // Chama o m�todo Initialize para registrar o processador
    }

    // M�todo que � chamado antes de qualquer cena ser carregada
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Initialize()
    {
        // Registra o DeltaTimeProcessor no sistema de entrada da Unity
        InputSystem.RegisterProcessor<DeltaTimeProcessor>();
    }

    // Sobrescreve o m�todo Process para aplicar deltaTime ao valor de entrada
    public override Vector2 Process(Vector2 value, InputControl control)
    {
        // Multiplica o valor de entrada pelo deltaTime para ajustar a entrada ao tempo do frame
        return value * Time.deltaTime;
    }
}
