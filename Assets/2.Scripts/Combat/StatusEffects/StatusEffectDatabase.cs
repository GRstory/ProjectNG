using System.Collections.Generic;
using UnityEngine;

namespace GRstory.Combat
{
    [CreateAssetMenu(fileName = "StatusEffectDatabase", menuName = "Combat/Status Effect Database")]
    public class StatusEffectDatabase : ScriptableObject
    {
        [SerializeField] private List<StatusEffectDefinition> _definitionList = new();

        private Dictionary<string, StatusEffectDefinition> _definitionDict;

        public StatusEffectDefinition GetById(string id)
        {
            EnsureIndexed();
            return _definitionDict.TryGetValue(id, out StatusEffectDefinition definition) ? definition : null;
        }

        private void EnsureIndexed()
        {
            if (_definitionDict != null) return;

            _definitionDict = new Dictionary<string, StatusEffectDefinition>();
            foreach (StatusEffectDefinition definition in _definitionList)
            {
                if (definition == null || string.IsNullOrEmpty(definition.Id))
                {
                    Debug.LogWarning($"StatusEffectDatabase: Id가 비었거나 null인 항목이 있어 제외됨 ({definition})", this);
                    continue;
                }
                if (!_definitionDict.TryAdd(definition.Id, definition))
                {
                    Debug.LogWarning($"StatusEffectDatabase: Id 중복 '{definition.Id}' ({definition.name})", this);
                }
            }
        }
    }
}
