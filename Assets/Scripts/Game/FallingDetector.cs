using UnityEngine;
using UnityEngine.SceneManagement;


public class FallingDetector : MonoBehaviour
{
    [SerializeField]
    private LevelData levelData;
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            levelData.isPlayerDead=true;
            SceneManager.LoadScene("GameResults");
        }
        
    }
    


}
