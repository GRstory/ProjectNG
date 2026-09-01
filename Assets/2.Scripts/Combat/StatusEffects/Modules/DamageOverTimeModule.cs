using UnityEngine;

namespace GRstory.Combat
{
    [CreateAssetMenu(fileName = "DamageOverTimeModule", menuName = "Combat/Modules/Damage Over Time")]
    public class DamageOverTimeModule : StatusEffectModule
    {
        [SerializeField] private float _damagePerTick;

        public override void OnTick(StatusEffectInstance instance)
        {
            if (!instance.Target.TryGetComponent(out IDamageable damageable)) return;

            damageable.GetDamage(new DamageContext
            {
                Attacker = instance.Caster,
                Damage = _damagePerTick * instance.StackCount,
                Type = EDamageType.Dot,
            });
        }
    }
}
