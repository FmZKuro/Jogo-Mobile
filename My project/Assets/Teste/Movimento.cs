using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Movimento : MonoBehaviour
{
    [SerializeField] private float velocidade = 4; // Velocidade que o personagem irá se mover

    private Vector2 myInput; // Vector2 que armazena os inputs do joystick de movimento
    private CharacterController characterController; // Referência ao componente de CharacterController do personagem
    private Transform myCamera; // Referência a câmera principal da cena
    private Animator animator; // Referência ao componente Animator do personagem

    private void Awake()
    {
        characterController = GetComponent<CharacterController>(); // Nesse código é feito a referenciação
        animator = GetComponent<Animator>(); // Nesse código é feito a referenciação
        myCamera = Camera.main.transform; // Nesse código é feito a referenciação
    }
   
    public void MoverPersonagem(InputAction.CallbackContext value)
    {
        myInput = value.ReadValue<Vector2>();
    }
    
    private void Update()
    {
        RotacionarPersonagem(); // Chama o método para definir a rotação do personagem
        characterController.Move(transform.forward * myInput.magnitude * velocidade * Time.deltaTime);        
        characterController.Move(Vector3.down * 9.81f * Time.deltaTime);

        animator.SetBool("andar", myInput != Vector2.zero);
    }

    private void RotacionarPersonagem()
    {
        Vector3 forward = myCamera.TransformDirection(Vector3.forward); // Armazena um vetor que indica a direção "para frente"
        
        Vector3 right = myCamera.TransformDirection(Vector3.right); // Armazena um vetor que indica a direção "para o lado direito"

        Vector3 targetDirection = myInput.x * right + myInput.y * forward; 


        if (myInput != Vector2.zero && targetDirection.magnitude > 0.1f) // Verifica se o Input é diferente de 0 e se a magnitude(intesidade) do input é maior do que 0.1, em uma escala de 0 a 1. Ou seja, desconsidera pequenos movimentos no joystick.
        {
            Quaternion freeRotation = Quaternion.LookRotation(targetDirection.normalized); // Cria uma rotação com as direções forward. Ou seja, retorna uma rotação indicando a direção alvo.
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(new Vector3(transform.eulerAngles.x, freeRotation.eulerAngles.y, transform.eulerAngles.z)), 10 * Time.deltaTime); // Aplica a rotação ao personagem. O método Quaternion.Slerp aplica uma suavização na rotação, para que ela não aconteça de forma abrupta
        }
    }

}
