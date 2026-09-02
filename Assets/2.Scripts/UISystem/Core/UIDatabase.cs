using System;
using System.Collections.Generic;
using UnityEngine;

namespace GRstory.UISystem
{
    [CreateAssetMenu(fileName = "UIDatabase", menuName = "UI/UI Database")]
    public class UIDatabase : ScriptableObject
    {
        [SerializeField] private List<BaseUI> _uiPrefabList = new();

        private Dictionary<Type, BaseUI> _uiPrefabDict;

        public BaseUI GetPrefab(Type type)
        {
            EnsureIndexed();
            return _uiPrefabDict.TryGetValue(type, out BaseUI prefab) ? prefab : null;
        }

        public T GetPrefab<T>() where T : BaseUI
        {
            return GetPrefab(typeof(T)) as T;
        }

        private void EnsureIndexed()
        {
            if (_uiPrefabDict != null) return;

            _uiPrefabDict = new Dictionary<Type, BaseUI>();
            foreach (BaseUI prefab in _uiPrefabList)
            {
                if (prefab == null)
                {
                    Debug.LogWarning("UIDatabase: null인 항목이 있어 제외됨", this);
                    continue;
                }
                if (!_uiPrefabDict.TryAdd(prefab.GetType(), prefab))
                {
                    Debug.LogWarning($"UIDatabase: 타입 중복 '{prefab.GetType().Name}' ({prefab.name})", this);
                }
            }
        }
    }
}
