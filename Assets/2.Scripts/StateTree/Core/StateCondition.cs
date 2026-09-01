using System;

namespace GRstory.StateTree
{
    [Serializable] // SerializeReference로 저장되려면 필수
    public abstract class StateCondition
    {
        public abstract bool IsAvailable(StateTreeContext context);
    }
}
