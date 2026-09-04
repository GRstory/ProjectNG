using GRstory.Combat;
using UnityEngine;

namespace GRstory.Character
{
    // "어디를 조준하는가"만 결정한다. 화면 좌표를 받아 커서 바닥 지점을 구하고, 그 근처에 적이 있으면 락온한다.
    // 입력 장치와 카메라 종류는 모른다. 게임패드를 붙일 때는 Behaviour가 다른 화면 좌표를 넘기면 된다
    public class PlayerAim : MonoBehaviour
    {
        [SerializeField] private LayerMask _targetMask;
        [SerializeField, Min(0f), Tooltip("커서 바닥 지점에서 이 거리 안의 적을 락온한다")]
        private float _lockOnRadius = 1.5f;
        [SerializeField, Min(1f), Tooltip("락온 해제 거리 배율. 잡은 타겟은 커서가 반경×배율을 벗어나야 풀린다 (경계 깜빡임 방지)")]
        private float _lockOnReleaseMultiplier = 1.3f;

        private readonly Collider[] _overlapBuffer = new Collider[16];
        private Camera _camera;
        private Collider _targetCollider;   // 몸통 중심 계산용. 락온 시 한 번만 찾는다
        private Vector3 _cursorPoint;       // 커서가 가리키는 바닥 지점. 레이가 실패한 프레임은 이전 값을 쓴다

        public bool IsAiming { get; private set; }
        public Transform CurrentTarget { get; private set; }
        public bool HasTarget => CurrentTarget != null;

        // 조준점. 락온 중엔 타겟 몸통 중심, 아니면 커서 바닥 지점
        public Vector3 AimPoint { get; private set; }

        // 플레이어에서 조준점으로 가는 수평 단위 벡터. 몸 회전과 타겟 없을 때의 사격 방향에 쓴다
        public Vector3 AimDirection { get; private set; } = Vector3.forward;

        #region MonoBehaviour
        private void Start()
        {
            _camera = Camera.main;
            if (_camera == null)
                Debug.LogError("MainCamera 태그가 붙은 카메라가 없어 마우스 조준이 동작하지 않습니다.", this);
        }

        private void OnDrawGizmosSelected()
        {
            if (!IsAiming) return;

            Gizmos.color = HasTarget ? Color.red : Color.yellow;
            Gizmos.DrawWireSphere(_cursorPoint, _lockOnRadius);
            Gizmos.DrawLine(transform.position + Vector3.up, AimPoint);
        }
        #endregion

        public void StartAim()
        {
            IsAiming = true;
            AimDirection = transform.forward;
            AimPoint = transform.position + transform.forward;
            _cursorPoint = AimPoint;
        }

        public void StopAim()
        {
            IsAiming = false;
            SetTarget(null);
        }

        // 조준하는 동안 매 프레임 불린다. 이동보다 먼저 불려야 그 프레임의 조준 방향으로 몸이 돈다
        public void UpdateAim(Vector2 screenPosition)
        {
            if (!IsAiming || _camera == null) return;

            if (TryGetCursorPoint(screenPosition, out Vector3 point))
                _cursorPoint = point;

            // 유지 판정을 먼저 본다. 잡은 타겟은 해제 반경까지 놓지 않아야 경계에서 떨리지 않는다
            if (!IsLockHeld(CurrentTarget))
                SetTarget(FindTargetNear(_cursorPoint));

            AimPoint = CurrentTarget != null ? GetBodyCenter() : _cursorPoint;

            // 커서가 플레이어 바로 위면 방향이 정의되지 않으므로 이전 방향을 유지한다
            Vector3 toAim = AimPoint - transform.position;
            toAim.y = 0f;
            if (toAim.sqrMagnitude > 0.0001f) AimDirection = toAim.normalized;
        }

        // 타겟이 있으면 원점에서 몸통 중심으로. 없으면 수평 사격.
        // 커서 바닥 지점을 향해 쏘면 총구 높이에서 바닥으로 꺾여 근거리에서 땅을 맞히기 때문
        public Vector3 GetFireDirection(Vector3 origin)
        {
            if (CurrentTarget != null)
            {
                Vector3 toTarget = AimPoint - origin;
                if (toTarget.sqrMagnitude > 0.0001f) return toTarget.normalized;
            }
            return AimDirection;
        }

        // 화면 좌표 → 플레이어 발 높이 평면 위의 점. 직교·원근 카메라 모두 동일하게 동작한다
        private bool TryGetCursorPoint(Vector2 screenPosition, out Vector3 point)
        {
            Ray ray = _camera.ScreenPointToRay(screenPosition);
            Plane ground = new Plane(Vector3.up, transform.position);
            if (!ground.Raycast(ray, out float distance))
            {
                point = default;
                return false;
            }

            point = ray.GetPoint(distance);
            return true;
        }

        private bool IsLockHeld(Transform target)
        {
            if (target == null || !target.gameObject.activeInHierarchy) return false;
            if (!IsAlive(target)) return false;

            float release = _lockOnRadius * _lockOnReleaseMultiplier;
            return HorizontalSqrDistance(target.position, _cursorPoint) <= release * release;
        }

        // 커서 반경 안에서 살아 있는 적 중 커서에 가장 가까운 것. 거리는 루트 위치 기준이라 유지 판정과 같은 잣대다
        private Transform FindTargetNear(Vector3 point)
        {
            int count = Physics.OverlapSphereNonAlloc(point, _lockOnRadius, _overlapBuffer, _targetMask, QueryTriggerInteraction.Ignore);

            Transform best = null;
            float bestSqrDistance = _lockOnRadius * _lockOnRadius;
            for (int i = 0; i < count; i++)
            {
                IDamageable damageable = _overlapBuffer[i].GetComponentInParent<IDamageable>();
                if (damageable is not Component component) continue;

                Transform candidate = component.transform;
                if (!IsAlive(candidate)) continue;

                float sqrDistance = HorizontalSqrDistance(candidate.position, point);
                if (sqrDistance <= bestSqrDistance)
                {
                    bestSqrDistance = sqrDistance;
                    best = candidate;
                }
            }
            return best;
        }

        private void SetTarget(Transform target)
        {
            CurrentTarget = target;
            _targetCollider = target != null ? target.GetComponentInChildren<Collider>() : null;
        }

        private Vector3 GetBodyCenter()
        {
            return _targetCollider != null ? _targetCollider.bounds.center : CurrentTarget.position;
        }

        private static bool IsAlive(Transform target)
        {
            return !target.TryGetComponent(out Health health) || !health.IsDead;
        }

        private static float HorizontalSqrDistance(Vector3 a, Vector3 b)
        {
            a.y = 0f;
            b.y = 0f;
            return (a - b).sqrMagnitude;
        }
    }
}
