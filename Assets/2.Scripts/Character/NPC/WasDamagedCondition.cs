using System;
using GRstory.Combat;
using GRstory.StateTree;

namespace GRstory.Character
{
    [Serializable]
    public class WasDamagedCondition : StateCondition
    {
        // 피격 플래그를 따로 두지 않는다. 적은 회복하지 않으므로 체력이 깎였다는 것이 곧 맞았다는 뜻
        public override bool IsAvailable(StateTreeContext context)
        {
            return context.OwnerObject.TryGetComponent(out Health health) && health.CurrentHealth < health.MaxHealth;
        }
    }
}
