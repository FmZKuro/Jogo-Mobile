using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

// Classe responsável pelo movimento do personagem
public class Movimento : MonoBehaviour
{
    [SerializeField] private float velocidade = 4f; // Velocidade que o personagem irá se mover
    [SerializeField] private float jumpForce = 1f; // Força do pulo
    [SerializeField] private float gravity = -20f; // Gravidade aplicada ao personagem
    [SerializeField] private Button jumpButton; // Botão de pulo na UI
    [SerializeField] private Button attackButton; // Botão de ataque na UI
    [SerializeField] private Transform groundCheck; // Ponto de verificação do chão
    [SerializeField] private float groundDistance = 0.4f; // Distância para verificar o chão
    [SerializeField] private LayerMask groundMask; // Camada do chão

    private Vector2 myInput; // Vector2 que armazena os inputs do joystick de movimento
    private CharacterController characterController; // Referência ao componente de CharacterController do personagem
    private Transform myCamera; // Referência à câmera principal da cena
    private Animator animator; // Referência ao componente Animator do personagem
    private Vector3 velocity; // Armazena a velocidade para aplicar a gravidade e o pulo
    private bool isGrounded; // Verifica se o personagem está no chão

    // Método chamado ao iniciar o script
    private void Awake()
    {
        // Faz referência aos componentes necessários
        characterController = GetComponent<CharacterController>(); // Referencia o CharacterController
        animator = GetComponent<Animator>(); // Referencia o Animator
        myCamera = Camera.main.transform; // Referencia a câmera principal

        // Adiciona um listener ao botão de pulo para chamar a função Jump ao ser pressionado
        jumpButton.onClick.AddListener(Jump);

        // Adiciona um listener ao botão de ataque para chamar a função Attack ao ser pressionado
        attackButton.onClick.AddListener(Attack);
    }

    // Método chamado para mover o personagem, utilizando o sistema de entrada
    public void MoverPersonagem(InputAction.CallbackContext value)
    {
        myInput = value.ReadValue<Vector2>(); // Lê o valor da entrada do joystick de movimento
    }

    // Método chamado a cada frame
    private void Update()
    {
        // Verifica se o personagem está no chão
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask); // Verifica se está no chão
        animator.SetBool("isGrounded", isGrounded); // Atualiza o estado do Animator

        // Se o personagem está no chão e a velocidade vertical é menor que 0, redefine a velocidade vertical
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f; // Pequena força para manter o jogador no chão
            animator.SetBool("isJumping", false); // Atualiza o estado do Animator
        }

        // Chama o método para rotacionar o personagem
        RotacionarPersonagem();

        // Calcula o movimento do personagem e aplica usando o CharacterController
        Vector3 move = transform.forward * myInput.magnitude * velocidade * Time.deltaTime; // Calcula o movimento
        characterController.Move(move); // Move o personagem

        // Aplica a gravidade
        velocity.y += gravity * Time.deltaTime; // Atualiza a velocidade vertical com a gravidade
        characterController.Move(velocity * Time.deltaTime); // Move o personagem verticalmente

        // Atualiza o estado do Animator para a animação de andar
        animator.SetBool("andar", myInput != Vector2.zero);
    }

    // Método para rotacionar o personagem de acordo com a direção de entrada
    private void RotacionarPersonagem()
    {
        // Calcula a direção para frente e para a direita da câmera
        Vector3 forward = myCamera.TransformDirection(Vector3.forward); // Armazena um vetor que indica a direção "para frente"
        Vector3 right = myCamera.TransformDirection(Vector3.right); // Armazena um vetor que indica a direção "para o lado direito"
        Vector3 targetDirection = myInput.x * right + myInput.y * forward; // Calcula a direção alvo com base no input

        // Se há entrada do jogador e a direção de destino é significativa, rotaciona o personagem
        if (myInput != Vector2.zero && targetDirection.magnitude > 0.1f) // Verifica se o Input é diferente de 0 e se a magnitude (intensidade) do input é maior do que 0.1, em uma escala de 0 a 1. Ou seja, desconsidera pequenos movimentos no joystick.
        {
            Quaternion freeRotation = Quaternion.LookRotation(targetDirection.normalized); // Cria uma rotação com a direção alvo
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(new Vector3(transform.eulerAngles.x, freeRotation.eulerAngles.y, transform.eulerAngles.z)), 10 * Time.deltaTime); // Aplica a rotação ao personagem. O método Quaternion.Slerp aplica uma suavização na rotação, para que ela não aconteça de forma abrupta
        }
    }

    // Método chamado ao pressionar o botão de pulo
    private void Jump()
    {
        // Se o personagem está no chão, aplica a força de pulo
        if (isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpForce * -2f * gravity); // Calcula a velocidade de pulo com base na força de pulo e gravidade
            animator.SetBool("isJumping", true); // Atualiza o estado do Animator para a animação de pulo
        }
    }

    // Método chamado ao pressionar o botão de ataque
    private void Attack()
    {
        animator.SetTrigger("attack"); // Define o gatilho de ataque no Animator
    }
}