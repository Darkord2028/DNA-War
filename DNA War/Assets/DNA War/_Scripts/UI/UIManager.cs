using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [SerializeField] private MonsterSpawner _spawner;

    [Header("Clue Images")]
    [SerializeField] private Image[] clueImages;
    [SerializeField] private Animator fadeAnimator;
    [SerializeField] private Image radialImage;

    [Header("Timer")]
    [SerializeField] private float roundDuration = 30f;

    private float _timeRemaining;
    private bool _timerRunning;

    private void OnEnable()
    {
        GameEvents.OnCluesReady += HandleClues;
        GameEvents.OnRoundStart += HandleRoundStart;
        GameEvents.OnRoundEnd += HandleRoundEnd;
    }

    private void OnDisable()
    {
        GameEvents.OnCluesReady -= HandleClues;
        GameEvents.OnRoundStart += HandleRoundStart;
        GameEvents.OnRoundEnd += HandleRoundEnd;
    }

    private void Update()
    {
        if (!_timerRunning) return;

        _timeRemaining -= Time.deltaTime;
        _timeRemaining = Mathf.Clamp(_timeRemaining, 0f, roundDuration);

        radialImage.fillAmount = Mathf.Ceil(_timeRemaining) / roundDuration;

        if (_timeRemaining <= 0f)
        {
            _timerRunning = false;
            GameEvents.TimeUp();
        }
    }

    private void StartTimer()
    {
        _timeRemaining = roundDuration;
        _timerRunning = true;
    }

    private void StopTimer()
    {
        _timerRunning = false;
    }

    private void HandleClues(MonsterParts monsterParts)
    {
        var usedCategories = new System.Collections.Generic.HashSet<int>();

        foreach (Image clueImage in clueImages)
        {
            int attempts = 0;
            int roll;

            do
            {
                roll = Random.Range(0, 5);
                attempts++;
            }
            while (usedCategories.Contains(roll) && attempts < 20);

            usedCategories.Add(roll);

            Sprite sprite = roll switch
            {
                0 => _spawner.GetPartSprite(0, monsterParts.HandIndex),
                1 => _spawner.GetPartSprite(1, monsterParts.LegIndex),
                2 => _spawner.GetPartSprite(2, monsterParts.EyeIndex),
                3 => _spawner.GetPartSprite(3, monsterParts.MouthIndex),
                _ => _spawner.GetPartSprite(4, monsterParts.DetailIndex)
            };

            clueImage.sprite = sprite;
            clueImage.SetNativeSize();
        }

        StartTimer();
    }

    private void HandleRoundEnd()
    {
        StopTimer();
        fadeAnimator.SetBool("fade", true);
    }

    private void HandleRoundStart()
    {
        StopTimer();
        fadeAnimator.SetBool("fade", false);
    }

}
