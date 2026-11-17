using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class showResults : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI  Title;
    [SerializeField] private TextMeshProUGUI  Description;
    
    [SerializeField] private LevelData levelData;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (levelData.isCardinalSafe)
        {
            Title.text="you saved the cardinal!";
            Description.text="";
        }
        else
        {
            Title.text="you lost!\n";
            if (levelData.isPlayerDead)
            {
            Description.text="because you died";
            }else if (levelData.isTimeUp)
            {
            Description.text="because the cardinal suffocated";
            }
        }

    }

    public void playAgain()
    {
        levelData.ResetData();
        SceneManager.LoadScene("Level1Scene");
    }
    
}
