using UnityEngine;
using UnityEngine.InputSystem;
using UnityEditor;
[InitializeOnLoad]

public class DeltaTimeProcessor : InputProcessor<Vector2>
{
    static DeltaTimeProcessor()
    {
        Initialize();
    }
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Initialize()
    {
        InputSystem.RegisterProcessor<DeltaTimeProcessor>();
    }
    public override Vector2 Process(Vector2 value, InputControl control)
    {
        return value * Time.deltaTime;
    }
}