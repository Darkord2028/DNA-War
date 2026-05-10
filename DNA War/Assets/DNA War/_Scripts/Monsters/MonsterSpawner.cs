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

    [Header("Pool Settings")]
    [SerializeField] int defaultPoolCapacity = 10;
    [SerializeField] int maxPoolSize = 100;

    [Header("Grid Settings")]
    [SerializeField] int columnNum = 2;
    [SerializeField] int rowNum = 2;
    [SerializeField] int cellSize = 1;
    [SerializeField] Sprite cellSprite;

    private MonsterData _data;
    private Monster _currentMonster;
    private Color _bodyColor;
    private Color _limbColor;

    private Dictionary<int, Monster> activeMonsters = new();
    private Dictionary<Vector2, SpriteRenderer> grid = new();

    private ObjectPool<GameObject> _tilePool;
    private ObjectPool<GameObject> _partPool;
    private ObjectPool<GameObject> _bodyPool;

    private int maxAttempts = 20;
    private HashSet<int> _usedCombinations = new();

    private void Awake()
    {
        _tilePool = new ObjectPool<GameObject>(
            CreateTilePoolObject,
            OnGetFromPool,
            OnReleaseToPool,
            OnDestroyPooled,
            true, defaultPoolCapacity, maxPoolSize);

        _partPool = new ObjectPool<GameObject>(
            CreatePartPoolObject,
            OnGetFromPool,
            OnReleaseToPool,
            OnDestroyPooled,
            true, defaultPoolCapacity, maxPoolSize);

        _bodyPool = new ObjectPool<GameObject>(
            CreateBodyPoolObject,
            OnGetFromPool,
            OnReleaseToPool,
            OnDestroyPooled,
            true, defaultPoolCapacity, maxPoolSize);
    }

    private void Start()
    {
        BuildGrid();
    }

    private void BuildGrid()
    {
        Vector2 offset = new Vector2((columnNum - 1) * cellSize / 2f, (rowNum - 1) * cellSize / 2f);

        for (int i = 0; i < columnNum; i++)
        {
            for (int j = 0; j < rowNum; j++)
            {
                Vector2 position = new Vector2(i * cellSize, j * cellSize) - offset;

                GameObject tile = _tilePool.Get();
                tile.name = "Tile";
                tile.transform.SetParent(transform);
                tile.transform.position = position;

                SpriteRenderer tileRenderer = tile.GetComponent<SpriteRenderer>();
                tileRenderer.sprite = cellSprite;
                tileRenderer.color = Color.white;
                tileRenderer.sortingOrder = -1;

                grid.Add(new Vector2(j, i), tileRenderer);

                Monster monster = BuildMonster(position);
                if (monster == null)
                    Debug.LogWarning($"[MonsterSpawner] Monster is null at grid ({j},{i})");
            }
        }

        GameEvents.PlacementReady();
    }

    public void RebuildGrid()
    {
        ReturnAllToPool();

        activeMonsters.Clear();
        _usedCombinations.Clear();
        grid.Clear();

        GameEvents.RoundStart();
        BuildGrid();
    }

    private void ReturnAllToPool()
    {
        foreach (var kvp in activeMonsters)
        {
            Monster monster = kvp.Value;
            if (monster == null) continue;

            GameObject monsterRoot = monster.gameObject;

            List<Transform> children = new List<Transform>();
            foreach (Transform child in monsterRoot.transform)
                children.Add(child);

            foreach (Transform child in children)
                _partPool.Release(child.gameObject);

            _bodyPool.Release(monsterRoot);
        }

        foreach (var kvp in grid)
        {
            SpriteRenderer tileRenderer = kvp.Value;
            if (tileRenderer == null) continue;
            _tilePool.Release(tileRenderer.gameObject);
        }
    }

    public Dictionary<Vector2, SpriteRenderer> GetGrid() => grid;
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

    private Monster BuildMonster(Vector3 spawnPos)
    {
        for (int i = 0; i < maxAttempts; i++)
        {
            MonsterParts parts = GenerateRandomParts();
            int key = GetUniqueKey(parts);

            if (_usedCombinations.Add(key))
                return MakeMonster(parts, key, spawnPos);
        }

        Debug.LogWarning("[MonsterSpawner] Could not generate a unique monster. Add more sprite or color variety.");
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
            parts.HandIndex,
            parts.LegIndex,
            parts.EyeIndex,
            parts.MouthIndex,
            parts.DetailIndex,
            parts.DataIndex,
            parts.ColorPaletteIndex
        );
    }

    private Monster MakeMonster(MonsterParts parts, int key, Vector2 spawnPos)
    {
        _data = monsterData[parts.DataIndex];
        _bodyColor = colors[parts.ColorPaletteIndex].bodyColor;
        _limbColor = colors[parts.ColorPaletteIndex].limbColor;

        GameObject monsterRoot = _bodyPool.Get();
        monsterRoot.name = $"Monster_{key}";
        monsterRoot.transform.SetParent(null);
        monsterRoot.transform.position = spawnPos;
        monsterRoot.transform.localScale = Vector3.one * 0.45f;

        _currentMonster = monsterRoot.AddComponent<Monster>();
        _currentMonster.monsterParts = parts;
        _currentMonster.monsterID = key;

        activeMonsters[key] = _currentMonster;

        MakeBody(monsterRoot);
        MakeHands(monsterRoot, parts.HandIndex);
        MakeLegs(monsterRoot, parts.LegIndex);
        MakeEyes(monsterRoot, parts.EyeIndex);
        MakeMouth(monsterRoot, parts.MouthIndex);
        MakeDetail(monsterRoot, parts.DetailIndex);

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

    private GameObject CreateTilePoolObject()
    {
        GameObject go = new GameObject("PooledTile");
        go.AddComponent<SpriteRenderer>();
        return go;
    }

    private GameObject CreatePartPoolObject()
    {
        GameObject go = new GameObject("PooledPart");
        go.AddComponent<SpriteRenderer>();
        return go;
    }

    private GameObject CreateBodyPoolObject()
    {
        GameObject go = new GameObject("PooledBody");
        go.AddComponent<SpriteRenderer>();
        BoxCollider2D collider2D = go.AddComponent<BoxCollider2D>();
        collider2D.size = new Vector2(3f, 3f);
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
        if (go.TryGetComponent(out Monster monster))
            Destroy(monster);

        SpriteRenderer sr = go.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.sprite = null;
            sr.color = Color.white;
            sr.flipX = false;
        }

        go.transform.SetParent(null);
        go.SetActive(false);
    }

    private void OnDestroyPooled(GameObject go) => Destroy(go);
}