using System.Collections.Generic;

namespace GRstory.StateTree
{
    public class StateTransition
    {
        private List<StateCondition> _conditionList = new();
        private EStateTransitionState _trigger = EStateTransitionState.Succeeded;
        private State _targetState;

        public List<StateCondition> ConditionList => _conditionList;
        public EStateTransitionState Trigger => _trigger;
        public State TargetState => _targetState;
    }
}