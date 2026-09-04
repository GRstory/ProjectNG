using System;
using GRstory.StateTree;
using UnityEngine;
using UnityEngine.AI;

namespace GRstory.Character
{
    [Serializable]
    public class ChaseTargetTask : StateTask
    {
        [SerializeField, Min(0f), Tooltip("목적지를 다시 찍는 간격(초). 매 프레임 경로를 다시 계산하지 않도록")]
        private float _repathInterval = 0.2f;

        private NavMeshAgent _agent;
        private float _repathTimer;

        public override void Enter(StateTreeContext context)
        {
            _agent = context.OwnerObject.GetComponent<NavMeshAgent>();
            if (_agent == null)
                Debug.LogError($"'{context.OwnerObject.name}'에 NavMeshAgent가 없어 추적할 수 없음", context.OwnerObject);

            _repathTimer = 0f;
        }

        public override EStateTaskState Update(StateTreeContext context, float deltaTime)
        {
            PlayerBehaviour player = PlayerRegistry.CurrentPlayerBehaviour;
            if (player == null || _agent == null || !_agent.isOnNavMesh) return EStateTaskState.Running;

            _repathTimer -= deltaTime;
            if (_repathTimer > 0f) return EStateTaskState.Running;

            _repathTimer = _repathInterval;
            _agent.SetDestination(player.transform.position);
            return EStateTaskState.Running;
        }

        public override void Exit(StateTreeContext context)
        {
            if (_agent != null && _agent.isOnNavMesh) _agent.ResetPath();
        }
    }
}
