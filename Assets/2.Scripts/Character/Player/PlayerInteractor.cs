using GRstory.Interaction;
using UnityEngine;

namespace GRstory.Character
{
    public class PlayerInteractor : MonoBehaviour
    {
        [SerializeField] private LayerMask _interactMask;
        [SerializeField] private float _radius = 1.2f;
        [SerializeField, Tooltip("탐지 중심의 로컬 오프셋 (전방)")]
        private Vector3 _offset = new(0f, 0f, 0.8f);

        private readonly Collider[] _overlapBuffer = new Collider[8];

        public bool TryInteract()
        {
            IInteractable target = FindNearest();
            if (target == null) return false;

            target.Interact(gameObject);
            return true;
        }

        private IInteractable FindNearest()
        {
            Vector3 center = transform.TransformPoint(_offset);
            int count = Physics.OverlapSphereNonAlloc(center, _radius, _overlapBuffer, _interactMask);

            IInteractable nearest = null;
            float nearestSqrDistance = float.MaxValue;
            for (int i = 0; i < count; i++)
            {
                if (!_overlapBuffer[i].TryGetComponent(out IInteractable interactable)) continue;

                float sqrDistance = (_overlapBuffer[i].transform.position - center).sqrMagnitude;
                if (sqrDistance < nearestSqrDistance)
                {
                    nearestSqrDistance = sqrDistance;
                    nearest = interactable;
                }
            }
            return nearest;
        }
    }
}
