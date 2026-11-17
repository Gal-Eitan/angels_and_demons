using Unity.VectorGraphics;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CardinalLifter : MonoBehaviour
{
    [SerializeField]
    private GameObject player;
    [SerializeField]
    private GameObject liftedCardinal;
    [SerializeField]
    private GameObject buriedCardinal;
    [SerializeField]
    private LevelData levelData;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            player.GetComponent<PlayerController>().onInteract.AddListener(LiftCardinal);
        }
        
    }
    void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            
            player.GetComponent<PlayerController>().onInteract.RemoveListener(LiftCardinal);
        }
    }

    public async void LiftCardinal()
    {
        Debug.Log(levelData.isGateOpen);
        if (levelData.isGateOpen){
            buriedCardinal.SetActive(false);
            liftedCardinal.SetActive(true);
            levelData.isCardinalSafe=true;
            await Awaitable.WaitForSecondsAsync(1f);
            SceneManager.LoadScene("GameResults");
        }
    }
}
