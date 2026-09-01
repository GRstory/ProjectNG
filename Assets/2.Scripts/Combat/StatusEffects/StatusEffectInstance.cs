using System.Collections.Generic;
using UnityEngine;

namespace GRstory.Combat
{
    public class StatusEffectInstance
    {
        public StatusEffectDefinition Definition { get; }
        public GameObject Target { get; }
        public GameObject Caster { get; }

        public int StackCount { get; internal set; } = 1;
        public float RemainingTime { get; internal set; }
        public float TickTimer { get; internal set; }

        private Dictionary<object, object> _moduleDataDict;

        public StatusEffectInstance(StatusEffectDefinition definition, GameObject target, GameObject caster)
        {
            Definition = definition;
            Target = target;
            Caster = caster;
            RemainingTime = definition.Duration;
        }

        // 공유 SO인 모듈이 인스턴스별 상태를 보관할 때 사용 (key는 보통 모듈 자신)
        public void SetData(object key, object data)
        {
            _moduleDataDict ??= new Dictionary<object, object>();
            _moduleDataDict[key] = data;
        }

        public bool TryGetData<T>(object key, out T data)
        {
            if (_moduleDataDict != null && _moduleDataDict.TryGetValue(key, out object value) && value is T typedValue)
            {
                data = typedValue;
                return true;
            }
            data = default;
            return false;
        }
    }
}
