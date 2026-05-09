using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

[System.Serializable]
public class ColorPalette
{
    public Color bodyColor;
    public Color limbColor;
}
public class MonsterSpawner : MonoBehaviour
{
    [Header("Monster Data")]
    [SerializeField] MonsterData[] monsterData;

    [Header("Color")]
    public ColorPalette[] colors;

    [Header("Part Sprites")]
    [SerializeField] Sprite[] handSprites;
    [SerializeField] Sprite[] legSprites;
    [SerializeField] Sprite[] eyeSprites;
    [SerializeField] Sprite[] mouthSprites;
    [SerializeField] Sprite[] detailSprites;

    [Header("Part Colors")]
    [SerializeField] Color[] bodyColor;
    [SerializeField] Color[] handColor;
    [SerializeField] Color[] legColor;
    [SerializeField] Color[] detailColor;

    [Header("Sorting Orders")]
    [SerializeField] string bodySortingOrder;
    [SerializeField] string handSortingOrder;
    [SerializeField] string legSortingOrder;
    [SerializeField] string eyeSortingOrder;
    [SerializeField] string mouthSortingOrder;
    [SerializeField] string detailSortingOrder;

    [Header("Pool Settings")]
    [SerializeField] int defaultPoolCapacity = 10;
    [SerializeField] int maxPoolSize = 100;

    private MonsterData _data;
    private Monster _currentMonster;
    private Color _bodyColor;
    private Color _limbColor;

    public Dictionary<int, Monster> activeMonsters;

    private ObjectPool<GameObject> _gameObjectPool;
    private int maxAttempts = 2;
    private HashSet<int> _usedCombinations = new HashSet<int>();

    private void Awake()
    {
        _gameObjectPool = new ObjectPool<GameObject>(CreateGameObjectPool, OnGetFromPool, OnReleaseToPool, OnDestroyPooled, true, defaultPoolCapacity, maxPoolSize);
    }

    private void Start()
    {
        for(int i = 0; i < 10; i++)
        {
            //BuildMonster();
        }
    }

    private void BuildMonster()
    {
        for (int i = 0; i < maxAttempts; i++)
        {
            MonsterParts parts = GenerateRandomParts();

            int key = GetUniqueKey(parts);
            if (_usedCombinations.Add(key))
            {
                MakeMonster(parts, Vector2.zero);
                return;
            }
        }

        Debug.LogWarning("Can not generate unique monster");
    }

    public void BuildMonster(Vector3 spawnPos)
    {
        for (int i = 0; i < maxAttempts; i++)
        {
            MonsterParts parts = GenerateRandomParts();

            int key = GetUniqueKey(parts);
            if (_usedCombinations.Add(key))
            {
                MakeMonster(parts, spawnPos);
                return;
            }
        }

        Debug.LogWarning("Can not generate unique monster");
    }

    private MonsterParts GenerateRandomParts()
    {
        return new MonsterParts
        {
            HandIndex = Random.Range(0, handSprites.Length),
            LegIndex = Random.Range(0, legSprites.Length),
            EyeIndex = Random.Range(0, eyeSprites.Length),
            MouthIndex = Random.Range(0, mouthSprites.Length),
            DetailIndex = Random.Range(0, detailSprites.Length)
        };
    }

    private int GetUniqueKey(MonsterParts parts)
    {
        return System.HashCode.Combine(
            parts.HandIndex,
            parts.LegIndex,
            parts.EyeIndex,
            parts.MouthIndex,
            parts.DetailIndex
        );
    }

    private void MakeMonster(MonsterParts parts, Vector2 spawnPos)
    {
        _data = monsterData[Random.Range(0, monsterData.Length)];

        // Root
        GameObject monster = _gameObjectPool.Get();
        monster.name = "monster";
        monster.transform.position = spawnPos;
        monster.transform.localScale = Vector3.one * 0.45f;
        _currentMonster = monster.AddComponent<Monster>();
        _currentMonster.monsterParts = parts;
        _currentMonster.monsterID = GetUniqueKey(parts);

        int randomColorIndex = Random.Range(0, colors.Length);
        _bodyColor = colors[randomColorIndex].bodyColor;
        _limbColor = colors[randomColorIndex].limbColor;

        MakeBody(monster);
        MakeHands(monster, parts.HandIndex);
        MakeLegs(monster, parts.LegIndex);
        MakeEyes(monster, parts.EyeIndex);
        MakeMouth(monster, parts.MouthIndex);
        MakeDetail(monster, parts.DetailIndex);
    }

