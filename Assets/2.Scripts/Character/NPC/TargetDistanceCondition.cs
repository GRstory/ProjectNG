using System;
using GRstory.StateTree;
using UnityEngine;

namespace GRstory.Character
{
    [Serializable]
    public class TargetDistanceCondition : StateCondition
    {
        [SerializeField, Min(0f)] private float _distance = 8f;
        [SerializeField, Tooltip("켜면 거리 이하일 때 참, 끄면 거리 초과일 때 참")]
        private bool _isWithin = true;

        public override bool IsAvailable(StateTreeContext context)
        {
            PlayerBehaviour player = PlayerRegistry.CurrentPlayerBehaviour;
            if (player == null) return !_isWithin; // 플레이어가 없으면 멀리 있는 것으로 본다

            Vector3 offset = player.transform.position - context.OwnerObject.transform.position;
            offset.y = 0f;
            bool within = offset.sqrMagnitude <= _distance * _distance;
            return within == _isWithin;
        }
    }
}
