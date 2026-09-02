using GRstory.SaveSystem;
using UnityEngine;

public class PlayerSpawner : MonoBehaviour
{
    [SerializeField] private GameObject _playerPrefab;
    [SerializeField] private SpawnPoint _defaultSpawnPoint;

    // RoomManager가 시작 방 판정에 읽는다
    public SpawnPoint UsedSpawnPoint { get; private set; }

    private void Awake()
    {
        GameSession session = GameSession.Instance;
        SpawnPoint spawnPoint = FindSpawnPoint(session.NextSpawnPointId) ?? _defaultSpawnPoint;
        session.NextSpawnPointId = null;
        UsedSpawnPoint = spawnPoint;

        if (spawnPoint == null)
        {
            Debug.LogError("스폰포인트를 찾지 못했고 _defaultSpawnPoint도 비어 있음", this);
            return;
        }

        GameObject player = Instantiate(_playerPrefab, spawnPoint.transform.position, spawnPoint.transform.rotation);
        session.RestorePlayer(player);
    }

    // 스폰포인트는 방 계층 아래 배치되므로 씬 전체에서 검색한다
    private SpawnPoint FindSpawnPoint(string id)
    {
        if (string.IsNullOrEmpty(id)) return null;

        foreach (SpawnPoint spawnPoint in FindObjectsByType<SpawnPoint>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (spawnPoint.Id == id) return spawnPoint;
        }

        Debug.LogWarning($"스폰포인트 '{id}'를 찾지 못해 기본 위치를 사용", this);
        return null;
    }
}
