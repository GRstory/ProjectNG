using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace GRstory.StateTree.Editor
{
    public class StateTreeGraphView : GraphView
    {
        private StateTreeAsset _asset;
        private readonly Dictionary<string, StateNodeView> _nodeViewDict = new();
        private string _selectedGuid;

        public event Action<string> OnStateSelected;
        public event Action OnGraphRebuilt;

        public StateTreeGraphView()
        {
            style.flexGrow = 1;
            Insert(0, new GridBackground());
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());
            SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);
            graphViewChanged = HandleGraphViewChanged;
        }

        public void Load(StateTreeAsset asset)
        {
            _asset = asset;
            _selectedGuid = null;
            Rebuild();
            schedule.Execute(() => FrameAll());
        }

        public void Rebuild()
        {
            string selectedGuid = _selectedGuid; // 요소를 지우면서 선택 해제 콜백이 돌아 지워지므로 미리 붙잡는다

            graphViewChanged = null; // 뷰를 비우는 동안 데이터 삭제 콜백이 돌면 안 된다
            DeleteElements(graphElements.ToList());
            graphViewChanged = HandleGraphViewChanged;
            _nodeViewDict.Clear();

            if (_asset == null)
            {
                OnGraphRebuilt?.Invoke();
                return;
            }

            foreach (State state in _asset.NodeList)
            {
                StateNodeView nodeView = new(state, state == _asset.RootState);
                nodeView.OnSelectedChanged += HandleNodeSelected;
                nodeView.OnAddTransitionRequested += AddTransition;
                nodeView.OnSetRootRequested += SetRoot;
                _nodeViewDict[state.GUID] = nodeView;
                AddElement(nodeView);
            }

            foreach (State state in _asset.NodeList)
            {
                StateNodeView from = _nodeViewDict[state.GUID];

                foreach (State child in state.ChildStateList)
                {
                    if (child != null && _nodeViewDict.TryGetValue(child.GUID, out StateNodeView childView))
                        AddElement(from.ChildrenPort.ConnectTo(childView.ParentPort));
                }

                for (int i = 0; i < state.TransitionList.Count; i++)
                {
                    State target = state.TransitionList[i].TargetState;
                    if (target != null && _nodeViewDict.TryGetValue(target.GUID, out StateNodeView targetView))
                        AddElement(from.TransitionPortList[i].ConnectTo(targetView.TransitionInPort));
                }
            }

            if (selectedGuid != null && _nodeViewDict.TryGetValue(selectedGuid, out StateNodeView selected))
                AddToSelection(selected);
            else
                _selectedGuid = null;

            OnGraphRebuilt?.Invoke();
        }

        // 이름·트리거처럼 구조가 안 바뀌는 편집은 노드만 다시 그린다
        public void RefreshNode(string guid)
        {
            State state = FindState(guid);
            if (state == null || !_nodeViewDict.TryGetValue(guid, out StateNodeView nodeView)) return;

            if (!nodeView.TryRefresh(state, state == _asset.RootState)) Rebuild();
        }

        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            List<Port> result = new();
            ports.ForEach(port =>
            {
                if (port == startPort || port.node == startPort.node) return;
                if (port.direction == startPort.direction) return;
                if (port.portType != startPort.portType) return; // 계층 포트끼리, 전이 포트끼리만
                result.Add(port);
            });
            return result;
        }

        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            if (_asset == null) return;

            if (evt.target is GraphView)
            {
                Vector2 position = contentViewContainer.WorldToLocal(evt.mousePosition);
                evt.menu.AppendAction("새 상태", _ => CreateState(position));
                evt.menu.AppendSeparator();
            }
            base.BuildContextualMenu(evt);
        }

        private GraphViewChange HandleGraphViewChanged(GraphViewChange change)
        {
            if (_asset == null) return change;

            Undo.RecordObject(_asset, "State Tree 편집");
            bool nodeRemoved = false;

            if (change.elementsToRemove != null)
            {
                // 노드보다 엣지를 먼저 정리해야 양 끝 상태를 아직 찾을 수 있다
                foreach (GraphElement element in change.elementsToRemove)
                {
                    if (element is Edge edge) RemoveLink(edge);
                }
                foreach (GraphElement element in change.elementsToRemove)
                {
                    if (element is not StateNodeView nodeView) continue;
                    DeleteState(FindState(nodeView.Guid));
                    nodeRemoved = true;
                }
            }

            change.edgesToCreate?.RemoveAll(edge => !TryAddLink(edge));

            if (change.movedElements != null)
            {
                foreach (GraphElement element in change.movedElements)
                {
                    if (element is StateNodeView nodeView && FindState(nodeView.Guid) is State state)
                        state.Position = nodeView.GetPosition().position;
                }
            }

            EditorUtility.SetDirty(_asset);
            if (nodeRemoved) schedule.Execute(Rebuild); // GraphView가 요소 제거를 마친 뒤에 다시 그린다
            return change;
        }

        private bool TryAddLink(Edge edge)
        {
            if (edge.output?.node is not StateNodeView fromView || edge.input?.node is not StateNodeView toView) return false;

            State from = FindState(fromView.Guid);
            State to = FindState(toView.Guid);
            if (from == null || to == null) return false;

            if (edge.output.portType == typeof(State))
            {
                if (to == _asset.RootState)
                {
                    Debug.LogWarning("루트 상태는 부모를 가질 수 없습니다. 다른 노드를 루트로 바꾼 뒤 연결하세요", _asset);
                    return false;
                }
                if (IsInSubtree(to, from))
                {
                    Debug.LogWarning($"'{to.StateName}'의 자손을 부모로 붙이면 순환이 생깁니다", _asset);
                    return false;
                }

                // 부모는 하나뿐이다. 다른 부모의 목록에 남아 있으면 지운다
                foreach (State state in _asset.NodeList)
                {
                    if (state != from) state.ChildStateList.Remove(to);
                }
                if (!from.ChildStateList.Contains(to)) from.ChildStateList.Add(to);
                return true;
            }

            int index = (int)edge.output.userData;
            if (index >= from.TransitionList.Count) return false;

            from.TransitionList[index].TargetState = to;
            return true;
        }

        private void RemoveLink(Edge edge)
        {
            if (edge.output?.node is not StateNodeView fromView || edge.input?.node is not StateNodeView toView) return;

            State from = FindState(fromView.Guid);
            State to = FindState(toView.Guid);
            if (from == null || to == null) return;

            if (edge.output.portType == typeof(State))
            {
                from.ChildStateList.Remove(to);
                return;
            }

            // 엣지를 지우면 대상만 비운다. 전이 자체(트리거·조건)는 인스펙터에서 지운다
            int index = (int)edge.output.userData;
            if (index < from.TransitionList.Count && from.TransitionList[index].TargetState == to)
                from.TransitionList[index].TargetState = null;
        }

        private void CreateState(Vector2 position)
        {
            Undo.RecordObject(_asset, "상태 추가");

            State state = new("New State", position);
            _asset.NodeList.Add(state);
            if (_asset.RootState == null) _asset.RootState = state;

            EditorUtility.SetDirty(_asset);
            _selectedGuid = state.GUID;
            Rebuild();
        }

        private void DeleteState(State state)
        {
            if (state == null) return;

            foreach (State other in _asset.NodeList)
            {
                other.ChildStateList.Remove(state);
                foreach (StateTransition transition in other.TransitionList)
                {
                    if (transition.TargetState == state) transition.TargetState = null;
                }
            }

            _asset.NodeList.Remove(state);
            if (_asset.RootState == state) _asset.RootState = null;
            if (_selectedGuid == state.GUID) _selectedGuid = null;
        }

        private void AddTransition(string guid)
        {
            State state = FindState(guid);
            if (state == null) return;

            Undo.RecordObject(_asset, "전이 추가");
            state.TransitionList.Add(new StateTransition());
            EditorUtility.SetDirty(_asset);

            _selectedGuid = guid;
            Rebuild();
        }

        private void SetRoot(string guid)
        {
            State state = FindState(guid);
            if (state == null || state == _asset.RootState) return;

            Undo.RecordObject(_asset, "루트 설정");
            // 루트는 부모가 없어야 하므로 기존 부모 관계를 끊는다
            foreach (State other in _asset.NodeList) other.ChildStateList.Remove(state);
            _asset.RootState = state;
            EditorUtility.SetDirty(_asset);

            Rebuild();
        }

        private void HandleNodeSelected(string guid, bool selected)
        {
            if (selected)
            {
                _selectedGuid = guid;
                OnStateSelected?.Invoke(guid);
            }
            else if (_selectedGuid == guid)
            {
                _selectedGuid = null;
                OnStateSelected?.Invoke(null);
            }
        }

        private State FindState(string guid)
        {
            return _asset == null || string.IsNullOrEmpty(guid) ? null : _asset.NodeList.Find(state => state.GUID == guid);
        }

        private static bool IsInSubtree(State root, State target)
        {
            if (root == target) return true;

            foreach (State child in root.ChildStateList)
            {
                if (child != null && IsInSubtree(child, target)) return true;
            }
            return false;
        }
    }
}
