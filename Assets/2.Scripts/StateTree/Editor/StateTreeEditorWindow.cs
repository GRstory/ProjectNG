using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace GRstory.StateTree.Editor
{
    public class StateTreeEditorWindow : EditorWindow
    {
        [SerializeField] private StateTreeAsset _asset; // 도메인 리로드 뒤에도 열어 둔 트리를 유지

        private StateTreeGraphView _graphView;
        private StateInspectorView _inspectorView;
        private Label _assetLabel;
        private Label _warningLabel;

        [MenuItem("Window/State Tree Editor")]
        public static void OpenWindow() => GetWindow<StateTreeEditorWindow>("State Tree");

        public static void Open(StateTreeAsset asset)
        {
            StateTreeEditorWindow window = GetWindow<StateTreeEditorWindow>("State Tree");
            window.Load(asset);
        }

        [OnOpenAsset]
        private static bool OnOpenAsset(int instanceId, int line)
        {
            if (EditorUtility.InstanceIDToObject(instanceId) is not StateTreeAsset asset) return false;

            Open(asset);
            return true;
        }

        #region EditorWindow
        private void CreateGUI()
        {
            Toolbar toolbar = new();
            _assetLabel = new Label { style = { unityTextAlign = TextAnchor.MiddleLeft, paddingLeft = 6 } };
            _warningLabel = new Label { style = { unityTextAlign = TextAnchor.MiddleLeft, paddingLeft = 12, color = new Color(1f, 0.6f, 0.2f) } };
            toolbar.Add(_assetLabel);
            toolbar.Add(_warningLabel);
            rootVisualElement.Add(toolbar);

            _graphView = new StateTreeGraphView();
            _inspectorView = new StateInspectorView();
            _graphView.OnStateSelected += guid => _inspectorView.SetTarget(_asset, guid);
            _graphView.OnGraphRebuilt += RefreshToolbar;
            _inspectorView.OnChanged += _graphView.RefreshNode;
            _inspectorView.OnStructureChanged += _graphView.Rebuild;

            TwoPaneSplitView split = new(1, 340, TwoPaneSplitViewOrientation.Horizontal) { style = { flexGrow = 1 } };
            split.Add(_graphView);
            split.Add(_inspectorView);
            rootVisualElement.Add(split);

            Load(_asset);
        }

        private void OnEnable()
        {
            Undo.undoRedoPerformed += HandleUndoRedo;
        }

        private void OnDisable()
        {
            Undo.undoRedoPerformed -= HandleUndoRedo;
        }

        // 프로젝트 창에서 다른 트리를 고르면 그 트리로 바꾼다
        private void OnSelectionChange()
        {
            if (Selection.activeObject is StateTreeAsset asset && asset != _asset) Load(asset);
        }
        #endregion

        private void Load(StateTreeAsset asset)
        {
            _asset = asset;
            if (_graphView == null) return; // CreateGUI에서 다시 불린다

            _graphView.Load(asset);
            _inspectorView.SetTarget(asset, null);
            RefreshToolbar();
        }

        private void HandleUndoRedo()
        {
            _graphView?.Rebuild();
            _inspectorView?.MarkDirtyRepaint();
        }

        private void RefreshToolbar()
        {
            _assetLabel.text = _asset != null ? _asset.name : "트리 에셋을 더블클릭하거나 선택하세요";
            _warningLabel.text = _asset != null && _asset.RootState == null
                ? "루트 상태가 없습니다. 노드 우클릭 → 루트로 설정"
                : string.Empty;
        }
    }
}
