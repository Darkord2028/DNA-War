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

    [Header("Sorting Orders")]
    [SerializeField] string bodySortingOrder;
    [SerializeField] string handSortingOrder;
    [SerializeField] string legSortingOrder;
    [SerializeField] string eyeSortingOrder;
    [SerializeField] string mouthSortingOrder;
    [SerializeField] string detailSortingOrder;

    [Header("Spawn Settings")]
    [SerializeField] Transform[] spawnPoints;
    [SerializeField] float spawnRadius = 1f;
    [SerializeField] int monstersPerPoint = 1;

    [Header("Physics")]
    [SerializeField] LayerMask monsterLayermask;
    [SerializeField] float bounciness = 0.6f;
    [SerializeField] float linearDrag = 0.5f;
    [SerializeField] float angularDrag = 1f;

    [Header("Pool Settings")]
    [SerializeField] int defaultPoolCapacity = 10;
    [SerializeField] int maxPoolSize = 100;

    private MonsterData _data;
    private Monster _currentMonster;
    private Color _bodyColor;
    private Color _limbColor;

    private Dictionary<int, Monster> activeMonsters = new();

    private ObjectPool<GameObject> _partPool;
    private ObjectPool<GameObject> _monsterRootPool;

    private int maxAttempts = 20;
    private HashSet<int> _usedCombinations = new();

    private void Awake()
    {
        _partPool = new ObjectPool<GameObject>(
            CreatePartPoolObject, OnGetFromPool, OnReleaseToPool, OnDestroyPooled,
            true, defaultPoolCapacity, maxPoolSize);

        _monsterRootPool = new ObjectPool<GameObject>(
            CreateMonsterRootObject, OnGetFromPool, OnReleaseToPool, OnDestroyPooled,
            true, defaultPoolCapacity, maxPoolSize);
    }

    private void Start()
    {
        Spawn();
    }

    public void Spawn()
    {
        foreach (Transform point in spawnPoints)
        {
            for (int i = 0; i < monstersPerPoint; i++)
            {
                Vector2 offset = Random.insideUnitCircle * spawnRadius;
                Vector3 spawnPos = point.position + new Vector3(offset.x, offset.y, 0f);
                BuildMonster(spawnPos);
            }
        }

        GameEvents.PlacementReady();
    }

    public void RebuildGrid()
    {
        ReturnAllToPool();
        activeMonsters.Clear();
        _usedCombinations.Clear();
        GameEvents.RoundStart();
        Spawn();
    }

    public Dictionary<int, Monster> GetActiveMonsters() => activeMonsters;

    public Sprite GetPartSprite(int category, int index)
    {
        return category switch
        {
            0 => handSprites[index],
            1 => legSprites[index],
            2 => eyeSprites[index],
            3 => mouthSprites[index],
            4 => detailSprites[index],
            _ => null
        };
    }

    private void ReturnAllToPool()
    {
        foreach (var kvp in activeMonsters)
        {
            Monster monster = kvp.Value;
            if (monster == null) continue;

            GameObject monsterRoot = monster.gameObject;

            var children = new List<Transform>();
            foreach (Transform child in monsterRoot.transform)
                children.Add(child);

            foreach (Transform child in children)
                _partPool.Release(child.gameObject);

            _monsterRootPool.Release(monsterRoot);
        }
    }

    private Monster BuildMonster(Vector3 spawnPos)
    {
        for (int i = 0; i < maxAttempts; i++)
        {
            MonsterParts parts = GenerateRandomParts();
            int key = GetUniqueKey(parts);
            if (_usedCombinations.Add(key))
                return MakeMonster(parts, key, spawnPos);
        }

        Debug.LogWarning("[MonsterSpawner] Could not generate a unique monster.");
        return null;
    }

    private MonsterParts GenerateRandomParts()
    {
        return new MonsterParts
        {
            HandIndex = Random.Range(0, handSprites.Length),
            LegIndex = Random.Range(0, legSprites.Length),
            EyeIndex = Random.Range(0, eyeSprites.Length),
            MouthIndex = Random.Range(0, mouthSprites.Length),
            DetailIndex = Random.Range(0, detailSprites.Length),
            DataIndex = Random.Range(0, monsterData.Length),
            ColorPaletteIndex = Random.Range(0, colors.Length)
        };
    }

    private int GetUniqueKey(MonsterParts parts)
    {
        return System.HashCode.Combine(
            parts.HandIndex, parts.LegIndex, parts.EyeIndex,
            parts.MouthIndex, parts.DetailIndex,
            parts.DataIndex, parts.ColorPaletteIndex);
    }

    private Monster MakeMonster(MonsterParts parts, int key, Vector2 spawnPos)
    {
        _data = monsterData[parts.DataIndex];
        _bodyColor = colors[parts.ColorPaletteIndex].bodyColor;
        _limbColor = colors[parts.ColorPaletteIndex].limbColor;

        GameObject monsterRoot = _monsterRootPool.Get();
        monsterRoot.name = $"Monster_{key}";
        monsterRoot.transform.SetParent(null);
        monsterRoot.transform.position = spawnPos;
        monsterRoot.transform.localScale = Vector3.one * 0.45f;
        monsterRoot.layer = monsterLayermask;

        // Physics
        Rigidbody2D rb = monsterRoot.GetComponent<Rigidbody2D>();
        rb.linearDamping = linearDrag;
        rb.angularDamping = angularDrag;

        // Bounce material
        CircleCollider2D col = monsterRoot.GetComponent<CircleCollider2D>();
        PhysicsMaterial2D bounceMat = new PhysicsMaterial2D("Bounce");
        bounceMat.bounciness = bounciness;
        bounceMat.friction = 0.2f;
        col.sharedMaterial = bounceMat;

        _currentMonster = monsterRoot.GetComponent<Monster>();
        _currentMonster.monsterParts = parts;
        _currentMonster.monsterID = key;

        activeMonsters[key] = _currentMonster;

        MakeBody(monsterRoot);
        MakeHands(monsterRoot, parts.HandIndex);
        MakeLegs(monsterRoot, parts.LegIndex);
        MakeEyes(monsterRoot, parts.EyeIndex);
        MakeMouth(monsterRoot, parts.MouthIndex);
        MakeDetail(monsterRoot, parts.DetailIndex);

        _currentMonster.StartIdle();

        return _currentMonster;
    }

    void MakeBody(GameObject monster)
    {
        GameObject body = _partPool.Get();
        body.name = "Body";
        body.transform.SetParent(monster.transform, false);
        body.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
        SpriteRenderer sr = body.GetComponent<SpriteRenderer>();
        sr.sprite = _data.body;
        sr.color = _bodyColor;
        sr.sortingLayerName = bodySortingOrder;
    }

    void MakeHands(GameObject monster, int index)
    {
        if (handSprites.Length == 0) return;
        Sprite chosen = handSprites[index];
        float nudge = Random.Range(-_data.handPosRange, _data.handPosRange);

        GameObject leftHand = _partPool.Get();
        leftHand.name = "LeftHand";
        leftHand.transform.SetParent(monster.transform, false);
        SpriteRenderer leftSR = leftHand.GetComponent<SpriteRenderer>();
        leftSR.sprite = chosen;
        leftSR.color = _limbColor;
        leftSR.flipX = true;
        leftSR.sortingLayerName = handSortingOrder;
        leftHand.transform.SetLocalPositionAndRotation(
            new Vector3(-_data.handXPos + nudge, _data.handYPos + nudge, _data.handZPos),
            Quaternion.identity);

        GameObject rightHand = _partPool.Get();
        rightHand.name = "RightHand";
        rightHand.transform.SetParent(monster.transform, false);
        SpriteRenderer rightSR = rightHand.GetComponent<SpriteRenderer>();
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

        GameObject leftLeg = _partPool.Get();
        leftLeg.name = "LeftLeg";
        leftLeg.transform.SetParent(monster.transform, false);
        SpriteRenderer leftSR = leftLeg.GetComponent<SpriteRenderer>();
        leftSR.sprite = chosen;
        leftSR.color = _limbColor;
        leftSR.flipX = true;
        leftSR.sortingLayerName = legSortingOrder;
        leftLeg.transform.SetLocalPositionAndRotation(
            new Vector3(-_data.legXPos + nudge, _data.legYPos + nudge, _data.legZPos),
            Quaternion.identity);

        GameObject rightLeg = _partPool.Get();
        rightLeg.name = "RightLeg";
        rightLeg.transform.SetParent(monster.transform, false);
        SpriteRenderer rightSR = rightLeg.GetComponent<SpriteRenderer>();
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

        GameObject leftEye = _partPool.Get();
        leftEye.name = "LeftEye";
        leftEye.transform.SetParent(monster.transform, false);
        SpriteRenderer leftSR = leftEye.GetComponent<SpriteRenderer>();
        leftSR.sprite = chosen;
        leftSR.flipX = true;
        leftSR.sortingLayerName = eyeSortingOrder;
        leftEye.transform.SetLocalPositionAndRotation(
            new Vector3(-_data.eyeXPos + nudge, _data.eyeYPos + nudge, _data.eyeZPos),
            Quaternion.identity);

        GameObject rightEye = _partPool.Get();
        rightEye.name = "RightEye";
        rightEye.transform.SetParent(monster.transform, false);
        SpriteRenderer rightSR = rightEye.GetComponent<SpriteRenderer>();
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

        GameObject mouth = _partPool.Get();
        mouth.name = "Mouth";
        mouth.transform.SetParent(monster.transform, false);
        SpriteRenderer sr = mouth.GetComponent<SpriteRenderer>();
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

        GameObject rightDetail = _partPool.Get();
        rightDetail.name = "RightDetail";
        rightDetail.transform.SetParent(monster.transform, false);
        SpriteRenderer rightSR = rightDetail.GetComponent<SpriteRenderer>();
        rightSR.sprite = chosen;
        rightSR.color = _bodyColor;
        rightSR.sortingLayerName = detailSortingOrder;
        rightDetail.transform.SetLocalPositionAndRotation(
            new Vector3(_data.detailXPos + nudge, _data.detailYPos + nudge, _data.detailZPos),
            Quaternion.identity);

        GameObject leftDetail = _partPool.Get();
        leftDetail.name = "LeftDetail";
        leftDetail.transform.SetParent(monster.transform, false);
        SpriteRenderer leftSR = leftDetail.GetComponent<SpriteRenderer>();
        leftSR.sprite = chosen;
        leftSR.color = _bodyColor;
        leftSR.flipX = true;
        leftSR.sortingLayerName = detailSortingOrder;
        leftDetail.transform.SetLocalPositionAndRotation(
            new Vector3(-_data.detailXPos + nudge, _data.detailYPos + nudge, _data.detailZPos),
            Quaternion.identity);
    }

    private GameObject CreateMonsterRootObject()
    {
        GameObject go = new GameObject("PooledMonsterRoot");

        Rigidbody2D rb = go.AddComponent<Rigidbody2D>();
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        CircleCollider2D col = go.AddComponent<CircleCollider2D>();
        col.radius = 2f;

        go.AddComponent<Monster>();
        return go;
    }

    private GameObject CreatePartPoolObject()
    {
        GameObject go = new GameObject("PooledPart");
        go.AddComponent<SpriteRenderer>();
        return go;
    }

    private void OnGetFromPool(GameObject go)
    {
        go.transform.localScale = Vector3.one;
        go.transform.localPosition = Vector3.zero;
        go.transform.rotation = Quaternion.identity;
        go.SetActive(true);
    }

    private void OnReleaseToPool(GameObject go)
    {
        SpriteRenderer sr = go.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.sprite = null;
            sr.color = Color.white;
            sr.flipX = false;
        }

        Rigidbody2D rb = go.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }

        go.transform.SetParent(null);
        go.SetActive(false);
    }

    private void OnDestroyPooled(GameObject go) => Destroy(go);
}