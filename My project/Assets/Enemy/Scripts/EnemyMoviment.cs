using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnemyMoviment : MonoBehaviour
{
    private int HP = 100;
    private int maxHP = 100;
    public Slider healthBar;
    public int damageAmount = 10;

    // Start is called before the first frame update
    void Start()
    {
        healthBar.maxValue = maxHP;
        healthBar.value = HP;
    }

    // Update is called once per frame
    void Update()
    {
        healthBar.value = HP;

        if (Input.GetKeyDown(KeyCode.Q))
        {
            TakeDamage(damageAmount);
        }
    }

    private void TakeDamage(int damage)
    {
        HP -= damage;

        if (HP < 0)
        {
            HP = 0;
        }

        healthBar.value = HP;

        if (HP <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Debug.Log("Inimigo Morreu!");
        Destroy(gameObject);
    }
}
