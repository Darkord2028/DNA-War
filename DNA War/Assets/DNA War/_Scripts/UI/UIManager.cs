using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [SerializeField] private MonsterSpawner _spawner;

    [Header("Clue Images")]
    [SerializeField] private Image[] clueImages;

    private void OnEnable()
    {
        GameEvents.OnCluesReady += HandleClues;
    }

    private void OnDisable()
    {
        GameEvents.OnCluesReady -= HandleClues;
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
    }

}
