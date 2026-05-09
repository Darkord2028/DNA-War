using UnityEngine;

public struct MonsterParts
{
    public int HandIndex;
    public int LegIndex;
    public int EyeIndex;
    public int MouthIndex;
    public int DetailIndex;
}
public class Monster : MonoBehaviour
{
    public int monsterID;
    
    public MonsterParts monsterParts;
}
