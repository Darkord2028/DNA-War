using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    [SerializeField] private int cellSize = 1;
    [SerializeField] private int numOfColumns = 10;
    [SerializeField] private int numOfRows = 10;

    [SerializeField] private GameObject cellPrefab;
    [SerializeField] private Dictionary<Vector2Int, GameObject> grid = new Dictionary<Vector2Int, GameObject>();

    void Start()
    {
        BuildGrid();
    }

    void Update()
    {
        
    }

    [ContextMenu("Build Grid")]
    private void BuildGrid()
    {
        grid.Clear();

        Vector2 offset = new Vector2((numOfColumns - 1) * cellSize / 2f, (numOfRows - 1) * cellSize / 2f);
        for (int i = 0; i < numOfColumns; i++)
        {
            for (int j = 0; j < numOfRows; j++)
            {
                Vector2 position = new Vector2(i * cellSize, j * cellSize) - offset;
                GameObject tile = Instantiate(cellPrefab, position, Quaternion.identity);
                SpriteRenderer renderer = tile.GetComponent<SpriteRenderer>();
                renderer.color = new Color32(
                    (byte)Random.Range(0, 256),
                    (byte)Random.Range(0, 256),
                    (byte)Random.Range(0, 256),
                    255
                );
                grid.Add(new Vector2Int(i, j), tile);
                tile.transform.parent = transform;
            }
        }
    }

}
