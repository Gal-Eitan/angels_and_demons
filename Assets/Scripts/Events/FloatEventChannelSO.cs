using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(menuName = "Events/FloatEventChannel")]
public class FloatEventChannelSO : ScriptableObject
{
    [TextArea] [SerializeField] private string description;
    public UnityAction<float> OnEventRaised;

    public void RaiseEvent(float value)
    {
        OnEventRaised?.Invoke(value);
    }
}