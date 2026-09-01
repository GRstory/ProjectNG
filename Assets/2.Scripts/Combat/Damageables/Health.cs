using System;
using UnityEngine;

namespace GRstory.Combat
{
    public class Health : MonoBehaviour, IDamageable
    {
        [SerializeField] private float _maxHealth;
        [SerializeField] private bool _isInvincible = false;

        [field: SerializeField] public float CurrentHealth { get; private set; }

        public float MaxHealth => _maxHealth;

        public event Action<DamageContext> OnHit;
        public event Action<float> OnHealed;
        public event Action OnDied;

        #region Monobehaviour
        private void Awake()
        {
            CurrentHealth = _maxHealth;
        }

        #endregion


        public void GetDamage(DamageContext context)
        {
            if (_isInvincible) return;

            CurrentHealth = Mathf.Clamp(CurrentHealth - context.Damage, 0, _maxHealth);
            OnHit?.Invoke(context);

            if (CurrentHealth <= 0)
            {
                OnDied?.Invoke();
            }
        }

        public void Heal(float amount)
        {
            if (CurrentHealth <= 0) return; // 사망 후 회복 불가

            CurrentHealth = Mathf.Clamp(CurrentHealth + amount, 0, _maxHealth);
            OnHealed?.Invoke(amount);
        }
    }
}