    void MakeBody(GameObject monster)
    {
        GameObject body = _gameObjectPool.Get();
        body.name = "body";
        body.transform.SetParent(monster.transform, false);
        body.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
        SpriteRenderer sr = body.AddComponent<SpriteRenderer>();
        sr.sprite = _data.body;
        sr.color = _bodyColor;
        sr.sortingLayerName = bodySortingOrder;
    }

    void MakeHands(GameObject monster, int index)
    {
        if (handSprites.Length == 0) return;
        Sprite chosen = handSprites[index];
        float nudge = Random.Range(-_data.handPosRange, _data.handPosRange);
        Color color = handColor[Random.Range(0, handColor.Length)];

        // Left
        GameObject leftHand = _gameObjectPool.Get();
        leftHand.name = "leftHand";
        leftHand.transform.SetParent(monster.transform, false);
        SpriteRenderer leftSR = leftHand.AddComponent<SpriteRenderer>();
        leftSR.sprite = chosen;
        leftSR.color = _limbColor;
        leftSR.flipX = true;
        leftSR.sortingLayerName = handSortingOrder;
        leftHand.transform.SetLocalPositionAndRotation(
            new Vector3(-_data.handXPos + nudge, _data.handYPos + nudge, _data.handZPos),
            Quaternion.identity);

        // Right
        GameObject rightHand = _gameObjectPool.Get();
        rightHand.name = "rightHand";
        rightHand.transform.SetParent(monster.transform, false);
        SpriteRenderer rightSR = rightHand.AddComponent<SpriteRenderer>();
        rightSR.sprite = chosen;
        rightSR.color = _limbColor;
        rightSR.sortingLayerName = handSortingOrder;
        rightHand.transform.SetLocalPositionAndRotation(
            new Vector3(_data.handXPos + nudge, _data.handYPos + nudge, _data.handZPos),
            Quaternion.identity);
    }

    void MakeLegs(GameObject monster, int index)
    {
        if (legSprites.Length == 0) return;
        Sprite chosen = legSprites[index];
        float nudge = Random.Range(-_data.legPosRange, _data.legPosRange);
        Color color = legColor[Random.Range(0, legColor.Length)];

        // Left
        GameObject leftLeg = _gameObjectPool.Get();
        leftLeg.name = "leftLeg";
        leftLeg.transform.SetParent(monster.transform, false);
        SpriteRenderer leftSR = leftLeg.AddComponent<SpriteRenderer>();
        leftSR.sprite = chosen;
        leftSR.color = _limbColor;
        leftSR.flipX = true;
        leftSR.sortingLayerName = legSortingOrder;
        leftLeg.transform.SetLocalPositionAndRotation(
            new Vector3(-_data.legXPos + nudge, _data.legYPos + nudge, _data.legZPos),
            Quaternion.identity);

        // Right
        GameObject rightLeg = _gameObjectPool.Get();
        rightLeg.name = "rightLeg";
        rightLeg.transform.SetParent(monster.transform, false);
        SpriteRenderer rightSR = rightLeg.AddComponent<SpriteRenderer>();
        rightSR.sprite = chosen;
        rightSR.color = _limbColor;
        rightSR.sortingLayerName = legSortingOrder;
        rightLeg.transform.SetLocalPositionAndRotation(
            new Vector3(_data.legXPos + nudge, _data.legYPos + nudge, _data.legZPos),
            Quaternion.identity);
    }

