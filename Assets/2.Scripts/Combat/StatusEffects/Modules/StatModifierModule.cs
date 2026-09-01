using System;
using UnityEngine;

namespace GRstory.Combat
{
    [CreateAssetMenu(fileName = "StatModifierModule", menuName = "Combat/Modules/Stat Modifier")]
    public class StatModifierModule : StatusEffectModule
    {
        [Serializable]
        private struct ModifierEntry
        {
            public EStatusType Stat;
            public EModifierType Op;
            public float Value;
        }

        [SerializeField] private ModifierEntry[] _modifiers;

        public override void OnApply(StatusEffectInstance instance)
        {
            ApplyOneStack(instance);
        }

        // 스택이 1 오를 때마다 같은 모디파이어를 한 벌 더 얹는다.
        // 해제 시 Source(instance) 기준으로 전부 회수되므로 벌 수를 따로 셀 필요 없음.
        public override void OnStackChanged(StatusEffectInstance instance)
        {
            ApplyOneStack(instance);
        }

        public override void OnRemove(StatusEffectInstance instance)
        {
            if (instance.Target.TryGetComponent(out StatusSystem statusSystem))
            {
                statusSystem.RemoveModifiersFromSource(instance);
            }
        }

        private void ApplyOneStack(StatusEffectInstance instance)
        {
            if (!instance.Target.TryGetComponent(out StatusSystem statusSystem)) return;

            foreach (ModifierEntry entry in _modifiers)
            {
                statusSystem.AddModifier(new StatusModifier(entry.Stat, entry.Op, entry.Value, instance));
            }
        }
    }
}
