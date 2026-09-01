using UnityEngine;

namespace GRstory.Combat
{
    [CreateAssetMenu(fileName = "NewStatusEffect", menuName = "Combat/Status Effect")]
    public class StatusEffectDefinition : ScriptableObject
    {
        [field: SerializeField] public string Id { get; private set; }
        [field: SerializeField] public Sprite Icon { get; private set; }

        [field: SerializeField, Tooltip("0 이하면 무한 지속 (수동 해제형)")]
        public float Duration { get; private set; }

        [field: SerializeField, Tooltip("0 이하면 틱 없음")]
        public float TickInterval { get; private set; }

        [field: SerializeField] public EStackPolicy StackPolicy { get; private set; }
        [field: SerializeField] public int MaxStacks { get; private set; } = 1;
        [field: SerializeField] public StatusEffectModule[] Modules { get; private set; }
    }
}
