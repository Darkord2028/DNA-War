using System;
using UnityEngine;

public static class GameEvents
{
    public static event Action OnPlacementReady;
    public static event Action<Monster> OnTargetSelected;
    public static event Action<MonsterParts> OnCluesReady;

    // Selection Events
    public static event Action<float> OnCorrectSelection;
    public static event Action<float> OnWrongSelection;

    // Round Events
    public static event Action OnRoundStart;
    public static event Action OnRoundEnd;

    // Timer
    public static event Action OnTimeUp;

    public static void PlacementReady() => OnPlacementReady?.Invoke();
    public static void TargetSelected(Monster m) => OnTargetSelected?.Invoke(m);
    public static void CluesReady(MonsterParts p) => OnCluesReady?.Invoke(p);

    // Selection Functions
    public static void CorrectSelection(float pct) => OnCorrectSelection?.Invoke(pct);
    public static void WrongSelection(float pct) => OnWrongSelection?.Invoke(pct);

    // Round Functions
    public static void RoundStart() => OnRoundStart?.Invoke();
    public static void RoundEnd() => OnRoundEnd?.Invoke();

    // Timer Functions
    public static void TimeUp() => OnTimeUp?.Invoke();

}