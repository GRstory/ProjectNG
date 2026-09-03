using GRstory.Interaction;
using UnityEngine;

public class Door : MonoBehaviour, IInteractable
{
    [SerializeField, Tooltip("목적지 방 안의 도착 지점. 반드시 그 방의 계층 아래(Content 밖)에 둘 것")]
    private Transform _arrivalPoint;

    [SerializeField] private bool _isLocked;

    public Transform ArrivalPoint => _arrivalPoint;
    public bool IsLocked => _isLocked;

    // 도착 지점의 부모 계층에서 소속 방을 해석한다. 문 자신은 방을 몰라도 된다
    public RoomBehaviour TargetRoom =>
        _arrivalPoint != null ? _arrivalPoint.GetComponentInParent<RoomBehaviour>(true) : null;

    public void Interact(GameObject interactor)
    {
        if (_isLocked) return; // TODO: 잠김 피드백 (소리/UI)
        if (LevelManager.Instance == null)
        {
            Debug.LogError($"문 '{name}': 씬에 LevelManager가 없음", this);
            return;
        }

        LevelManager.Instance.MoveThroughDoor(this, interactor);
    }

    public void SetLocked(bool locked) => _isLocked = locked;

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (_arrivalPoint != null && _arrivalPoint.GetComponentInParent<RoomBehaviour>(true) == null)
            Debug.LogWarning($"문 '{name}': 도착 지점이 어느 방 계층에도 속해 있지 않음", this);
    }

    private void OnDrawGizmosSelected()
    {
        if (_arrivalPoint == null) return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, _arrivalPoint.position);
        Gizmos.DrawWireSphere(_arrivalPoint.position, 0.3f);
        Gizmos.DrawRay(_arrivalPoint.position, _arrivalPoint.forward);
    }
#endif
}
