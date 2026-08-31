using System.Collections.Generic;
using UnityEngine;

namespace GRstory.StateTree
{
    public class State
    {
        [SerializeField] private string _stateName;
        [SerializeField] private List<State> _childStateList = new();
        [SerializeField] private List<StateTask> _taskList = new();
        [SerializeField] private List<StateTransition> _trasitionList = new();

        public string StateName => _stateName;
        public List<State> StateList => _childStateList;
        public List<StateTask> StateTaskList => _taskList;
        public List<StateTransition> StateTransitionList => _trasitionList;
    }          
}
