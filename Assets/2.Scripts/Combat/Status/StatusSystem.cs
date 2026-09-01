using System;
using System.Collections.Generic;
using UnityEngine;

namespace GRstory.Combat
{
    public class StatusSystem : MonoBehaviour
    {
        [Serializable]
        private struct BaseStatEntry
        {
            public EStatusType Type;
            public float Value;
        }

        [SerializeField] private BaseStatEntry[] _baseStatusArray;

        private readonly Dictionary<EStatusType, StatusData> _statusDataDict = new();

        public event Action<EStatusType> OnStatChanged;

        #region MonoBehaviour
        private void Awake()
        {
            foreach (BaseStatEntry entry in _baseStatusArray)
            {
                _statusDataDict[entry.Type] = new StatusData(entry.Value);
            }
        }
        #endregion

        public float GetValue(EStatusType statusType)
        {
            if (_statusDataDict.TryGetValue(statusType, out StatusData statusData))
            {
                return statusData.Value;
            }
            return 0f;
        }

        public void AddModifier(StatusModifier modifier)
        {
            if (!_statusDataDict.TryGetValue(modifier.Stat, out StatusData statusData))
            {
                statusData = new StatusData(0f);
                _statusDataDict[modifier.Stat] = statusData;
            }

            statusData.AddModifier(modifier);
            OnStatChanged?.Invoke(modifier.Stat);
        }

        public void RemoveModifiersFromSource(object source)
        {
            foreach (KeyValuePair<EStatusType, StatusData> pair in _statusDataDict)
            {
                if (pair.Value.RemoveModifiersFromSource(source))
                {
                    OnStatChanged?.Invoke(pair.Key);
                }
            }
        }
    }
}
