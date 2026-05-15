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

    [Header("Idle Settings")]
    [SerializeField] float squishSpeed = 1.8f;
    [SerializeField] float squishAmount = 0.08f;
    [SerializeField] float handSwingSpeed = 1.2f;
    [SerializeField] float handSwingAngle = 18f;

    private Transform _bodyTransform;
    private Transform _leftHand;
    private Transform _rightHand;
    private Vector3 _baseBodyScale;
    private float _idleTime;
    private bool _idleRunning;

    public void StartIdle()
    {
        _idleTime = Random.Range(0f, Mathf.PI * 2f);

        foreach (Transform child in transform)
        {
            if (child.name == "Body") _bodyTransform = child;
            if (child.name == "LeftHand") _leftHand = child;
            if (child.name == "RightHand") _rightHand = child;
        }

        if (_bodyTransform != null)
            _baseBodyScale = _bodyTransform.localScale;

        _idleRunning = true;
    }

    public void StopIdle()
    {
        _idleRunning = false;

        if (_bodyTransform != null)
            _bodyTransform.localScale = _baseBodyScale;
        if (_leftHand != null)
            _leftHand.localRotation = Quaternion.identity;
        if (_rightHand != null)
            _rightHand.localRotation = Quaternion.identity;
    }

    private void Update()
    {
        if (!_idleRunning) return;

        _idleTime += Time.deltaTime;

        if (_bodyTransform != null)
        {
            float squish = Mathf.Sin(_idleTime * squishSpeed) * squishAmount;
            _bodyTransform.localScale = new Vector3(
                _baseBodyScale.x * (1f - squish),
                _baseBodyScale.y * (1f + squish),
                _baseBodyScale.z);
        }

        float handAngle = Mathf.Sin(_idleTime * handSwingSpeed) * handSwingAngle;
        if (_leftHand != null)
            _leftHand.localRotation = Quaternion.Euler(0f, 0f, handAngle);
        if (_rightHand != null)
            _rightHand.localRotation = Quaternion.Euler(0f, 0f, -handAngle);
    }

    public float GetSimilarity(Monster other)
    {
        int total = 7;
        int matches = 0;

        if (monsterParts.HandIndex == other.monsterParts.HandIndex) matches++;
        if (monsterParts.LegIndex == other.monsterParts.LegIndex) matches++;
        if (monsterParts.EyeIndex == other.monsterParts.EyeIndex) matches++;
        if (monsterParts.MouthIndex == other.monsterParts.MouthIndex) matches++;
        if (monsterParts.DetailIndex == other.monsterParts.DetailIndex) matches++;
        if (monsterParts.DataIndex == other.monsterParts.DataIndex) matches++;
        if (monsterParts.ColorPaletteIndex == other.monsterParts.ColorPaletteIndex) matches++;

        return (float)matches / total;
    }
}