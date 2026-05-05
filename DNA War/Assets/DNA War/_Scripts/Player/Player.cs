using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GridManager gridManager;
    [SerializeField] private PressureSystem pressureSystem;

    [Header("Runtime")]
    [SerializeField] private Vector2Int gridPosition;

    private Dictionary<Vector2Int, GameObject> grid;

    private PlayerInputAction inputActions;

    private void Awake()
    {
        inputActions = new PlayerInputAction();
    }

    void Start()
    {
        grid = gridManager.GetGrid();

        TeleportToCell(gridPosition);
    }

    private void OnEnable()
    {
        inputActions.PlayerMap.Enable();
        inputActions.PlayerMap.Movement.performed += HandlePlayerMovement;
    }

    private void OnDisable()
    {
        inputActions.PlayerMap.Disable();
        inputActions.PlayerMap.Movement.performed -= HandlePlayerMovement;
    }

    void Update()
    {
        
    }

    private void TryMove(Vector2Int direction)
    {
        Vector2Int newPos = gridPosition + direction;

        if (grid.TryGetValue(newPos, out GameObject tile))
        {
            gridPosition = newPos;
            MoveToCell(gridPosition);
        }
    }

    private void MoveToCell(Vector2Int cell)
    {
        if (grid.TryGetValue(cell, out GameObject tile))
        {
            transform.position = tile.transform.position;
            pressureSystem.AddStep();
        }
    }

    private void TeleportToCell(Vector2Int cell)
    {
        if (grid.TryGetValue(cell, out GameObject tile))
        {
            transform.position = tile.transform.position;
        }
    }

    private void HandlePlayerMovement(InputAction.CallbackContext context)
    {
        Vector2 raw = context.ReadValue<Vector2>();
        Vector2Int input = new Vector2Int(Mathf.RoundToInt(raw.y), Mathf.RoundToInt(raw.x));

        if (input != Vector2Int.zero)
            TryMove(input);
    }
}