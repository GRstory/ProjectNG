using System;
using GRstory.SaveSystem;
using UnityEngine;

/// <summary>
/// "현재 방"의 단일 소유자. 방 전환 순서를 보장하고 OnRoomEntered를 발행한다.
/// 씬마다 하나 배치한다. 방은 자동 수집되므로 등록 절차 없음.
/// </summary>
public class RoomManager : MonoBehaviour
{
    public static RoomManager Instance { get; private set; }

    [SerializeField, Tooltip("스폰 정보가 없을 때(테스트 씬 등) 시작 방 수동 지정. 평소엔 비워둔다")]
    private RoomBehaviour _startRoomOverride;

    private RoomBehaviour[] _roomArray;

    public RoomBehaviour CurrentRoom { get; private set; }

    public event Action<RoomBehaviour> OnRoomEntered;

    #region MonoBehaviour
    private void Awake()
    {
        Instance = this;
        _roomArray = FindObjectsByType<RoomBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None);
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    // PlayerSpawner가 Awake에서 스폰을 끝내므로, Start 시점엔 플레이어와 사용된 스폰포인트가 존재한다
    private void Start()
    {
        RoomBehaviour startRoom = ResolveStartRoom();
        if (startRoom == null)
        {
            Debug.LogError("시작 방을 찾지 못함. 스폰포인트가 방 계층 아래 있는지, 또는 _startRoomOverride를 확인", this);
            return;
        }

        foreach (RoomBehaviour room in _roomArray)
        {
            if (room != startRoom) room.Deactivate();
        }
        EnterRoom(startRoom);
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

        RoomBehaviour previousRoom = CurrentRoom;

        targetRoom.Activate();     // 도착지를 먼저 완전한 상태로 만든 뒤
        Teleport(player, door.ArrivalPoint);
        if (previousRoom != null)
        {
            previousRoom.Deactivate(); // 플레이어가 떠난 뒤에 끈다
        }

        EnterRoom(targetRoom);
    }

    private void EnterRoom(RoomBehaviour room)
    {
        room.Activate();
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

    private RoomBehaviour ResolveStartRoom()
    {
        if (_startRoomOverride != null) return _startRoomOverride;

        PlayerSpawner spawner = FindFirstObjectByType<PlayerSpawner>();
        if (spawner != null && spawner.UsedSpawnPoint != null)
        {
            return spawner.UsedSpawnPoint.GetComponentInParent<RoomBehaviour>(true);
        }
        return null;
    }
}
