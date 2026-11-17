using System;
using UnityEngine;

[CreateAssetMenu(fileName = "LevelData", menuName = "Scriptable Objects/LevelData")]
public class LevelData : ScriptableObject
{
    [NonSerialized] public bool isGateOpen = false;
    [NonSerialized] public bool isCardinalSafe = false;
    [NonSerialized] public bool isPlayerDead = false;
    [NonSerialized] public bool isTimeUp = false;

    private void OnEnable()
    {
        ResetData();
    }

    public void ResetData()
    {
        isGateOpen = false;
        isCardinalSafe = false;
        isPlayerDead = false;
        isTimeUp = false;
    }
}
