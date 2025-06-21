using UnityEngine;

public class SpawnPoint : MonoBehaviour
{
    [Header("Spawn Point Settings")]
    [SerializeField] private bool isActive = true;

    public bool IsActive => isActive;

    private void OnDrawGizmos()
    {
        Gizmos.color = isActive ? Color.green : Color.red;
        Gizmos.DrawSphere(transform.position, 0.5f);
    }
}