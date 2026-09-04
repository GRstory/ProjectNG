using UnityEditor;
using UnityEngine;

namespace GRstory.StateTree.Editor
{
    [CustomEditor(typeof(StateTreeAsset))]
    public class StateTreeAssetEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            StateTreeAsset asset = (StateTreeAsset)target;

            if (GUILayout.Button("State Tree 에디터 열기", GUILayout.Height(28)))
                StateTreeEditorWindow.Open(asset);

            EditorGUILayout.Space(4);
            EditorGUILayout.LabelField("상태 수", asset.NodeList.Count.ToString());
            EditorGUILayout.LabelField("루트", asset.RootState != null ? asset.RootState.StateName : "없음");
        }
    }
}
