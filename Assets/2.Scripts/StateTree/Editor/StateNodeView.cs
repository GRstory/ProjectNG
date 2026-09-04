using System;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace GRstory.StateTree.Editor
{
    public class StateNodeView : Node
    {
        private static readonly Color HierarchyColor = new(0.45f, 0.75f, 1f);
        private static readonly Color TransitionColor = new(1f, 0.65f, 0.25f);
        private static readonly Color RootTitleColor = new(0.2f, 0.45f, 0.3f);

        // 뷰는 State 인스턴스를 들고 있지 않는다. Undo나 인스펙터 적용으로 인스턴스가 바뀌어도 GUID로 다시 찾는다
        public string Guid { get; }
        public Port ParentPort { get; }
        public Port ChildrenPort { get; }
        public Port TransitionInPort { get; }
        public List<Port> TransitionPortList { get; } = new();

        public event Action<string, bool> OnSelectedChanged;
        public event Action<string> OnAddTransitionRequested;
        public event Action<string> OnSetRootRequested;

        public StateNodeView(State state, bool isRoot)
        {
            Guid = state.GUID;
            SetPosition(new Rect(state.Position, Vector2.zero));

            ParentPort = CreatePort(Direction.Input, Port.Capacity.Single, typeof(State), "부모", HierarchyColor);
            TransitionInPort = CreatePort(Direction.Input, Port.Capacity.Multi, typeof(StateTransition), "전이", TransitionColor);
            inputContainer.Add(ParentPort);
            inputContainer.Add(TransitionInPort);

            ChildrenPort = CreatePort(Direction.Output, Port.Capacity.Multi, typeof(State), "자식", HierarchyColor);
            outputContainer.Add(ChildrenPort);

            for (int i = 0; i < state.TransitionList.Count; i++)
            {
                Port port = CreatePort(Direction.Output, Port.Capacity.Single, typeof(StateTransition), GetTransitionLabel(state.TransitionList[i]), TransitionColor);
                port.userData = i; // 전이 목록의 인덱스. 전이가 추가·삭제되면 그래프 전체를 다시 만든다
                TransitionPortList.Add(port);
                outputContainer.Add(port);
            }

            Button addButton = new(() => OnAddTransitionRequested?.Invoke(Guid)) { text = "+ 전이" };
            titleButtonContainer.Insert(0, addButton);

            TryRefresh(state, isRoot);
            RefreshExpandedState();
            RefreshPorts();
        }

        // 전이 수가 포트 수와 다르면 구조가 바뀐 것이므로 false. 호출자가 전체를 다시 만든다
        public bool TryRefresh(State state, bool isRoot)
        {
            title = isRoot ? $"{state.StateName}  [ROOT]" : state.StateName;
            titleContainer.style.backgroundColor = isRoot ? new StyleColor(RootTitleColor) : new StyleColor(StyleKeyword.Null);

            if (TransitionPortList.Count != state.TransitionList.Count) return false;

            for (int i = 0; i < TransitionPortList.Count; i++)
                TransitionPortList[i].portName = GetTransitionLabel(state.TransitionList[i]);
            return true;
        }

        public override void OnSelected()
        {
            base.OnSelected();
            OnSelectedChanged?.Invoke(Guid, true);
        }

        public override void OnUnselected()
        {
            base.OnUnselected();
            OnSelectedChanged?.Invoke(Guid, false);
        }

        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            evt.menu.AppendAction("루트로 설정", _ => OnSetRootRequested?.Invoke(Guid));
            evt.menu.AppendSeparator();
            base.BuildContextualMenu(evt);
        }

        private Port CreatePort(Direction direction, Port.Capacity capacity, Type type, string name, Color color)
        {
            Port port = InstantiatePort(Orientation.Horizontal, direction, capacity, type);
            port.portName = name;
            port.portColor = color;
            return port;
        }

        private static string GetTransitionLabel(StateTransition transition)
        {
            int count = transition.ConditionList.Count;
            return count > 0 ? $"{transition.Trigger} ({count})" : transition.Trigger.ToString();
        }
    }
}
