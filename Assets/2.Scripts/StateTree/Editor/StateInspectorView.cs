using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace GRstory.StateTree.Editor
{
    public class StateInspectorView : VisualElement
    {
        private StateTreeAsset _asset;
        private SerializedObject _serializedObject;
        private string _guid;
        private bool _structureDirty;
        private readonly IMGUIContainer _container;

        public event Action<string> OnChanged;   // 선택 노드의 표시만 갱신하면 되는 편집
        public event Action OnStructureChanged;  // 전이 추가·삭제처럼 그래프를 다시 그려야 하는 편집

        public StateInspectorView()
        {
            style.paddingLeft = 6;
            style.paddingRight = 6;
            style.paddingTop = 4;

            ScrollView scroll = new() { style = { flexGrow = 1 } };
            _container = new IMGUIContainer(DrawInspector);
            scroll.Add(_container);
            Add(scroll);
        }

        public void SetTarget(StateTreeAsset asset, string guid)
        {
            if (_asset != asset)
            {
                _asset = asset;
                _serializedObject = asset != null ? new SerializedObject(asset) : null;
            }
            _guid = guid;
            _container.MarkDirtyRepaint();
        }

        private void DrawInspector()
        {
            if (_asset == null || _serializedObject == null || _serializedObject.targetObject == null)
            {
                EditorGUILayout.HelpBox("트리 에셋을 여세요", MessageType.Info);
                return;
            }

            int index = string.IsNullOrEmpty(_guid) ? -1 : _asset.NodeList.FindIndex(state => state.GUID == _guid);
            if (index < 0)
            {
                EditorGUILayout.HelpBox("그래프에서 상태를 선택하세요", MessageType.Info);
                return;
            }

            _serializedObject.Update();
            SerializedProperty stateProp = _serializedObject.FindProperty("_nodeList").GetArrayElementAtIndex(index);

            EditorGUILayout.PropertyField(stateProp.FindPropertyRelative("_stateName"), new GUIContent("이름"));
            EditorGUILayout.Space(8);
            DrawReferenceList(stateProp.FindPropertyRelative("_enterConditionList"), "진입 조건", typeof(StateCondition));
            EditorGUILayout.Space(8);
            DrawReferenceList(stateProp.FindPropertyRelative("_taskList"), "태스크", typeof(StateTask));
            EditorGUILayout.Space(8);
            DrawTransitionList(stateProp.FindPropertyRelative("_transitionList"));

            bool applied = _serializedObject.ApplyModifiedProperties();

            // IMGUI 콜백 안에서 그래프 요소를 지우고 만들면 안 되므로 다음 틱으로 미룬다
            if (_structureDirty)
            {
                _structureDirty = false;
                schedule.Execute(() => OnStructureChanged?.Invoke());
            }
            else if (applied)
            {
                string guid = _guid;
                schedule.Execute(() => OnChanged?.Invoke(guid));
            }
        }

        private void DrawReferenceList(SerializedProperty listProp, string label, Type baseType)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(label, EditorStyles.boldLabel);
            if (GUILayout.Button("+", GUILayout.Width(24))) ShowTypeMenu(listProp, baseType);
            EditorGUILayout.EndHorizontal();

            for (int i = 0; i < listProp.arraySize; i++)
            {
                SerializedProperty element = listProp.GetArrayElementAtIndex(i);
                string typeName = element.managedReferenceValue?.GetType().Name ?? "(없음)";

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(element, new GUIContent(typeName), true);
                bool remove = GUILayout.Button("−", GUILayout.Width(24));
                EditorGUILayout.EndHorizontal();

                if (!remove) continue;

                element.managedReferenceValue = null;
                listProp.DeleteArrayElementAtIndex(i);
                break; // 배열이 바뀌었으니 이번 프레임 그리기는 여기까지
            }
        }

        private void DrawTransitionList(SerializedProperty listProp)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("전이", EditorStyles.boldLabel);
            if (GUILayout.Button("+", GUILayout.Width(24)))
            {
                int index = listProp.arraySize;
                listProp.arraySize = index + 1;

                // 새 원소는 마지막 원소의 복사본이다. 조건 인스턴스가 공유되면 한쪽 편집이 양쪽에 반영되므로 비운다
                SerializedProperty added = listProp.GetArrayElementAtIndex(index);
                added.FindPropertyRelative("_trigger").intValue = (int)EStateTransitionState.Succeeded;
                added.FindPropertyRelative("_conditionList").ClearArray();
                added.FindPropertyRelative("_targetState").managedReferenceValue = null;
                _structureDirty = true;
            }
            EditorGUILayout.EndHorizontal();

            for (int i = 0; i < listProp.arraySize; i++)
            {
                SerializedProperty element = listProp.GetArrayElementAtIndex(i);

                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(element.FindPropertyRelative("_trigger"), new GUIContent($"전이 {i + 1}"));
                bool remove = GUILayout.Button("−", GUILayout.Width(24));
                EditorGUILayout.EndHorizontal();

                if (remove)
                {
                    EditorGUILayout.EndVertical();
                    listProp.DeleteArrayElementAtIndex(i);
                    _structureDirty = true;
                    break;
                }

                State target = element.FindPropertyRelative("_targetState").managedReferenceValue as State;
                EditorGUILayout.LabelField("대상", target != null ? target.StateName : "없음 (그래프에서 포트를 연결)");
                DrawReferenceList(element.FindPropertyRelative("_conditionList"), "조건", typeof(StateCondition));
                EditorGUILayout.EndVertical();
            }
        }

        private void ShowTypeMenu(SerializedProperty listProp, Type baseType)
        {
            GenericMenu menu = new();
            bool any = false;

            foreach (Type type in TypeCache.GetTypesDerivedFrom(baseType))
            {
                if (type.IsAbstract || type.IsGenericType) continue;

                any = true;
                Type captured = type;
                menu.AddItem(new GUIContent(type.Name), false, () => AddReference(listProp, captured));
            }

            if (!any) menu.AddDisabledItem(new GUIContent($"{baseType.Name} 구현이 없습니다"));
            menu.ShowAsContext();
        }

        // 메뉴 선택은 OnGUI 밖에서 돌아오므로 직렬화 객체를 직접 갱신·적용한다
        private void AddReference(SerializedProperty listProp, Type type)
        {
            _serializedObject.Update();

            int index = listProp.arraySize;
            listProp.arraySize = index + 1;
            listProp.GetArrayElementAtIndex(index).managedReferenceValue = Activator.CreateInstance(type);

            _serializedObject.ApplyModifiedProperties();
            OnChanged?.Invoke(_guid);
            _container.MarkDirtyRepaint();
        }
    }
}
