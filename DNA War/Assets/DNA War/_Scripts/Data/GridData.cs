using UnityEngine;

[CreateAssetMenu(fileName = "NewGridData", menuName = "Grid/Grid Data")]
public class GridData : ScriptableObject
{
    [Range(1, 20)] public int rows = 4;
    [Range(1, 20)] public int columns = 4;
    public float cellSize = 1f;

    public bool[] cells = new bool[16];

    public void ResizeCells()
    {
        bool[] newCells = new bool[rows * columns];

        if (cells != null)
        {
            int oldColumns = cells.Length > 0 ? (columns > 0 ? cells.Length / Mathf.Max(rows, 1) : columns) : columns;
            for (int i = 0; i < Mathf.Min(cells.Length, newCells.Length); i++)
                newCells[i] = cells[i];
        }

        cells = newCells;
    }

    public bool GetCell(int row, int col) => cells[row * columns + col];
    public void SetCell(int row, int col, bool value) => cells[row * columns + col] = value;
    public void ToggleCell(int row, int col) => cells[row * columns + col] = !cells[row * columns + col];
}
