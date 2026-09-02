using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace GRstory.UISystem
{
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance;

        [SerializeField] private Transform _rootTransform;
        [SerializeField] private BaseUI _defaultUI;
        [SerializeField] private BaseUI _escapeUI;   // 화면 위에서 ESC로 여는 팝업 (예: 일시정지 메뉴)

        private UIDatabase _database;

        private Dictionary<Type, BaseUI> _cacheDict = new();
        private Stack<BaseUI> _stack = new();

        #region MonoBehaviour
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this as UIManager;
            }
            else
            {
                Destroy(gameObject);
                return;
            }

            _database = Resources.Load<UIDatabase>("Database/UIDatabase");
            if (_database == null)
            {
                Debug.LogError("UIManager: No UI Database", this);
            }
        }

        private void Start()
        {
            if (_defaultUI != null) ActiveUI(_defaultUI.GetType());
        }

        private void Update()
        {
            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                HandleEscape();
            }
        }
        #endregion

        public T GetUIInstance<T>() where T : BaseUI
        {
            if (_cacheDict.TryGetValue(typeof(T), out BaseUI cachedUI))
            {
                return cachedUI as T;
            }
            return null;
        }

        public void ActiveUI<T>() where T : BaseUI
        {
            ActiveUI(typeof(T));
        }

        public void ActiveUI(Type type)
        {
            if (!_cacheDict.TryGetValue(type, out BaseUI ui))
            {
                BaseUI prefab = _database.GetPrefab(type);
                if (prefab == null)
                {
                    Debug.LogError($"UIManager: UIDatabase에 등록되지 않은 타입 '{type.Name}'", this);
                    return;
                }
                ui = Instantiate(prefab, _rootTransform);
                _cacheDict[type] = ui;
            }

            if (_stack.Contains(ui))
            {
                Debug.LogError($"UIManager: '{type.Name}'은(는) 이미 스택에 있음", this);
                return;
            }

            if (_stack.Count > 0)
            {
                // 화면 전환이면 이전 UI 퇴장, 팝업이면 보이는 채로 입력만 차단
                if (ui.UIType == EUIType.Screen) _stack.Peek().OnUIDeactive();
                else _stack.Peek().OnUICovered();
            }

            // 퇴장 애니메이션 중인 이전 UI보다 위에 그려지도록
            ui.transform.SetAsLastSibling();
            ui.OnUIActive();
            _stack.Push(ui);
        }

        public void DeactiveUI<T>() where T : BaseUI
        {
            if (_stack.Count == 0 || _stack.Peek().GetType() != typeof(T))
            {
                Debug.LogError($"UIManager: '{typeof(T).Name}'은(는) 스택 top이 아니라 닫을 수 없음", this);
                return;
            }

            DeactiveTopUI();
        }

        public void DeactiveTopUI()
        {
            if (_stack.Count == 0) return;

            BaseUI poppedUI = _stack.Pop();
            poppedUI.OnUIDeactive();

            if (_stack.Count > 0)
            {
                // 닫힌 게 화면이면 아래 UI 재등장, 팝업이면 입력만 복원
                if (poppedUI.UIType == EUIType.Screen) _stack.Peek().OnUIActive();
                else _stack.Peek().OnUIRevealed();
            }
        }

        private void HandleEscape()
        {
            // 팝업이 열려 있으면 닫고, 아니면 지정된 ESC 팝업을 연다
            if (_stack.Count > 0 && _stack.Peek().UIType == EUIType.Popup)
            {
                DeactiveTopUI();
            }
            else if (_escapeUI != null)
            {
                ActiveUI(_escapeUI.GetType());
            }
        }
    }
}
