using UnityEngine;

public class StairSwitch : MonoBehaviour
{
    [SerializeField] private Collider2D upperStairsCollider;
    [SerializeField] private bool ignoreUpperStairs = true;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent<PlayerController>(out _))
        {
            Physics2D.IgnoreCollision(other, upperStairsCollider, ignoreUpperStairs);
        }
    }
}