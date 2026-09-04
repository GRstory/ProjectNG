using System;
using GRstory.Combat;
using GRstory.StateTree;
using UnityEngine;
using UnityEngine.AI;

namespace GRstory.Character
{
    [RequireComponent(typeof(Health))]
    [RequireComponent(typeof(NavMeshAgent))]
    public class EnemyBehaviour : MonoBehaviour
    {
        [SerializeField] private StateTreeAsset _tree;

        private readonly StateTreeRunner _runner = new();
        private Health _health;
        private NavMeshAgent _agent;
        private bool _isDead;

        public string CurrentStateName => _runner.CurrentLeaf?.StateName;

        public event Action OnAttacked;

        // 공격 태스크가 휘두르는 순간 부른다. 태스크는 에셋 안의 순수 객체라 애니메이터를 직접 모르게 여기서 중계한다
        public void NotifyAttacked() => OnAttacked?.Invoke();

        #region MonoBehaviour
        private void Awake()
        {
            _health = GetComponent<Health>();
            _agent = GetComponent<NavMeshAgent>();

            if (_tree == null)
            {
                Debug.LogError($"'{name}'에 StateTree 에셋이 비어 있음", this);
                return;
            }
            _runner.Init(_tree, new StateTreeContext { OwnerObject = gameObject });
        }

        // 방이 꺼지면 같이 멈추고, 다시 켜지면 루트부터 새로 시작한다.
        // 사망 여부는 Health.IsDead가 아니라 자체 플래그로 본다. 씬 로드 때 이 OnEnable은 Health.Awake보다 먼저 불려서
        // CurrentHealth가 아직 0이라 IsDead가 참으로 나오기 때문
        private void OnEnable()
        {
            _health.OnDied += HandleDied;
            if (!_isDead) _runner.Start();
        }

        private void OnDisable()
        {
            _health.OnDied -= HandleDied;
            _runner.Stop();
        }

        private void Update()
        {
            // 정지 중엔 timeScale이 0이라 deltaTime도 0. 따로 막지 않는다
            _runner.Update(Time.deltaTime);
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (!Application.isPlaying || _runner.CurrentLeaf == null) return;
            UnityEditor.Handles.Label(transform.position + Vector3.up * 2f, _runner.CurrentLeaf.StateName);
        }
#endif
        #endregion

        // 사망은 트리 밖에서 처리한다. 트리마다 Dead 상태를 복붙할 필요가 없고, 시체는 더 판단할 게 없다
        private void HandleDied()
        {
            _isDead = true;
            _runner.Stop();
            _agent.enabled = false;

            // 시체가 총알과 플레이어 이동을 막지 않도록. 락온은 Health.IsDead로 이미 걸러진다
            foreach (Collider collider in GetComponentsInChildren<Collider>())
                collider.enabled = false;
        }
    }
}
