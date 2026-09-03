using GRstory.Combat;
using UnityEngine;

namespace GRstory.ItemSystem
{
    [CreateAssetMenu(fileName = "NewItem", menuName = "Item/Item Data")]
    public class ItemData : ScriptableObject
    {
        [field: SerializeField, Tooltip("세이브 키. 한 번 정하면 바꾸지 말 것")]
        public string Id { get; private set; }

        [field: SerializeField] public string DisplayName { get; private set; }
        [field: SerializeField, TextArea] public string Description { get; private set; }
        [field: SerializeField] public Sprite Icon { get; private set; }
        [field: SerializeField, Min(1)] public int MaxStack { get; protected set; } = 1;

        [field: SerializeField] public bool ConsumeOnUse { get; protected set; } = true;

        [field: SerializeField, Tooltip("비어 있으면 사용 불가 (열쇠, 문서류)")]
        public StatusEffectDefinition UseEffect { get; private set; }

        public virtual bool IsUsable => UseEffect != null;

        // 사용 효과는 상태이상 시스템에 그대로 얹는다. 즉시 효과도 짧은 Duration의 상태이상으로 표현한다
        public virtual void Use(GameObject user)
        {
            if (UseEffect == null) return;
            if (!user.TryGetComponent(out StatusEffectController controller))
            {
                Debug.LogError($"아이템 '{name}' 사용 실패: '{user.name}'에 StatusEffectController가 없음", this);
                return;
            }

            controller.Apply(UseEffect, user);
        }
    }
}
