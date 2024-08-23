using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnemyMoviment : MonoBehaviour, Interactable
{
    [SerializeField] private Dialog dialog;

    private Animator animator;
    private bool isDead = false;
    private int HP = 100;
    private int maxHP = 100;
    public Slider healthBar;
    public int damageAmount = 10;

    public void Interact()
    {
        if (!isDead) // S� pode interagir se n�o estiver morto
        {
            StartCoroutine(DialogManager.Instance.ShowDialog(dialog));
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        healthBar.maxValue = maxHP;
        healthBar.value = HP;
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if (isDead) return; // Se estiver morto, n�o fa�a mais nada

        healthBar.value = HP;

        if (Input.GetKeyDown(KeyCode.Q))
        {
            TakeDamage(damageAmount);
        }

        if (Input.GetKeyDown(KeyCode.Z) && !DialogManager.Instance.dialogBox.activeInHierarchy)
        {
            StartCoroutine(DialogManager.Instance.ShowDialog(dialog));
        }

        DialogManager.Instance.HandleUpdate();
    }

    private void TakeDamage(int damage)
    {
        if (isDead) return; // N�o faz nada se o inimigo estiver morto

        HP -= damage;

        if (HP < 0)
        {
            HP = 0;
        }

        healthBar.value = HP;

        if (animator != null)
        {
            animator.SetTrigger("EnemyHit"); // Ativa a anima��o de dano
        }

        if (HP <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        if (isDead) return; // Se j� estiver morto, n�o faz nada

        isDead = true; // Marca o inimigo como morto

        if (animator != null)
        {
            animator.SetTrigger("EnemyDeath"); // Ativa a anima��o de morte
        }
    }
}