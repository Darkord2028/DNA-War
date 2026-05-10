using UnityEngine;

public struct MonsterParts
{
    public int HandIndex;
    public int LegIndex;
    public int EyeIndex;
    public int MouthIndex;
    public int DetailIndex;
    public int DataIndex;
    public int ColorPaletteIndex;
}
public class Monster : MonoBehaviour
{
    public int monsterID;
    public MonsterParts monsterParts;

    public float GetSimilarity(Monster other)
    {
        int totalPart = 7;
        int matchingParts = 0;

        if (other.monsterParts.HandIndex == monsterParts.HandIndex) matchingParts++;
        if (other.monsterParts.LegIndex == monsterParts.LegIndex) matchingParts++;
        if (other.monsterParts.EyeIndex == monsterParts.EyeIndex) matchingParts++;
        if (other.monsterParts.MouthIndex == monsterParts.MouthIndex) matchingParts++;
        if (other.monsterParts.DetailIndex == monsterParts.DetailIndex) matchingParts++;
        if (other.monsterParts.DataIndex == monsterParts.DataIndex) matchingParts++;
        if (other.monsterParts.ColorPaletteIndex == monsterParts.ColorPaletteIndex) matchingParts++;

        return (float)matchingParts / totalPart;
    }
}
