using System;
using UnityEngine;
using UnityEngine.Events;

public class SpawnedStone : MonoBehaviour
{
    

    [SerializeField] private float destroyXPosition = 23f;
    [SerializeField] private float stoneHitPlayerTimePenality = 3;

    [SerializeField] private VoidEventChannelSO stoneDestroyedChannel;

    [SerializeField] private FloatEventChannelSO stoneHittedPlayer;

    private void Update()
    {
        if (transform.position.x >= destroyXPosition)
        {
            Destroy(gameObject);
        }
        
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Debug.Log("stone hit player");
            stoneHittedPlayer.RaiseEvent(stoneHitPlayerTimePenality);
            Destroy(gameObject);
        }
    }
    

    private void OnDestroy()
    {
        if (stoneDestroyedChannel != null)
        {
            stoneDestroyedChannel.RaiseEvent();
        }
    }
}