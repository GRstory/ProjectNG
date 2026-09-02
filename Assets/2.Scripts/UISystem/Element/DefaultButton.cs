using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace GRstory.UISystem
{
    [RequireComponent(typeof(Button))]
    public class DefaultButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private Image _hoverImage;

        private Button _button;

        // 부모 UI의 Awake가 먼저 돌아도 안전하도록 지연 조회
        public Button Button
        {
            get
            {
                if (_button == null) _button = GetComponent<Button>();
                return _button;
            }
        }

        #region MonoBehaviour
        private void Awake()
        {
            _hoverImage.enabled = false;
        }

        private void OnDisable()
        {
            _hoverImage.enabled = false;
        }
        #endregion

        public void OnPointerEnter(PointerEventData eventData)
        {
            _hoverImage.enabled = true;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _hoverImage.enabled = false;
        }
    }
}
