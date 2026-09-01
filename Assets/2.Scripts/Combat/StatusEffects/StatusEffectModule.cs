using UnityEngine;

namespace GRstory.Combat
{
    /// <summary>
    /// 상태이상 효과 조각의 베이스.
    /// 애셋 하나가 모든 캐릭터에 공유되므로 런타임 상태(생성한 VFX 등)는
    /// 이 클래스의 필드가 아니라 instance.SetData()에 저장해야 한다.
    /// </summary>
    public abstract class StatusEffectModule : ScriptableObject
    {
        public virtual void OnApply(StatusEffectInstance instance) { }
        public virtual void OnTick(StatusEffectInstance instance) { }
        public virtual void OnStackChanged(StatusEffectInstance instance) { }
        public virtual void OnRemove(StatusEffectInstance instance) { }
    }
}
