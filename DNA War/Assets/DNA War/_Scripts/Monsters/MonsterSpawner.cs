using UnityEngine;

public class MonsterSpawner : MonoBehaviour
{
    [Header("Monster Data")]
    [SerializeField] MonsterData[] monsterData;

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

    private MonsterData _data;
    private Color _singleBodyColor;

    [ContextMenu("Make Monster")]
    public void MakeMonster()
    {
        _data = monsterData[Random.Range(0, monsterData.Length)];

        // Root
        GameObject monster = new GameObject("Monster");
        _singleBodyColor = bodyColor[Random.Range(0, bodyColor.Length)];

        MakeBody(monster);
        MakeHands(monster);
        MakeLegs(monster);
        MakeEyes(monster);
        MakeMouth(monster);
        MakeDetail(monster);
    }

    void MakeBody(GameObject monster)
    {
        GameObject body = new GameObject("Body");
        body.transform.SetParent(monster.transform, false);
        body.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
        SpriteRenderer sr = body.AddComponent<SpriteRenderer>();
        sr.sprite = _data.body;
        sr.color = _singleBodyColor;
        sr.sortingLayerName = bodySortingOrder;
    }

    void MakeHands(GameObject monster)
    {
        if (handSprites.Length == 0) return;
        Sprite chosen = handSprites[Random.Range(0, handSprites.Length)];
        float nudge = Random.Range(-_data.handPosRange, _data.handPosRange);
        Color color = handColor[Random.Range(0, handColor.Length)];

        // Left
        GameObject leftHand = new GameObject("LeftHand");
        leftHand.transform.SetParent(monster.transform, false);
        SpriteRenderer leftSR = leftHand.AddComponent<SpriteRenderer>();
        leftSR.sprite = chosen;
        leftSR.color = _singleBodyColor;
        leftSR.flipX = true;
        leftSR.sortingLayerName = handSortingOrder;
        leftHand.transform.SetLocalPositionAndRotation(
            new Vector3(-_data.handXPos + nudge, _data.handYPos + nudge, _data.handZPos),
            Quaternion.identity);

        // Right
        GameObject rightHand = new GameObject("RightHand");
        rightHand.transform.SetParent(monster.transform, false);
        SpriteRenderer rightSR = rightHand.AddComponent<SpriteRenderer>();
        rightSR.sprite = chosen;
        rightSR.color = _singleBodyColor;
        rightSR.sortingLayerName = handSortingOrder;
        rightHand.transform.SetLocalPositionAndRotation(
            new Vector3(_data.handXPos + nudge, _data.handYPos + nudge, _data.handZPos),
            Quaternion.identity);
    }

    void MakeLegs(GameObject monster)
    {
        if (legSprites.Length == 0) return;
        Sprite chosen = legSprites[Random.Range(0, legSprites.Length)];
        float nudge = Random.Range(-_data.legPosRange, _data.legPosRange);
        Color color = legColor[Random.Range(0, legColor.Length)];

        // Left
        GameObject leftLeg = new GameObject("LeftLeg");
        leftLeg.transform.SetParent(monster.transform, false);
        SpriteRenderer leftSR = leftLeg.AddComponent<SpriteRenderer>();
        leftSR.sprite = chosen;
        leftSR.color = _singleBodyColor;
        leftSR.flipX = true;
        leftSR.sortingLayerName = legSortingOrder;
        leftLeg.transform.SetLocalPositionAndRotation(
            new Vector3(-_data.legXPos + nudge, _data.legYPos + nudge, _data.legZPos),
            Quaternion.identity);

        // Right
        GameObject rightLeg = new GameObject("RightLeg");
        rightLeg.transform.SetParent(monster.transform, false);
        SpriteRenderer rightSR = rightLeg.AddComponent<SpriteRenderer>();
        rightSR.sprite = chosen;
        rightSR.color = _singleBodyColor;
        rightSR.sortingLayerName = legSortingOrder;
        rightLeg.transform.SetLocalPositionAndRotation(
            new Vector3(_data.legXPos + nudge, _data.legYPos + nudge, _data.legZPos),
            Quaternion.identity);
    }

