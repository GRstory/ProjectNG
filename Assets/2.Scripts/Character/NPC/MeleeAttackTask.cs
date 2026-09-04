using System;
using GRstory.Combat;
using GRstory.StateTree;
using UnityEngine;

namespace GRstory.Character
{
    [Serializable]
    public class MeleeAttackTask : StateTask
    {
        [SerializeField, Min(0f)] private float _damage = 10f;
        [SerializeField, Min(0f), Tooltip("타격 판정 거리. 쿨다운이 돌아왔을 때 이 안에 있어야 맞는다")]
        private float _range = 2f;
        [SerializeField, Min(0f)] private float _cooldown = 1f;
        [SerializeField, Min(0f), Tooltip("타겟을 향해 도는 속도(도/초)")]
        private float _turnSpeed = 360f;

        private StatusSystem _statusSystem;
        private EnemyBehaviour _behaviour;
        private float _cooldownTimer;

        public override void Enter(StateTreeContext context)
        {
            _statusSystem = context.OwnerObject.GetComponent<StatusSystem>();
            _behaviour = context.OwnerObject.GetComponent<EnemyBehaviour>();
            _cooldownTimer = _cooldown; // 들어오자마자 때리지 않는다. 선딜 역할
        }

        public override EStateTaskState Update(StateTreeContext context, float deltaTime)
        {
            PlayerBehaviour player = PlayerRegistry.CurrentPlayerBehaviour;
            if (player == null) return EStateTaskState.Running;

            Transform owner = context.OwnerObject.transform;
            Vector3 toTarget = player.transform.position - owner.position;
            toTarget.y = 0f;
            if (toTarget.sqrMagnitude > 0.0001f)
                owner.rotation = Quaternion.RotateTowards(owner.rotation, Quaternion.LookRotation(toTarget), _turnSpeed * deltaTime);

            _cooldownTimer -= deltaTime;
            if (_cooldownTimer > 0f) return EStateTaskState.Running;
            _cooldownTimer = _cooldown;

            // 휘두르는 건 범위와 무관하다. 범위 밖이면 헛스윙이고 쿨다운은 그대로 소모된다
            if (_behaviour != null) _behaviour.NotifyAttacked();
            if (toTarget.sqrMagnitude > _range * _range) return EStateTaskState.Running;

            // 플레이어와 같은 공식: 기본 데미지 + Attack 스탯. 센 놈은 프리팹의 StatusSystem 값만 올리면 된다
            float damage = _damage;
            if (_statusSystem != null) damage += _statusSystem.GetValue(EStatusType.Attack);

            player.GetComponent<IDamageable>()?.GetDamage(new DamageContext
            {
                Attacker = context.OwnerObject,
                Damage = damage,
                Type = EDamageType.Normal,
            });
            return EStateTaskState.Running;
        }
    }
}
