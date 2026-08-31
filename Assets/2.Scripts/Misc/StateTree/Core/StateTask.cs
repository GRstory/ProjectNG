using UnityEngine;

namespace GRstory.StateTree
{
    public abstract class StateTask
    {
        public virtual void Enter(StateTreeContext context)
        {

        }

        public virtual EStateTaskState Update(StateTreeContext context, float deltaTime)
        {
            return EStateTaskState.Running;
        }

        public virtual void Exit(StateTreeContext context)
        {

        }
    }
}