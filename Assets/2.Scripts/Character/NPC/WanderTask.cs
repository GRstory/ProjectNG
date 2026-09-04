using System;
using GRstory.StateTree;
using UnityEngine;
using UnityEngine.AI;

namespace GRstory.Character
{
    [Serializable]
    public class WanderTask : StateTask
    {
        [SerializeField, Min(0f), Tooltip("진입 시점 위치를 중심으로 이 반경 안에서 목적지를 고른다")]
        private float _radius = 5f;
        [SerializeField, Min(0f), Tooltip("목적지에 도착한 뒤 다음 목적지까지 기다리는 시간(초)")]
        private float _waitTime = 2f;

        private NavMeshAgent _agent;
        private Vector3 _center;
        private float _waitTimer;
        private bool _isWaiting;

        public override void Enter(StateTreeContext context)
        {
            _agent = context.OwnerObject.GetComponent<NavMeshAgent>();
            if (_agent == null)
                Debug.LogError($"'{context.OwnerObject.name}'에 NavMeshAgent가 없어 배회할 수 없음", context.OwnerObject);

            _center = context.OwnerObject.transform.position;
            _isWaiting = true;
            _waitTimer = _waitTime * UnityEngine.Random.value; // 여러 마리가 같은 박자로 움직이지 않도록
        }

        public override EStateTaskState Update(StateTreeContext context, float deltaTime)
        {
            if (_agent == null || !_agent.isOnNavMesh) return EStateTaskState.Running;

            if (_isWaiting)
            {
                _waitTimer -= deltaTime;
                if (_waitTimer <= 0f && TrySetRandomDestination()) _isWaiting = false;
                return EStateTaskState.Running;
            }

            if (!_agent.pathPending && _agent.remainingDistance <= _agent.stoppingDistance + 0.1f)
            {
                _isWaiting = true;
                _waitTimer = _waitTime;
            }
            return EStateTaskState.Running;
        }

        public override void Exit(StateTreeContext context)
        {
            if (_agent != null && _agent.isOnNavMesh) _agent.ResetPath();
        }

        private bool TrySetRandomDestination()
        {
            Vector2 offset = UnityEngine.Random.insideUnitCircle * _radius;
            Vector3 point = _center + new Vector3(offset.x, 0f, offset.y);
            if (!NavMesh.SamplePosition(point, out NavMeshHit hit, 2f, NavMesh.AllAreas)) return false;

            return _agent.SetDestination(hit.position);
        }
    }
}
