using System;
using UnityEngine;
using UnityEngine.UIElements;

public class PressureSystem : MonoBehaviour
{
    [Header("Pressure Settings")]
    [SerializeField] private int pressurePerStep;
    [SerializeField] private int maxPressure;
    [SerializeField] private GameObject[] pressureImages;

    public float CurrentPressure;
    public float MaxPressure => maxPressure;
    public bool IsMaxed => CurrentPressure >= MaxPressure;

    public event Action OnMaxPressureReached;
    public event Action<float> OnPressureChanged;

    private void Start()
    {
        UpdatePressurePanel();
    }

    public void AddStep()
    {
        CurrentPressure = Mathf.Min(CurrentPressure + pressurePerStep, MaxPressure);
        OnPressureChanged?.Invoke(CurrentPressure);
        UpdatePressurePanel();

        if (IsMaxed)
        {
            OnMaxPressureReached?.Invoke();
        }
    }

    public void Reduce(float amount)
    {
        CurrentPressure = Mathf.Max(CurrentPressure - amount, 0f);
        OnPressureChanged?.Invoke(CurrentPressure);
        UpdatePressurePanel();
    }

    private void UpdatePressurePanel()
    {
        float ratio = CurrentPressure / MaxPressure;
        int activeCount = Mathf.RoundToInt(ratio * pressureImages.Length);

        for (int i = 0; i < pressureImages.Length; i++)
        {
            pressureImages[i].SetActive(i < activeCount);
        }
    }

}
