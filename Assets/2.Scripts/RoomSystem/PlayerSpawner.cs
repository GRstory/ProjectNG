using GRstory.SaveSystem;
using NUnit.Framework;
using UnityEngine;

public class PlayerSpawner : MonoBehaviour
{
    [SerializeField] private GameObject _playerPrefab;
    [SerializeField] private SpawnPoint _defaultSpawnPoint;

    private void Awake()
    {
        GameSession session = GameSession.Instance;
        SpawnPoint spawnPoint = FindSpawnPoint(session.NextSpawnPointId) ?? _defaultSpawnPoint;
        session.NextSpawnPointId = null;

        GameObject player = Instantiate(_playerPrefab, spawnPoint.transform.position, spawnPoint.transform.rotation);
        session.RestorePlayer(player);
    }

    private SpawnPoint FindSpawnPoint(string id)
    {
        if (string.IsNullOrEmpty(id)) return null;

        foreach (SpawnPoint spawnPoint in GetComponentsInChildren<SpawnPoint>())
        {
            if (spawnPoint.Id == id) return spawnPoint;
        }

        return null;
    }
}
