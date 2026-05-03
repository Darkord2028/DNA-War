using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(GridData))]
public class GridDataEditor : Editor
{
    private const float CELL_SIZE = 30f;
    private const float CELL_GAP = 4f;

    private static readonly Color COLOR_ACTIVE = new Color(0.25f, 0.75f, 0.45f);
    private static readonly Color COLOR_INACTIVE = new Color(0.22f, 0.22f, 0.22f);

    public override void OnInspectorGUI()
    {
        GridData grid = (GridData)target;
        serializedObject.Update();

        EditorGUILayout.Space(4);
        GUIStyle headerStyle = new GUIStyle(EditorStyles.boldLabel)
        {
            fontSize = 13,
            alignment = TextAnchor.MiddleCenter
        };
        EditorGUILayout.LabelField("Grid Configuration", headerStyle);
        EditorGUILayout.Space(6);

        EditorGUI.BeginChangeCheck();

        int newRows = EditorGUILayout.IntSlider("Rows", grid.rows, 1, 20);
        int newCols = EditorGUILayout.IntSlider("Columns", grid.columns, 1, 20);
        float newCellSize = EditorGUILayout.FloatField("Cell Size (world units)", grid.cellSize);

        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(grid, "Resize Grid");
            grid.rows = newRows;
            grid.columns = newCols;
            grid.cellSize = Mathf.Max(0.01f, newCellSize);
            grid.ResizeCells();
            EditorUtility.SetDirty(grid);
        }

        EditorGUILayout.Space(6);

        int activeCount = 0;
        if (grid.cells != null)
            foreach (bool c in grid.cells) if (c) activeCount++;

        GUIStyle statStyle = new GUIStyle(EditorStyles.miniLabel) { alignment = TextAnchor.MiddleCenter };
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField($"Total: {grid.rows * grid.columns}", statStyle);
        EditorGUILayout.LabelField($"Active: {activeCount}", statStyle);
        EditorGUILayout.LabelField($"Empty: {grid.rows * grid.columns - activeCount}", statStyle);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(4);
        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("Fill All", EditorStyles.miniButton))
        {
            Undo.RecordObject(grid, "Fill All Cells");
            for (int i = 0; i < grid.cells.Length; i++) grid.cells[i] = true;
            EditorUtility.SetDirty(grid);
        }

        if (GUILayout.Button("Clear All", EditorStyles.miniButton))
        {
            Undo.RecordObject(grid, "Clear All Cells");
            for (int i = 0; i < grid.cells.Length; i++) grid.cells[i] = false;
            EditorUtility.SetDirty(grid);
        }

        if (GUILayout.Button("Invert", EditorStyles.miniButton))
        {
            Undo.RecordObject(grid, "Invert Cells");
            for (int i = 0; i < grid.cells.Length; i++) grid.cells[i] = !grid.cells[i];
            EditorUtility.SetDirty(grid);
        }

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(8);

        if (grid.cells == null || grid.cells.Length != grid.rows * grid.columns)
        {
            grid.ResizeCells();
            EditorUtility.SetDirty(grid);
        }

        float gridWidth = grid.columns * (CELL_SIZE + CELL_GAP) - CELL_GAP;
        float gridHeight = grid.rows * (CELL_SIZE + CELL_GAP) - CELL_GAP;

        Rect canvasRect = GUILayoutUtility.GetRect(gridWidth, gridHeight + 4);
        float startX = canvasRect.x + (canvasRect.width - gridWidth) * 0.5f;
        float startY = canvasRect.y + 2f;

        // Transparent button style — color comes from DrawRect underneath
        GUIStyle flatButton = new GUIStyle(GUI.skin.button);
        flatButton.normal.background = null;
        flatButton.hover.background = null;
        flatButton.active.background = null;
        flatButton.fontSize = 16;
        flatButton.alignment = TextAnchor.MiddleCenter;
        flatButton.normal.textColor = new Color(1f, 1f, 1f, 0.85f);

        for (int r = 0; r < grid.rows; r++)
        {
            for (int c = 0; c < grid.columns; c++)
            {
                int index = r * grid.columns + c;
                Rect cellRect = new Rect(
                    startX + c * (CELL_SIZE + CELL_GAP),
                    startY + r * (CELL_SIZE + CELL_GAP),
                    CELL_SIZE,
                    CELL_SIZE
                );

                bool isActive = grid.cells[index];

                // Colored background rect
                EditorGUI.DrawRect(cellRect, isActive ? COLOR_ACTIVE : COLOR_INACTIVE);

                // Native button on top (handles hover + click)
                if (GUI.Button(cellRect, isActive ? "✓" : "", flatButton))
                {
                    Undo.RecordObject(grid, "Toggle Cell");
                    grid.ToggleCell(r, c);
                    EditorUtility.SetDirty(grid);
                }
            }
        }

        serializedObject.ApplyModifiedProperties();
    }

    private void DrawSwatch(Color color)
    {
        Rect r = GUILayoutUtility.GetRect(14, 14, GUILayout.Width(14), GUILayout.Height(14));
        EditorGUI.DrawRect(r, color);
    }
}