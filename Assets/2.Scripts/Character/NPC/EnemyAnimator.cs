using GRstory.Combat;
using UnityEngine;
using UnityEngine.AI;

namespace GRstory.Character
{
    [RequireComponent(typeof(EnemyBehaviour))]
    [RequireComponent(typeof(Health))]
    [RequireComponent(typeof(NavMeshAgent))]
    public class EnemyAnimator : MonoBehaviour
    {
        private static readonly int SpeedHash = Animator.StringToHash("Speed");
        private static readonly int IsDeadHash = Animator.StringToHash("IsDead");
        private static readonly int AttackHash = Animator.StringToHash("Attack");

        [SerializeField, Tooltip("비워두면 자식에서 찾는다")]
        private Animator _animator;
        [SerializeField] private float _dampTime = 0.1f;

        private EnemyBehaviour _behaviour;
        private Health _health;
        private NavMeshAgent _agent;

        #region MonoBehaviour
        private void Awake()
        {
            _behaviour = GetComponent<EnemyBehaviour>();
            _health = GetComponent<Health>();
            _agent = GetComponent<NavMeshAgent>();

            if (_animator == null)
                _animator = GetComponentInChildren<Animator>();

            if (_animator == null)
            {
                Debug.LogWarning("Animator를 찾지 못해 EnemyAnimator를 비활성화합니다.", this);
                enabled = false;
            }
        }

        private void OnEnable()
        {
            _behaviour.OnAttacked += HandleAttacked;
            _health.OnDied += HandleDied;
        }

        private void OnDisable()
        {
            _behaviour.OnAttacked -= HandleAttacked;
            _health.OnDied -= HandleDied;
        }

        private void Update()
        {
            // 에이전트 최대 속도 대비 배율. 플레이어의 NormalizedSpeed와 같은 잣대라 걷기 클립이 1에 맞는다
            float normalizedSpeed = _agent.speed > 0f ? _agent.velocity.magnitude / _agent.speed : 0f;
            _animator.SetFloat(SpeedHash, normalizedSpeed, _dampTime, Time.deltaTime);
        }
        #endregion

        private void HandleAttacked()
        {
            _animator.SetTrigger(AttackHash);
        }

        private void HandleDied()
        {
            _animator.SetBool(IsDeadHash, true);
        }
    }
}
