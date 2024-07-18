using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Movimento : MonoBehaviour
{
    [SerializeField] private float velocidade = 4; // Velocidade que o personagem ir� se mover

    private Vector2 myInput; // Vector2 que armazena os inputs do joystick de movimento
    private CharacterController characterController; // Refer�ncia ao componente de CharacterController do personagem
    private Transform myCamera; // Refer�ncia a c�mera principal da cena
    private Animator animator; // Refer�ncia ao componente Animator do personagem

    private void Awake()
    {
        characterController = GetComponent<CharacterController>(); // Nesse c�digo � feito a referencia��o
        animator = GetComponent<Animator>(); // Nesse c�digo � feito a referencia��o
        myCamera = Camera.main.transform; // Nesse c�digo � feito a referencia��o
    }
   
    public void MoverPersonagem(InputAction.CallbackContext value)
    {
        myInput = value.ReadValue<Vector2>();
    }
    
    private void Update()
    {
        RotacionarPersonagem(); // Chama o m�todo para definir a rota��o do personagem
        characterController.Move(transform.forward * myInput.magnitude * velocidade * Time.deltaTime);        
        characterController.Move(Vector3.down * 9.81f * Time.deltaTime);

        animator.SetBool("andar", myInput != Vector2.zero);
    }

    private void RotacionarPersonagem()
    {
        Vector3 forward = myCamera.TransformDirection(Vector3.forward); // Armazena um vetor que indica a dire��o "para frente"
        
        Vector3 right = myCamera.TransformDirection(Vector3.right); // Armazena um vetor que indica a dire��o "para o lado direito"

        Vector3 targetDirection = myInput.x * right + myInput.y * forward; 


        if (myInput != Vector2.zero && targetDirection.magnitude > 0.1f) // Verifica se o Input � diferente de 0 e se a magnitude(intesidade) do input � maior do que 0.1, em uma escala de 0 a 1. Ou seja, desconsidera pequenos movimentos no joystick.
        {
            Quaternion freeRotation = Quaternion.LookRotation(targetDirection.normalized); // Cria uma rota��o com as dire��es forward. Ou seja, retorna uma rota��o indicando a dire��o alvo.
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(new Vector3(transform.eulerAngles.x, freeRotation.eulerAngles.y, transform.eulerAngles.z)), 10 * Time.deltaTime); // Aplica a rota��o ao personagem. O m�todo Quaternion.Slerp aplica uma suaviza��o na rota��o, para que ela n�o aconte�a de forma abrupta
        }
    }

}
