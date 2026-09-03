using System;
using GRstory.SaveSystem;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }

    [SerializeField] private GameObject _playerPrefab;
    [SerializeField, Tooltip("세션에 스폰 정보가 없을 때(첫 진입, 테스트 씬) 쓰는 스폰포인트. 이 스폰포인트가 속한 방이 시작 방이 된다")]
    private SpawnPoint _defaultSpawnPoint;

    private RoomBehaviour[] _roomArray;
    private GameObject _player;
    private SpawnPoint _usedSpawnPoint;

    public RoomBehaviour CurrentRoom { get; private set; }

    public event Action<RoomBehaviour> OnRoomEntered;

    #region MonoBehaviour
    private void Awake()
    {
        Instance = this;
        _roomArray = FindObjectsByType<RoomBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        // 다른 오브젝트의 Start보다 먼저 플레이어가 존재해야 하므로 스폰은 Awake에서 끝낸다
        SpawnPlayer();
    }

    // 첫 방 진입은 Start에서 한다. Awake/OnEnable에서 OnRoomEntered를 구독한 쪽이 첫 진입을 받아야 하므로
    private void Start()
    {
        if (_player == null) return;

        RoomBehaviour startRoom = _usedSpawnPoint.GetComponentInParent<RoomBehaviour>(true);
        if (startRoom == null)
        {
            // 방이 없는 씬(테스트 등)은 스폰만 하고 끝. 방이 있는데 못 찾으면 배치 오류
            if (_roomArray.Length > 0)
                Debug.LogError($"스폰포인트 '{_usedSpawnPoint.name}'가 어느 방 계층에도 속해 있지 않음", _usedSpawnPoint);
            return;
        }

        foreach (RoomBehaviour room in _roomArray)
        {
            if (room != startRoom) room.Deactivate();
        }
        EnterRoomAt(_player, startRoom, _usedSpawnPoint.transform);
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }
    #endregion

    public void MoveThroughDoor(Door door, GameObject player)
    {
        if (door == null || player == null) return;

        RoomBehaviour targetRoom = door.TargetRoom;
        if (targetRoom == null)
        {
            Debug.LogError($"문 '{door.name}'의 도착 지점이 어느 방에도 속해 있지 않음", door);
            return;
        }

        // 같은 방 안의 지름길 문이면 이동만 한다
        if (targetRoom == CurrentRoom)
        {
            Teleport(player, door.ArrivalPoint);
            return;
        }

        EnterRoomAt(player, targetRoom, door.ArrivalPoint);
    }

    private void SpawnPlayer()
    {
        GameSession session = GameSession.Instance;
        SpawnPoint spawnPoint = FindSpawnPoint(session.NextSpawnPointId) ?? _defaultSpawnPoint;
        session.NextSpawnPointId = null;

        if (spawnPoint == null)
        {
            Debug.LogError("스폰포인트를 찾지 못했고 _defaultSpawnPoint도 비어 있음", this);
            return;
        }

        _usedSpawnPoint = spawnPoint;
        _player = Instantiate(_playerPrefab, spawnPoint.transform.position, spawnPoint.transform.rotation);
        session.RestorePlayer(_player);
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

    // 스폰과 문 이동이 공유하는 방 진입 절차: 도착 방을 먼저 완전한 상태로 만들고, 옮기고, 떠난 방을 끈다
    private void EnterRoomAt(GameObject player, RoomBehaviour room, Transform point)
    {
        RoomBehaviour previousRoom = CurrentRoom;

        room.Activate();
        Teleport(player, point);
        if (previousRoom != null && previousRoom != room) previousRoom.Deactivate();

        CurrentRoom = room;
        GameSession.Instance.GetSceneState(gameObject.scene.name).MarkRoomVisited(room.RoomId);
        OnRoomEntered?.Invoke(room);
    }

    private void Teleport(GameObject player, Transform point)
    {
        // CharacterController가 켜진 상태에선 transform 대입이 무시되므로 껐다 켠다
        if (player.TryGetComponent(out CharacterController controller))
        {
            controller.enabled = false;
            player.transform.SetPositionAndRotation(point.position, point.rotation);
            controller.enabled = true;
        }
        else
        {
            player.transform.SetPositionAndRotation(point.position, point.rotation);
        }
    }
}
