using GRstory.Character;
using GRstory.Combat;
using UnityEngine;
using UnityEngine.UI;

namespace GRstory.UISystem
{
    public class HUDUI : BaseUI
    {
        private static readonly int IntensityId = Shader.PropertyToID("_Intensity");

        [SerializeField] private Image _lowHealthVignette;
        [SerializeField, Range(0f, 1f)] private float _lowHealthThreshold = 0.3f;
        [SerializeField, Min(0f)] private float _vignetteFadeSpeed = 1.5f;

        private Health _health;
        private Material _vignetteMaterial;
        private float _intensity;
        private float _targetIntensity;

        #region MonoBehaviour
        protected override void Awake()
        {
            base.Awake();

            _vignetteMaterial = Instantiate(_lowHealthVignette.material);
            _lowHealthVignette.material = _vignetteMaterial;
        }

        private void OnEnable()
        {
            PlayerRegistry.OnPlayerChanged += Bind;
            Bind(PlayerRegistry.CurrentPlayerBehaviour);
        }

        private void Start()
        {
            Refresh();
        }

        private void Update()
        {
            if (_intensity == _targetIntensity) return;

            _intensity = Mathf.MoveTowards(_intensity, _targetIntensity, _vignetteFadeSpeed * Time.deltaTime);
            _vignetteMaterial.SetFloat(IntensityId, _intensity);
        }

        private void OnDisable()
        {
            PlayerRegistry.OnPlayerChanged -= Bind;
            Unbind();
        }

        private void OnDestroy()
        {
            if (_vignetteMaterial != null) Destroy(_vignetteMaterial);
        }
        #endregion

        private void Bind(PlayerBehaviour player)
        {
            Unbind();
            if (player == null || !player.TryGetComponent(out _health)) return;

            _health.OnHealthChanged += HandleHealthChanged;
            Refresh();
        }

        private void Unbind()
        {
            if (_health == null) return;

            _health.OnHealthChanged -= HandleHealthChanged;
            _health = null;
        }

        private void HandleHealthChanged(float current, float max)
        {
            _targetIntensity = ToIntensity(current, max);
        }

        private void Refresh()
        {
            if (_health == null) return;

            _targetIntensity = ToIntensity(_health.CurrentHealth, _health.MaxHealth);
            _intensity = _targetIntensity;
            _vignetteMaterial.SetFloat(IntensityId, _intensity);
        }

        private float ToIntensity(float current, float max)
        {
            float ratio = max > 0f ? current / max : 1f;
            return Mathf.InverseLerp(_lowHealthThreshold, 0f, ratio);
        }
    }
}
