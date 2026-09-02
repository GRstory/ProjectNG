using UnityEngine;

public class SpawnPoint : MonoBehaviour
{
    [field: SerializeField] public string Id { get; private set; }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, 0.3f);
        Gizmos.DrawRay(transform.position, transform.forward);
    }
}
