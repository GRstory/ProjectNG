using System;
using GRstory.SaveSystem;
using UnityEngine;

namespace GRstory.Combat
{
    public class Health : MonoBehaviour, IDamageable, IPlayerData
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

        public void CaptureData(PlayerSnapshot snapshot)
        {
            snapshot.MaxHealth = _maxHealth;
            snapshot.CurrentHealth = CurrentHealth;
        }

        // 복원은 이벤트를 쏘지 않는다. 피격/회복 연출이 아니라 상태 재구성이기 때문
        public void RestoreData(PlayerSnapshot snapshot)
        {
            if (snapshot.MaxHealth > 0f)
                _maxHealth = snapshot.MaxHealth;

            CurrentHealth = Mathf.Clamp(snapshot.CurrentHealth, 0f, _maxHealth);
        }
    }
}