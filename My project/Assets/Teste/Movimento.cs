using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

// Classe respons�vel pelo movimento do personagem
public class Movimento : MonoBehaviour
{
    [SerializeField] private float velocidade = 4f; // Velocidade que o personagem ir� se mover
    [SerializeField] private float jumpForce = 1f; // For�a do pulo
    [SerializeField] private float gravity = -20f; // Gravidade aplicada ao personagem
    [SerializeField] private Button jumpButton; // Bot�o de pulo na UI
    [SerializeField] private Button attackButton; // Bot�o de ataque na UI
    [SerializeField] private Transform groundCheck; // Ponto de verifica��o do ch�o
    [SerializeField] private float groundDistance = 0.4f; // Dist�ncia para verificar o ch�o
    [SerializeField] private LayerMask groundMask; // Camada do ch�o

    private Vector2 myInput; // Vector2 que armazena os inputs do joystick de movimento
    private CharacterController characterController; // Refer�ncia ao componente de CharacterController do personagem
    private Transform myCamera; // Refer�ncia � c�mera principal da cena
    private Animator animator; // Refer�ncia ao componente Animator do personagem
    private Vector3 velocity; // Armazena a velocidade para aplicar a gravidade e o pulo
    private bool isGrounded; // Verifica se o personagem est� no ch�o

    // M�todo chamado ao iniciar o script
    private void Awake()
    {
        // Faz refer�ncia aos componentes necess�rios
        characterController = GetComponent<CharacterController>(); // Referencia o CharacterController
        animator = GetComponent<Animator>(); // Referencia o Animator
        myCamera = Camera.main.transform; // Referencia a c�mera principal

        // Adiciona um listener ao bot�o de pulo para chamar a fun��o Jump ao ser pressionado
        jumpButton.onClick.AddListener(Jump);

        // Adiciona um listener ao bot�o de ataque para chamar a fun��o Attack ao ser pressionado
        attackButton.onClick.AddListener(Attack);
    }

    // M�todo chamado para mover o personagem, utilizando o sistema de entrada
    public void MoverPersonagem(InputAction.CallbackContext value)
    {
        myInput = value.ReadValue<Vector2>(); // L� o valor da entrada do joystick de movimento
    }

    // M�todo chamado a cada frame
    private void Update()
    {
        // Verifica se o personagem est� no ch�o
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask); // Verifica se est� no ch�o
        animator.SetBool("isGrounded", isGrounded); // Atualiza o estado do Animator

        // Se o personagem est� no ch�o e a velocidade vertical � menor que 0, redefine a velocidade vertical
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f; // Pequena for�a para manter o jogador no ch�o
            animator.SetBool("isJumping", false); // Atualiza o estado do Animator
        }

        // Chama o m�todo para rotacionar o personagem
        RotacionarPersonagem();

        // Calcula o movimento do personagem e aplica usando o CharacterController
        Vector3 move = transform.forward * myInput.magnitude * velocidade * Time.deltaTime; // Calcula o movimento
        characterController.Move(move); // Move o personagem

        // Aplica a gravidade
        velocity.y += gravity * Time.deltaTime; // Atualiza a velocidade vertical com a gravidade
        characterController.Move(velocity * Time.deltaTime); // Move o personagem verticalmente

        // Atualiza o estado do Animator para a anima��o de andar
        animator.SetBool("andar", myInput != Vector2.zero);
    }

    // M�todo para rotacionar o personagem de acordo com a dire��o de entrada
    private void RotacionarPersonagem()
    {
        // Calcula a dire��o para frente e para a direita da c�mera
        Vector3 forward = myCamera.TransformDirection(Vector3.forward); // Armazena um vetor que indica a dire��o "para frente"
        Vector3 right = myCamera.TransformDirection(Vector3.right); // Armazena um vetor que indica a dire��o "para o lado direito"
        Vector3 targetDirection = myInput.x * right + myInput.y * forward; // Calcula a dire��o alvo com base no input

        // Se h� entrada do jogador e a dire��o de destino � significativa, rotaciona o personagem
        if (myInput != Vector2.zero && targetDirection.magnitude > 0.1f) // Verifica se o Input � diferente de 0 e se a magnitude (intensidade) do input � maior do que 0.1, em uma escala de 0 a 1. Ou seja, desconsidera pequenos movimentos no joystick.
        {
            Quaternion freeRotation = Quaternion.LookRotation(targetDirection.normalized); // Cria uma rota��o com a dire��o alvo
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(new Vector3(transform.eulerAngles.x, freeRotation.eulerAngles.y, transform.eulerAngles.z)), 10 * Time.deltaTime); // Aplica a rota��o ao personagem. O m�todo Quaternion.Slerp aplica uma suaviza��o na rota��o, para que ela n�o aconte�a de forma abrupta
        }
    }

    // M�todo chamado ao pressionar o bot�o de pulo
    private void Jump()
    {
        // Se o personagem est� no ch�o, aplica a for�a de pulo
        if (isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpForce * -2f * gravity); // Calcula a velocidade de pulo com base na for�a de pulo e gravidade
            animator.SetBool("isJumping", true); // Atualiza o estado do Animator para a anima��o de pulo
        }
    }

    // M�todo chamado ao pressionar o bot�o de ataque
    private void Attack()
    {
        animator.SetTrigger("attack"); // Define o gatilho de ataque no Animator
    }
}