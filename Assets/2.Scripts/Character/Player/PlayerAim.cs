using System.Collections.Generic;
using GRstory.Combat;
using UnityEngine;

namespace GRstory.Character
{
    public class PlayerAim : MonoBehaviour
    {
        [SerializeField] private LayerMask _targetMask;
        [SerializeField] private float _range = 10f;
        [SerializeField] private float _viewAngle = 150f;
        [SerializeField, Tooltip("락온 유지 한계 거리 배율. 이 거리를 벗어나면 타겟을 다시 찾는다")]
        private float _loseTargetRangeMultiplier = 1.15f;

        private readonly Collider[] _overlapBuffer = new Collider[32];
        private readonly List<Transform> _candidateList = new();

        public bool IsAiming { get; private set; }
        public Transform CurrentTarget { get; private set; }
        public bool HasTarget => CurrentTarget != null;

        public Vector3 AimDirection
        {
            get
            {
                if (CurrentTarget == null) return transform.forward;

                Vector3 direction = CurrentTarget.position - transform.position;
                direction.y = 0f;
                return direction.sqrMagnitude > 0.0001f ? direction.normalized : transform.forward;
            }
        }

        public void StartAim()
        {
            IsAiming = true;
            CurrentTarget = FindBestTarget();
        }

        public void StopAim()
        {
            IsAiming = false;
            CurrentTarget = null;
        }

        // 조준하는 동안 매 프레임 불린다. 타겟이 죽거나 너무 멀어지면 새로 잡는다
        public void UpdateAim()
        {
            if (!IsAiming) return;

            if (!IsValidTarget(CurrentTarget, _range * _loseTargetRangeMultiplier))
                CurrentTarget = FindBestTarget();
        }

        public void CycleTarget(int direction)
        {
            if (!IsAiming) return;

            GatherCandidates();
            if (_candidateList.Count == 0) return;

            // 타겟 전환이 왼쪽에서 오른쪽 순서가 되도록 각도순으로 정렬
            _candidateList.Sort((a, b) => SignedYawTo(a).CompareTo(SignedYawTo(b)));

            int index = _candidateList.IndexOf(CurrentTarget);
            index = index < 0 ? 0 : (index + direction + _candidateList.Count) % _candidateList.Count;
            CurrentTarget = _candidateList[index];
        }

        private Transform FindBestTarget()
        {
            GatherCandidates();

            Transform best = null;
            float bestAngle = float.MaxValue;
            foreach (Transform candidate in _candidateList)
            {
                float angle = Mathf.Abs(SignedYawTo(candidate));
                if (angle < bestAngle)
                {
                    bestAngle = angle;
                    best = candidate;
                }
            }
            return best;
        }

        private void GatherCandidates()
        {
            _candidateList.Clear();

            int count = Physics.OverlapSphereNonAlloc(transform.position, _range, _overlapBuffer, _targetMask);
            for (int i = 0; i < count; i++)
            {
                IDamageable damageable = _overlapBuffer[i].GetComponentInParent<IDamageable>();
                if (damageable is not Component component) continue;

                Transform candidate = component.transform;
                if (_candidateList.Contains(candidate)) continue; // 콜라이더를 여러 개 가진 대상은 한 번만 넣는다
                if (Mathf.Abs(SignedYawTo(candidate)) > _viewAngle * 0.5f) continue;
                if (!IsAlive(candidate)) continue;

                _candidateList.Add(candidate);
            }
        }

        private bool IsValidTarget(Transform target, float maxDistance)
        {
            if (target == null || !target.gameObject.activeInHierarchy) return false;
            if (!IsAlive(target)) return false;

            Vector3 offset = target.position - transform.position;
            offset.y = 0f;
            return offset.sqrMagnitude <= maxDistance * maxDistance;
        }

        private static bool IsAlive(Transform target)
        {
            return !target.TryGetComponent(out Health health) || health.CurrentHealth > 0f;
        }

        private float SignedYawTo(Transform target)
        {
            Vector3 direction = target.position - transform.position;
            direction.y = 0f;
            return Vector3.SignedAngle(transform.forward, direction, Vector3.up);
        }
    }
}
