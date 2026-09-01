using System;
using System.Collections.Generic;
using UnityEngine;

namespace GRstory.StateTree
{
    [Serializable]
    public class State
    {
        [SerializeField] private string _guid;      // 그래프 에디터용 식별자. 노드 생성 시 발급
        [SerializeField] private string _stateName;
        [SerializeField] private Vector2 _position; // 그래프 에디터용 노드 좌표. 런타임 미사용

        // 자기 참조(State)와 abstract 타입(Task/Condition)은 값 직렬화가 불가능하므로 전부 참조로 저장
        [SerializeReference] private List<State> _childStateList = new();
        [SerializeReference] private List<StateCondition> _enterConditionList = new();
        [SerializeReference] private List<StateTask> _taskList = new();
        [SerializeField] private List<StateTransition> _transitionList = new();

        [NonSerialized] private State _parentState; // 직렬화 대상이 아님. BuildParentLinks에서 연결

        public string GUID => _guid;
        public string StateName => _stateName;
        public Vector2 Position { get => _position; set => _position = value; }
        public State ParentState { get => _parentState; set => _parentState = value; }
        public List<State> ChildStateList => _childStateList;
        public List<StateCondition> EnterConditionList => _enterConditionList;
        public List<StateTask> TaskList => _taskList;
        public List<StateTransition> TransitionList => _transitionList;
    }
}
