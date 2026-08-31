using System;
using System.Collections.Generic;

namespace GRstory.StateTree
{
    public class StateTreeRunner
    {
        private StateTreeAsset _treeAsset;
        private StateTreeContext _context;

        private readonly List<State> _activePath = new();
        private readonly Dictionary<StateTask, EStateTaskState> _taskStateDict = new();
        private bool _isRunning = false;

        public bool IsRunning => _isRunning;
        public State CurrentLeaf => _activePath.Count > 0 ? _activePath[^1] : null;
        public event Action<State> OnStateChanged;
        #region Manage Function
        public void Init(StateTreeAsset stateTreeAsset, StateTreeContext context)
        {
            // 에셋 원본은 여러 에이전트가 공유하므로 복제본을 실행한다
            _treeAsset = stateTreeAsset.Clone();
            _treeAsset.BuildParentLinks();
            _context = context;
        }

        public void Start()
        {
            if (_treeAsset == null || _isRunning) return;

            _isRunning = true;
            TransitionTo(_treeAsset.RootState);
        }

        public void Update(float deltaTime)
        {
            if (!_isRunning) return;
            
            //1. 전이 검사
            if (TryTransitions(EStateTransitionState.Tick)) return;

            //2. 전체 Path 업데이트
            EStateTaskState result = UpdatePath(deltaTime);

            //3. 완료 트리거
            if (result == EStateTaskState.Succeeded)
                TryTransitions(EStateTransitionState.Succeeded);
            else if (result == EStateTaskState.Failed)
                TryTransitions(EStateTransitionState.Failed);
        }

        public void Stop()
        {
            if (!_isRunning) return;

            for (int i = _activePath.Count - 1; i >= 0; i--)
                ExitState(_activePath[i]);
            _activePath.Clear();
            _taskStateDict.Clear();
            _isRunning = false;
        }
        #endregion

        private EStateTaskState UpdatePath(float deltaTime)
        {
            //하나라도 실패시 Failed
            bool anyRunning = false;
            foreach (State state in _activePath)
            {
                foreach (StateTask task in state.TaskList)
                {
                    if (_taskStateDict[task] != EStateTaskState.Running) continue;

                    EStateTaskState result = task.Update(_context, deltaTime);
                    _taskStateDict[task] = result;

                    if (result == EStateTaskState.Failed) return EStateTaskState.Failed;
                    if (result == EStateTaskState.Running) anyRunning = true;
                }
            }
            return anyRunning ? EStateTaskState.Running : EStateTaskState.Succeeded;
        }

        private bool TryTransitions(EStateTransitionState trigger)
        {
            for (int i = _activePath.Count - 1; i >= 0; i--)
            {
                foreach (StateTransition transition in _activePath[i].TransitionList)
                {
                    if (transition.Trigger != trigger) continue;
                    if (!EvaluateConditions(transition)) continue;

                    TransitionTo(transition.TargetState);
                    return true;
                }
            }
            return false;
        }

        private bool EvaluateConditions(StateTransition transition)
        {
            foreach (StateCondition condition in transition.ConditionList)
            {
                if (condition == null || !condition.IsAvailable(_context)) return false;
            }
            return true;
        }

        private void TransitionTo(State targetState)
        {
            if (targetState == null) return;

            State leaf = SelectLeaf(targetState);
            List<State> newPath = BuildPath(leaf);

            // 공통 조상 찾기: 앞에서부터 일치하는 구간은 Exit/Enter 없이 유지
            int common = 0;
            while (common < _activePath.Count && common < newPath.Count
                   && _activePath[common] == newPath[common]) common++;

            for (int i = _activePath.Count - 1; i >= common; i--)
                ExitState(_activePath[i]);
            _activePath.RemoveRange(common, _activePath.Count - common);

            for (int i = common; i < newPath.Count; i++)
            {
                EnterState(newPath[i]);
                _activePath.Add(newPath[i]);
            }

            OnStateChanged?.Invoke(CurrentLeaf);
        }

        // 진입 조건을 통과하는 첫 자식으로 리프까지 하강
        private State SelectLeaf(State state)
        {
            while (true)
            {
                State next = null;
                foreach (State child in state.ChildStateList)
                {
                    if (CanEnter(child)) { next = child; break; }
                }
                if (next == null) return state;
                state = next;
            }
        }

        private List<State> BuildPath(State leaf)
        {
            List<State> path = new();
            for (State s = leaf; s != null; s = s.ParentState)
                path.Insert(0, s);
            return path;
        }

        private bool CanEnter(State state)
        {
            foreach (StateCondition condition in state.EnterConditionList)
            {
                if (condition == null || !condition.IsAvailable(_context)) return false;
            }
            return true;
        }

        private void EnterState(State state)
        {
            foreach (StateTask task in state.TaskList)
            {
                _taskStateDict[task] = EStateTaskState.Running;
                task.Enter(_context);
            }
        }

        private void ExitState(State state)
        {
            foreach (StateTask task in state.TaskList)
                task.Exit(_context);
        }
    }
}
