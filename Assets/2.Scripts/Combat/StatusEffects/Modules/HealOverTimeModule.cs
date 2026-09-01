using UnityEngine;

namespace GRstory.Combat
{
    [CreateAssetMenu(fileName = "HealOverTimeModule", menuName = "Combat/Modules/Heal Over Time")]
    public class HealOverTimeModule : StatusEffectModule
    {
        [SerializeField] private float _healPerTick;

        public override void OnTick(StatusEffectInstance instance)
        {
            if (!instance.Target.TryGetComponent(out Health health)) return;

            health.Heal(_healPerTick * instance.StackCount);
        }
    }
}
