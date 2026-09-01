using System.Collections.Generic;
using UnityEngine;

namespace GRstory.StateTree
{
    [CreateAssetMenu(menuName = "StateTree/State Tree")]
    public class StateTreeAsset : ScriptableObject
    {
        [SerializeReference] private State _rootState;
        [SerializeReference] private List<State> _nodeList = new(); // 그래프 에디터용 flat 목록. 루트 포함 전체 노드

        public State RootState => _rootState;
        public List<State> NodeList => _nodeList;

        // 에이전트별 독립 인스턴스. SerializeReference 객체까지 깊은 복사됨
        public StateTreeAsset Clone() => Instantiate(this);

        // ParentState는 직렬화하지 않으므로 로드 후 한 번 연결
        public void BuildParentLinks()
        {
            foreach (State node in _nodeList)
            {
                foreach (State child in node.ChildStateList)
                    child.ParentState = node;
            }
        }
    }
}
