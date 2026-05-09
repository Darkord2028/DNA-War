using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    [SerializeField] private MonsterSpawner spawner;
    [SerializeField] private GridData gridData;
    [SerializeField] private GameObject cellPrefab;
    private Dictionary<Vector2Int, GameObject> grid = new Dictionary<Vector2Int, GameObject>();

    void Start()
    {
        BuildGrid();
    }

    [ContextMenu("Build Grid")]
    private void BuildGrid()
    {
        grid.Clear();

        Vector2 offset = new Vector2((gridData.columns - 1) * gridData.cellSize / 2f, (gridData.rows - 1) * gridData.cellSize / 2f);

        for (int i = 0; i < gridData.columns; i++)
        {
            for (int j = 0; j < gridData.rows; j++)
            {
                bool isCell = gridData.GetCell(j, i);
                if (!isCell) break;

                Vector2 position = new Vector2(i * gridData.cellSize, j * gridData.cellSize) - offset;
                GameObject tile = Instantiate(cellPrefab, position, Quaternion.identity);
                spawner.BuildMonster(position);
               
                if (tile.transform.GetChild(0).TryGetComponent<SpriteRenderer>(out SpriteRenderer renderer))
                {
                    renderer.color = Color.blue;
                }
                else
                {
                    Debug.Log("Renderer is not found!");
                }
                grid.Add(new Vector2Int(j, i), tile);
                tile.transform.parent = transform;
            }
        }

    }

    public Dictionary<Vector2Int, GameObject> GetGrid()
    {
        return grid;
    }

}
