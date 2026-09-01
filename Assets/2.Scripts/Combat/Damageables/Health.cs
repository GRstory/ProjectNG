using System;
using UnityEngine;

namespace GRstory.Combat
{
    public class Health : MonoBehaviour, IDamageable
    {
        [SerializeField] private float _maxHealth;
        [SerializeField] private bool _isInvincible = false;

        [field: SerializeField] public float CurrentHealth { get; private set; }

        public event Action<DamageContext> OnHit;
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
    }
}