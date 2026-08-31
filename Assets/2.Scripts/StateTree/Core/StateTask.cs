using System;

namespace GRstory.StateTree
{
    [Serializable] // SerializeReference로 저장되려면 필수
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
