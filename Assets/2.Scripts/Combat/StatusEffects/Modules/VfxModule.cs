using UnityEngine;

namespace GRstory.Combat
{
    [CreateAssetMenu(fileName = "VfxModule", menuName = "Combat/Modules/Vfx")]
    public class VfxModule : StatusEffectModule
    {
        [SerializeField] private GameObject _vfxPrefab;

        public override void OnApply(StatusEffectInstance instance)
        {
            if (_vfxPrefab == null) return;

            GameObject vfx = Instantiate(_vfxPrefab, instance.Target.transform);
            instance.SetData(this, vfx);
        }

        public override void OnRemove(StatusEffectInstance instance)
        {
            if (instance.TryGetData(this, out GameObject vfx) && vfx != null)
            {
                Destroy(vfx);
            }
        }
    }
}
