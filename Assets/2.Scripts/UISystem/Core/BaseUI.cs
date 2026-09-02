using System.Collections;
using UnityEngine;

namespace GRstory.UISystem
{
    [RequireComponent(typeof(CanvasGroup), typeof(Animator))]
    public class BaseUI : MonoBehaviour
    {
        private static readonly int ActiveStateHash = Animator.StringToHash("Active");
        private static readonly int DeactiveStateHash = Animator.StringToHash("Deactive");

        private CanvasGroup _canvasGroup;
        private Animator _animator;
        private Coroutine _deactiveRoutine;

        [field: SerializeField] public EUIType UIType = EUIType.Screen;

        #region MonoBehaviour
        protected virtual void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
            _animator = GetComponent<Animator>();
            _animator.updateMode = AnimatorUpdateMode.UnscaledTime;
        }

        #endregion

        #region BaseUI
        public virtual void OnUIActive()
        {
            if (_deactiveRoutine != null)
            {
                StopCoroutine(_deactiveRoutine);
                _deactiveRoutine = null;
            }

            gameObject.SetActive(true);
            _canvasGroup.blocksRaycasts = true;
            if (_animator.runtimeAnimatorController != null && _animator.HasState(0, ActiveStateHash))
            {
                _animator.CrossFade(ActiveStateHash, 0.1f);
            }
        }

        public virtual void OnUIDeactive()
        {
            _canvasGroup.blocksRaycasts = false;
            if (!gameObject.activeSelf) return;

            if (_deactiveRoutine != null) StopCoroutine(_deactiveRoutine);
            _deactiveRoutine = StartCoroutine(DeactiveRoutine());
        }

        public virtual void OnUICovered()
        {
            _canvasGroup.blocksRaycasts = false;
        }

        public virtual void OnUIRevealed()
        {
            _canvasGroup.blocksRaycasts = true;
        }
        #endregion

        private IEnumerator DeactiveRoutine()
        {
            if (_animator.runtimeAnimatorController != null && _animator.HasState(0, DeactiveStateHash))
            {
                _animator.CrossFade(DeactiveStateHash, 0.1f);
                yield return null;

                // Deactive 상태 재생이 끝날 때까지 대기
                while (true)
                {
                    AnimatorStateInfo stateInfo = _animator.GetCurrentAnimatorStateInfo(0);
                    if (!_animator.IsInTransition(0) && stateInfo.shortNameHash == DeactiveStateHash && stateInfo.normalizedTime >= 1f) break;
                    yield return null;
                }
            }

            _deactiveRoutine = null;
            gameObject.SetActive(false);
        }
    }

    public enum EUIType { Screen, Popup }
}
