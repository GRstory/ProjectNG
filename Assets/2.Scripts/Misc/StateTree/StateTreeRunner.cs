
using System.Collections.Generic;
using UnityEngine;

namespace GRstory.StateTree 
{
    public class StateTreeRunner
    {
        [SerializeField] private StateTreeContext context;
        [SerializeField] private List<State> StateList = new();

        public void Update(float deltaTime)
        {

        }
    }
}