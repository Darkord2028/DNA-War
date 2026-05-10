using System;
using UnityEngine;

public static class GameEvents
{
    public static event Action OnPlacementReady;
    public static event Action<Monster> OnTargetSelected;
    public static event Action<MonsterParts> OnCluesReady;
    public static event Action<float> OnCorrectSelection;
    public static event Action<float> OnWrongSelection;
    public static event Action OnRoundStart;

    public static void PlacementReady() => OnPlacementReady?.Invoke();
    public static void TargetSelected(Monster m) => OnTargetSelected?.Invoke(m);
    public static void CluesReady(MonsterParts p) => OnCluesReady?.Invoke(p);
    public static void CorrectSelection(float pct) => OnCorrectSelection?.Invoke(pct);
    public static void WrongSelection(float pct) => OnWrongSelection?.Invoke(pct);
    public static void RoundStart() => OnRoundStart?.Invoke();
}