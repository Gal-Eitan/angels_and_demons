using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class StoneSpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    [SerializeField] private GameObject prefabToSpawn;
    [SerializeField] private float spawnInterval = 0.5f;
    [SerializeField] private int maxActiveObjects = 10;

    
    [SerializeField] private VoidEventChannelSO stoneDestroyedChannel;
    private bool spawnerEnabled=false;

    private int currentActiveCount = 0;

    public void enableSpawner()
    {
        spawnerEnabled=true;
    }
    public void disableSpawner()
    {
        spawnerEnabled=false;
    }

    private void Start()
    {
        StartCoroutine(SpawnRoutine());
        stoneDestroyedChannel.OnEventRaised+=OnObjectDestroyed;
    }

    public IEnumerator SpawnRoutine()
    {
       
        while (true)
        {
            yield return new WaitForSeconds(spawnInterval);

            if (spawnerEnabled && currentActiveCount < maxActiveObjects)
            {

                SpawnObject();
                currentActiveCount++;
                //Debug.Log($"stone spawned. now {currentActiveCount}");
            }
            
            
        }
    }

    public void SpawnObject()
    {
        if (prefabToSpawn == null) return;

        // יצירת האובייקט במיקום ובזווית של ה-Spawner
        GameObject spawnedObj = Instantiate(prefabToSpawn, transform.position, Quaternion.identity);
        
        

        // אופציונלי: האזנה להשמדת האובייקט כדי לעדכן את המונה
        // ניתן להשתמש ב-C# Events או בקומפוננטה ייעודית על ה-Prefab
    }

    // פונקציה שניתן לקרוא לה דרך Event כאשר אובייקט מושמד או נאסף
    public void OnObjectDestroyed()
    {
        currentActiveCount = Mathf.Max(0, currentActiveCount - 1);
    }
}