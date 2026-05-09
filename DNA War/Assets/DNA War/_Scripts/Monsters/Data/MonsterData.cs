using System;
using UnityEngine;

[CreateAssetMenu(fileName = "NewMonsterData", menuName = "Data/MonsterData")]
public class MonsterData : ScriptableObject
{
    [Header("Body")]
    public Sprite body;

    [Header("Hand Position")]
    [Range(-2.0f, 2.0f)] public float handXPos;
    [Range(-2.0f, 2.0f)] public float handYPos;
    [Range(-2.0f, 2.0f)] public float handZPos;
    [Range(0f, 0.3f)] public float handPosRange;

    [Header("Leg Position")]
    [Range(-2.0f, 2.0f)] public float legXPos;
    [Range(-2.0f, 2.0f)] public float legYPos;
    [Range(-2.0f, 2.0f)] public float legZPos;
    [Range(0f, 0.3f)] public float legPosRange;

    [Header("Eye Position")]
    [Range(-2.0f, 2.0f)] public float eyeXPos;
    [Range(-2.0f, 2.0f)] public float eyeYPos;
    [Range(-2.0f, 2.0f)] public float eyeZPos;
    [Range(0f, 0.3f)] public float eyePosRange;

    [Header("Mouth Position")]
    [Range(-2.0f, 2.0f)] public float mouthXPos;
    [Range(-2.0f, 2.0f)] public float mouthYPos;
    [Range(-2.0f, 2.0f)] public float mouthZPos;
    [Range(0f, 0.3f)] public float mouthPosRange;

    [Header("Detail Position")]
    [Range(-2.0f, 2.0f)] public float detailXPos;
    [Range(-2.0f, 2.0f)] public float detailYPos;
    [Range(-2.0f, 2.0f)] public float detailZPos;
    [Range(0f, 0.3f)] public float detailPosRange;
}