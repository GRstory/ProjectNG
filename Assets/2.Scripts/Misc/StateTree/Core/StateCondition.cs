using UnityEngine;

namespace GRstory.StateTree
{
    public abstract class StateCondition
    {
        public abstract bool IsAbailable(StateTreeContext context);
    }
}