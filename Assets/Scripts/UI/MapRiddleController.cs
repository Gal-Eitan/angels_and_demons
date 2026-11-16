using UnityEngine;
using TMPro;

public class MapRiddleController : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField] private LevelConfigSO levelConfig;
    [SerializeField] private int correctLocationId = 1; // 0 = Pantheon, 1 = Santa Maria del Popolo, 2 = St. Peter's

    [Header("Scene Panels")]
    [SerializeField] private GameObject gamePanel; // הפאנל שמכיל את המשחק עצמו

    [Header("Broadcasting Channels")]
    [SerializeField] private FloatEventChannelSO timePenaltyChannel;
    [SerializeField] private VoidEventChannelSO levelStartedChannel;

    [Header("Optional UI Feedback")]
    [SerializeField] private TextMeshProUGUI feedbackText;

    private void Start()
    {
        if (feedbackText != null)
            feedbackText.text = "";
    }

    public void SelectLocation(int locationId)
    {
        if (locationId == correctLocationId)
        {
            if (gamePanel != null)
                gamePanel.SetActive(true); // מדליק את פאנל המשחק

            levelStartedChannel?.RaiseEvent(); // מזניק את הטיימר
            transform.parent.gameObject.SetActive(false);
        }
        else
        {
            timePenaltyChannel?.RaiseEvent(levelConfig.wrongMapPenalty);

            if (feedbackText != null)
                feedbackText.text = $"Wrong location! -{levelConfig.wrongMapPenalty}s";
        }
    }
}