    void MakeEyes(GameObject monster, int index)
    {
        if (eyeSprites.Length == 0) return;
        Sprite chosen = eyeSprites[index];
        float nudge = Random.Range(-_data.eyePosRange, _data.eyePosRange);

        // Left
        GameObject leftEye = _gameObjectPool.Get();
        leftEye.name = "leftEye";
        leftEye.transform.SetParent(monster.transform, false);
        SpriteRenderer leftSR = leftEye.AddComponent<SpriteRenderer>();
        leftSR.sprite = chosen;
        leftSR.flipX = true;
        leftSR.sortingLayerName = eyeSortingOrder;
        leftEye.transform.SetLocalPositionAndRotation(
            new Vector3(-_data.eyeXPos + nudge, _data.eyeYPos + nudge, _data.eyeZPos),
            Quaternion.identity);

        // Right
        GameObject rightEye = _gameObjectPool.Get();
        rightEye.name = "rightEye";
        rightEye.transform.SetParent(monster.transform, false);
        SpriteRenderer rightSR = rightEye.AddComponent<SpriteRenderer>();
        rightSR.sprite = chosen;
        rightSR.sortingLayerName = eyeSortingOrder;
        rightEye.transform.SetLocalPositionAndRotation(
            new Vector3(_data.eyeXPos + nudge, _data.eyeYPos + nudge, _data.eyeZPos),
            Quaternion.identity);
    }

    void MakeMouth(GameObject monster, int index)
    {
        if (mouthSprites.Length == 0) return;
        Sprite chosen = mouthSprites[index];
        float nudge = Random.Range(-_data.mouthPosRange, _data.mouthPosRange);

        GameObject mouth = _gameObjectPool.Get();
        mouth.name = "mouth";
        mouth.transform.SetParent(monster.transform, false);
        SpriteRenderer sr = mouth.AddComponent<SpriteRenderer>();
        sr.sprite = chosen;
        sr.sortingLayerName = mouthSortingOrder;
        mouth.transform.SetLocalPositionAndRotation(
            new Vector3(_data.mouthXPos + nudge, _data.mouthYPos + nudge, _data.mouthZPos),
            Quaternion.identity);
    }

    void MakeDetail(GameObject monster, int index)
    {
        if (detailSprites.Length == 0) return;
        Sprite chosen = detailSprites[index];
        float nudge = Random.Range(-_data.detailPosRange, _data.detailPosRange);
        Color color = detailColor[Random.Range(0, detailColor.Length)];

        GameObject rightDetail = _gameObjectPool.Get();
        rightDetail.name = "rightDetail";
        rightDetail.transform.SetParent(monster.transform, false);
        SpriteRenderer right_sr = rightDetail.AddComponent<SpriteRenderer>();
        right_sr.sprite = chosen;
        right_sr.color = _bodyColor;
        right_sr.sortingLayerName = detailSortingOrder;
        rightDetail.transform.SetLocalPositionAndRotation(
            new Vector3(_data.detailXPos + nudge, _data.detailYPos + nudge, _data.detailZPos),
            Quaternion.identity);

        GameObject leftDetail = _gameObjectPool.Get();
        leftDetail.name = "leftDetail";
        leftDetail.transform.SetParent(monster.transform, false);
        SpriteRenderer left_sr = leftDetail.AddComponent<SpriteRenderer>();
        left_sr.sprite = chosen;
        left_sr.color = _bodyColor;
        left_sr.flipX = true;
        left_sr.sortingLayerName = detailSortingOrder;
        leftDetail.transform.SetLocalPositionAndRotation(
            new Vector3(-_data.detailXPos + nudge, _data.detailYPos + nudge, _data.detailZPos),
            Quaternion.identity);
    }

    // Object Pool Functions
    private GameObject CreateGameObjectPool()
    {
        GameObject go = new GameObject("PooledMonster");
        return go;
    }

    private void OnGetFromPool(GameObject go)
    {
        foreach(GameObject child in go.transform)
        {
            _gameObjectPool.Release(child);
        }
        go.SetActive(true);
    }

    private void OnReleaseToPool(GameObject go)
    {
        if (TryGetComponent(out Monster monster))
        {
            Destroy(monster);
        }
        go.SetActive(false);
    }

    private void OnDestroyPooled(GameObject go)
    {
        Destroy(go);
    }
}