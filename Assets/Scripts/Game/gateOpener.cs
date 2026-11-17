using UnityEngine;

public class gateOpener : MonoBehaviour
{
    [SerializeField]
    private GameObject player;
    [SerializeField]
    private GameObject openedGate;
    [SerializeField]
    private GameObject closedGate;
    [SerializeField]
    private LevelData levelData;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            player.GetComponent<PlayerController>().onInteract.AddListener(openGate);
        }
        
    }
    void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            
            player.GetComponent<PlayerController>().onInteract.RemoveListener(openGate);
        }
    }

    public async void openGate()
    {
        gameObject.transform.Rotate(new Vector3(0,0,180));
        await Awaitable.WaitForSecondsAsync(0.5f);
        closedGate.SetActive(false);
        openedGate.SetActive(true);
        levelData.isGateOpen=true;
    }
}
