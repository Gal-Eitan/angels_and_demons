using UnityEngine;

using UnityEngine.SceneManagement;

public class LevelTimer : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField] private LevelConfigSO levelConfig;

    [Header("Listening Channels")]
    [SerializeField] private VoidEventChannelSO levelStartedChannel;
    [SerializeField] private FloatEventChannelSO timePenaltyChannel;

    [Header("Broadcasting Channels")]
    [SerializeField] private FloatEventChannelSO timeUpdatedChannel;
    [SerializeField] private VoidEventChannelSO timeExpiredChannel;

    [SerializeField] private LevelData levelData;
    private float currentTime;
    private bool isRunning = false;

    private void OnEnable()
    {
        if (levelStartedChannel != null)
            levelStartedChannel.OnEventRaised += StartTimer;

        if (timePenaltyChannel != null)
            timePenaltyChannel.OnEventRaised += ApplyPenalty;
    }

    private void OnDisable()
    {
        if (levelStartedChannel != null)
            levelStartedChannel.OnEventRaised -= StartTimer;

        if (timePenaltyChannel != null)
            timePenaltyChannel.OnEventRaised -= ApplyPenalty;
    }

    private void Start()
    {
        currentTime = levelConfig.startingTime;
        timeUpdatedChannel?.RaiseEvent(currentTime);
    }

    private void Update()
    {
        if (!isRunning) return;

        currentTime -= Time.deltaTime;

        if (currentTime <= 0f)
        {
            currentTime = 0f;
            isRunning = false;
            timeUpdatedChannel?.RaiseEvent(currentTime);
            timeExpiredChannel?.RaiseEvent();
            timeUp();
            return;
        }

        timeUpdatedChannel?.RaiseEvent(currentTime);
    }

    private void StartTimer()
    {
        isRunning = true;
    }

    private void ApplyPenalty(float penaltySeconds)
    {
        currentTime = Mathf.Max(0f, currentTime - penaltySeconds);
        timeUpdatedChannel?.RaiseEvent(currentTime);

        if (currentTime <= 0f && isRunning)
        {
            isRunning = false;
            timeUp();
            timeExpiredChannel?.RaiseEvent();
        }
    }

    private async void timeUp()
    {
        Debug.Log("time up");
        levelData.isTimeUp=true;
        await Awaitable.WaitForSecondsAsync(1f);
        SceneManager.LoadScene("GameResults");
    }
    
}