using System;
using System.Collections.Generic;
using GRstory.Interaction;
using GRstory.SaveSystem;
using UnityEngine;

namespace GRstory.ItemSystem
{
    public class WorldItem : MonoBehaviour, IInteractable
    {
        private const string PickedFact = "picked";

        [SerializeField, Tooltip("획득 기록(SceneState)의 키. 에디터에서 자동 생성되며 복제하면 새 값을 받는다")]
        private string _id;
        [SerializeField] private ItemData _item;
        [SerializeField, Min(1)] private int _count = 1;

        public ItemData Item => _item;
        public int Count => _count;

        #region MonoBehaviour
        // 아이템은 전부 씬에 미리 배치되므로, 이미 주운 것은 방이 켜질 때 여기서 사라진다
        private void Awake()
        {
            if (string.IsNullOrEmpty(_id))
            {
                Debug.LogError($"월드 아이템 '{name}'의 Id가 비어 있어 획득 기록이 남지 않음", this);
                return;
            }

            if (GetSceneState().GetBool(_id, PickedFact)) Destroy(gameObject);
        }
        #endregion

        public void Interact(GameObject interactor)
        {
            if (!interactor.TryGetComponent(out Inventory inventory)) return;
            if (!inventory.TryAdd(_item, _count)) return; // TODO: 가득 참 피드백 (소리/UI)

            if (!string.IsNullOrEmpty(_id)) GetSceneState().SetBool(_id, PickedFact, true);
            Destroy(gameObject);
        }

        private SceneState GetSceneState() => GameSession.Instance.GetSceneState(gameObject.scene.name);

#if UNITY_EDITOR
        // Id → 소유자. 복제된 오브젝트는 원본보다 늦게 등록되므로, 이미 다른 소유자가 있으면 자신이 복제본이다
        private static readonly Dictionary<string, WorldItem> _editorIdRegistry = new();

        private void OnValidate()
        {
            // 프리팹 에셋에는 Id를 주지 않는다. 모든 인스턴스가 같은 Id를 물려받기 때문
            if (UnityEditor.PrefabUtility.IsPartOfPrefabAsset(this)) return;
            if (UnityEditor.SceneManagement.PrefabStageUtility.GetPrefabStage(gameObject) != null) return;

            if (string.IsNullOrEmpty(_id) || IsTakenByOther(_id))
            {
                _id = Guid.NewGuid().ToString("N");
                UnityEditor.EditorUtility.SetDirty(this);
            }
            _editorIdRegistry[_id] = this;
        }

        private bool IsTakenByOther(string id)
        {
            // 삭제된 소유자는 Unity의 null 비교로 걸러진다 (삭제 후 Undo로 되살아나면 자기 Id를 유지)
            return _editorIdRegistry.TryGetValue(id, out WorldItem owner) && owner != null && owner != this;
        }
#endif
    }
}
