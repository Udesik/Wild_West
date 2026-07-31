using UnityEngine;
using System;

public class Health : MonoBehaviour
{
    [SerializeField] private float _maxHealth;

    private float _countHealth;

    public event Action<float, float> OnHealthChanged;
    public event Action Died;

    private void OnEnable()
    {
        _countHealth = _maxHealth;
    }

    public void TakeDamage(float damage)
    {
        _countHealth -= damage;
        _countHealth = Mathf.Clamp(_countHealth, 0, _maxHealth);
        
        OnHealthChanged?.Invoke(_countHealth, _maxHealth);

        if (_countHealth == 0)
        {
            Died?.Invoke();
        }
    }

    public void Heal(float heal)
    {
        _countHealth += heal;
        OnHealthChanged?.Invoke(_countHealth, _maxHealth);
    }

    public void SetBoss(int bossHP)
    {
        _countHealth = bossHP;
    }

    public void SetEnemy()
    {
        _countHealth = _maxHealth;
    }
}
