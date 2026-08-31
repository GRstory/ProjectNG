using System;
using System.Collections.Generic;
using UnityEngine;

namespace GRstory.StateTree
{
    [Serializable]
    public class StateTransition
    {
        [SerializeField] private EStateTransitionState _trigger = EStateTransitionState.Succeeded;
        [SerializeReference] private List<StateCondition> _conditionList = new();
        [SerializeReference] private State _targetState; // 값 직렬화 시 복사본이 저장되므로 반드시 참조로

        public EStateTransitionState Trigger => _trigger;
        public List<StateCondition> ConditionList => _conditionList;
        public State TargetState => _targetState;
    }
}
