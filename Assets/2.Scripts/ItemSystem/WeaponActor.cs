using GRstory.Combat;
using UnityEngine;

namespace GRstory.ItemSystem
{
    // WeaponItemData의 씬 쪽 짝. 장착 중에만 손 소켓 아래에 존재하며 총구 위치와 발사 실행을 맡는다
    public class WeaponActor : MonoBehaviour
    {
        [SerializeField, Tooltip("히트스캔 시작점. 총열 끝에 두고 +Z가 총구 방향")]
        private Transform _muzzle;

        private WeaponItemData _data;
        private float _cooldownTimer;

        public Transform Muzzle => _muzzle;

        private void Update()
        {
            if (_cooldownTimer > 0f) _cooldownTimer -= Time.deltaTime;
        }

        // 프리팹은 SO를 모른다. 장착 시 PlayerWeapon이 넘겨준다
        public void Initialize(WeaponItemData data)
        {
            _data = data;
            _cooldownTimer = 0f;
        }

        // 발사 여부를 돌려준다. 빗나가도 발사는 한 것이다
        public bool TryFire(GameObject attacker, Vector3 direction, float bonusDamage)
        {
            if (_cooldownTimer > 0f) return false;
            _cooldownTimer = _data.Cooldown;

            // 스탯 보너스는 펠릿 수로 나눠 무기와 상관없이 총량이 같게 한다
            int pellets = _data.PelletCount;
            float damagePerPellet = _data.Damage + bonusDamage / pellets;
            Vector3 origin = _muzzle.position;

            for (int i = 0; i < pellets; i++)
            {
                Vector3 pelletDirection = ApplySpread(direction, _data.SpreadAngle);
                if (!Physics.Raycast(origin, pelletDirection, out RaycastHit hit, _data.Range, _data.HitMask, QueryTriggerInteraction.Ignore))
                    continue;

                IDamageable damageable = hit.collider.GetComponentInParent<IDamageable>();
                damageable?.GetDamage(new DamageContext
                {
                    Attacker = attacker,
                    Damage = damagePerPellet,
                    Type = EDamageType.Normal,
                });
            }
            return true;
        }

        // 기준 방향을 원뿔 안에서 무작위로 튼다. angle이 0이면 그대로
        private static Vector3 ApplySpread(Vector3 direction, float angle)
        {
            if (angle <= 0f) return direction;

            Vector2 offset = Random.insideUnitCircle * angle;
            return Quaternion.LookRotation(direction) * Quaternion.Euler(offset.y, offset.x, 0f) * Vector3.forward;
        }
    }
}
