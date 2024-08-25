using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnemyMoviment : MonoBehaviour, Interactable
{
    [Header("Enemy Movement Settings")]
    [SerializeField] private int HP = 100;                                  // Declara uma vari�vel privada para representar a vida atual no editor
    [SerializeField] private int maxHP = 100;                               // Declara uma vari�vel privada para representar a vida m�xima no editor

    [Header("Enemy UI Settings")]
    [SerializeField] private Slider healthBar;                              // Declara uma vari�vel para representar a barra de vida do enemy

    [Header("Enemy Dialog Settings")]
    [SerializeField] private Dialog dialog;                                 // Declara uma vari�vel privada do tipo Dialog para acesso no editor

    private Animator animator;                                              // Declara uma vari�vel privada para representar o componente Animator
    private bool isDead = false;                                            // Declara uma vari�vel booleana para representar se o Enemy est� morto

    // M�todo para interagir com o Personagem
    public void Interact()
    {
        if (!isDead)                                                        // S� pode interagir se n�o estiver morto
        {
            StartCoroutine(DialogManager.Instance.ShowDialog(dialog));      // Inicia uma coroutine para mostrar o di�logo associado ao Enemy
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        healthBar.maxValue = maxHP;                                         // Define o valor m�ximo da barra de vida com base no HP m�ximo do Enemy
        healthBar.value = HP;                                               // Define o valor atual da barra de vida com base no HP atual do Enemy
        animator = GetComponent<Animator>();                                // Obt�m e armazena o componente Animator do Enemy
    }

    // Update is called once per frame
    void Update()
    {
        if (isDead) return;                                                 // Se estiver morto, n�o fa�a mais nada
        healthBar.value = HP;                                               // Atualiza o valor da barra de vida com o valor atual do HP

        // Verifica se a tecla "Z" foi pressionada e se a caixa de di�logo n�o est� ativa
        if (Input.GetKeyDown(KeyCode.Z) && !DialogManager.Instance.dialogBox.activeInHierarchy)
        {
            StartCoroutine(DialogManager.Instance.ShowDialog(dialog));      // Inicia uma coroutine para mostrar o di�logo associado ao Enemy
        }

        DialogManager.Instance.HandleUpdate();                              // Chama o m�todo HandleUpdate do DialogManager para atualizar o estado do di�logo
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isDead) return;                                                 // Se estiver morto, ignora a colis�o

        // Verifica se o inimigo colidiu com o colisor da espada
        if (other.CompareTag("Sword"))
        {
            // Obt�m o dano da espada a partir do script do Player
            MovimentPlayer player = other.GetComponentInParent<MovimentPlayer>();
            if (player != null)
            {
                int swordDamage = player.GetSwordDamage();                  // Obt�m o valor do dano da espada
                TakeDamage(swordDamage);                                    // Aplica dano ao inimigo com base no dano da espada
            }
        }
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;                                                 // Se estiver morto, n�o fa�a mais nada
        HP -= damage;                                                       // Reduz o HP do personagem com base na quantidade de dano recebido

        if (HP < 0)                                                         // Garante que o HP n�o seja menor que zero
        {
            HP = 0;
        }

        healthBar.value = HP;                                               // Atualiza o valor da barra de vida ap�s o dano

        if (animator != null)                                               // Se o componente Animator estiver presente, ativa a anima��o de dano
        {
            animator.SetTrigger("EnemyHit");
        }

        if (HP <= 0)                                                        // Verifica se o HP do Enemy chegou a zero para determinar se ele deve morrer
        {
            Die();                                                          // Chama o m�todo Die para tratar a morte do Enemy
        }
    }

    private void Die()
    {
        if (isDead) return;                                                 // Se j� estiver morto, n�o faz nada
        isDead = true;                                                      // Marca o inimigo como morto

        if (animator != null)                                               // Se o componente Animator estiver presente, ativa a anima��o de morte
        {
            animator.SetTrigger("EnemyDeath");
        }
    }
}