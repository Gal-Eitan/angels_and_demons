using UnityEngine;

[CreateAssetMenu(menuName = "Configs/LevelConfig")]
public class LevelConfigSO : ScriptableObject
{
    public float startingTime = 90f;
    public float wrongMapPenalty = 10f;
    public float rockHitPenalty = 5f;
}