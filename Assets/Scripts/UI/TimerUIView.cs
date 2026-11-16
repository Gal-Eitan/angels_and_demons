using UnityEngine;
using TMPro;

public class TimerUIView : MonoBehaviour
{
    [Header("Listening Channel")]
    [SerializeField] private FloatEventChannelSO timeUpdatedChannel;

    [Header("UI Component")]
    [SerializeField] private TextMeshProUGUI timerText;

    private void Awake()
    {
        if (timerText == null)
            timerText = GetComponent<TextMeshProUGUI>();
    }

    private void OnEnable()
    {
        if (timeUpdatedChannel != null)
            timeUpdatedChannel.OnEventRaised += UpdateDisplay;
    }

    private void OnDisable()
    {
        if (timeUpdatedChannel != null)
            timeUpdatedChannel.OnEventRaised -= UpdateDisplay;
    }

    private void UpdateDisplay(float timeInSeconds)
    {
        int minutes = Mathf.FloorToInt(timeInSeconds / 60f);
        int seconds = Mathf.FloorToInt(timeInSeconds % 60f);
        timerText.text = $"{minutes:00}:{seconds:00}";
    }
}