    void MakeEyes(GameObject monster)
    {
        if (eyeSprites.Length == 0) return;
        Sprite chosen = eyeSprites[Random.Range(0, eyeSprites.Length)];
        float nudge = Random.Range(-_data.eyePosRange, _data.eyePosRange);

        // Left
        GameObject leftEye = new GameObject("LeftEye");
        leftEye.transform.SetParent(monster.transform, false);
        SpriteRenderer leftSR = leftEye.AddComponent<SpriteRenderer>();
        leftSR.sprite = chosen;
        leftSR.flipX = true;
        leftSR.sortingLayerName = eyeSortingOrder;
        leftEye.transform.SetLocalPositionAndRotation(
            new Vector3(-_data.eyeXPos + nudge, _data.eyeYPos + nudge, _data.eyeZPos),
            Quaternion.identity);

        // Right
        GameObject rightEye = new GameObject("RightEye");
        rightEye.transform.SetParent(monster.transform, false);
        SpriteRenderer rightSR = rightEye.AddComponent<SpriteRenderer>();
        rightSR.sprite = chosen;
        rightSR.sortingLayerName = eyeSortingOrder;
        rightEye.transform.SetLocalPositionAndRotation(
            new Vector3(_data.eyeXPos + nudge, _data.eyeYPos + nudge, _data.eyeZPos),
            Quaternion.identity);
    }

    void MakeMouth(GameObject monster)
    {
        if (mouthSprites.Length == 0) return;
        Sprite chosen = mouthSprites[Random.Range(0, mouthSprites.Length)];
        float nudge = Random.Range(-_data.mouthPosRange, _data.mouthPosRange);

        GameObject mouth = new GameObject("Mouth");
        mouth.transform.SetParent(monster.transform, false);
        SpriteRenderer sr = mouth.AddComponent<SpriteRenderer>();
        sr.sprite = chosen;
        sr.sortingLayerName = mouthSortingOrder;
        mouth.transform.SetLocalPositionAndRotation(
            new Vector3(_data.mouthXPos + nudge, _data.mouthYPos + nudge, _data.mouthZPos),
            Quaternion.identity);
    }

    void MakeDetail(GameObject monster)
    {
        if (detailSprites.Length == 0) return;
        Sprite chosen = detailSprites[Random.Range(0, detailSprites.Length)];
        float nudge = Random.Range(-_data.detailPosRange, _data.detailPosRange);
        Color color = detailColor[Random.Range(0, detailColor.Length)];

        GameObject rightDetail = new GameObject("Right Detail");
        rightDetail.transform.SetParent(monster.transform, false);
        SpriteRenderer right_sr = rightDetail.AddComponent<SpriteRenderer>();
        right_sr.sprite = chosen;
        right_sr.color = _singleBodyColor;
        right_sr.sortingLayerName = detailSortingOrder;
        rightDetail.transform.SetLocalPositionAndRotation(
            new Vector3(_data.detailXPos + nudge, _data.detailYPos + nudge, _data.detailZPos),
            Quaternion.identity);

        GameObject leftDetail = new GameObject("Left Detail");
        leftDetail.transform.SetParent(monster.transform, false);
        SpriteRenderer left_sr = leftDetail.AddComponent<SpriteRenderer>();
        left_sr.sprite = chosen;
        left_sr.color = _singleBodyColor;
        left_sr.flipX = true;
        left_sr.sortingLayerName = detailSortingOrder;
        leftDetail.transform.SetLocalPositionAndRotation(
            new Vector3(-_data.detailXPos + nudge, _data.detailYPos + nudge, _data.detailZPos),
            Quaternion.identity);
    }
}