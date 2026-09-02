using UnityEngine;

public class RoomBehaviour : MonoBehaviour
{
    [SerializeField, Tooltip("현재 방일 때만 켜지는 루트 (적, 소품, 조명). 문·도착 지점·스폰포인트는 이 밖에 둘 것")]
    private GameObject _contentRoot;

    // 방문 기록(SceneState)의 키. 씬 안에서 유일해야 하고, 이름을 바꾸면 기존 세이브의 방문 기록과 어긋난다
    public string RoomId => name;

    public bool IsActive { get; private set; } = true;

    public void Activate()
    {
        IsActive = true;
        if (_contentRoot != null) _contentRoot.SetActive(true);
    }

    public void Deactivate()
    {
        IsActive = false;
        if (_contentRoot != null) _contentRoot.SetActive(false);
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (_contentRoot == gameObject)
            Debug.LogError($"방 '{name}': Content 루트로 방 루트 자신을 지정하면 안 됨 (문과 도착 지점까지 꺼져버림)", this);
    }
#endif
}
