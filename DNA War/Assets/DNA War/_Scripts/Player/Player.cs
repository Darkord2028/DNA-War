using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    [SerializeField] private MonsterSpawner spawner;
    [SerializeField] private float roundEndDelay = 1.5f;

    [Header("Game Juice")]
    [SerializeField] private ParticleSystem bloodParticleSystem;
    [SerializeField] private AudioClip bloodAudio;

    private PlayerInputAction inputActions;
    private Camera _mainCamera;
    private Monster _targetMonster;
    private AudioSource _audioSource;

    private Dictionary<int, Monster> _activeMonsters = new();
    private bool _canSelect = false;

    private void Awake()
    {
        inputActions = new PlayerInputAction();
    }

    void Start()
    {
        _mainCamera = Camera.main;
        _audioSource = GetComponent<AudioSource>();
        _activeMonsters = spawner.GetActiveMonsters();
    }

    private void OnEnable()
    {
        inputActions.PlayerMap.Enable();
        inputActions.PlayerMap.Select.performed += HandlePlayerSelect;
        GameEvents.OnPlacementReady += HandlePlacementReady;
        GameEvents.OnCorrectSelection += HandleCorrectSelection;
        GameEvents.OnWrongSelection += HandleWrongSelection;
        GameEvents.OnRoundStart += HandleRoundStart;
        GameEvents.OnTimeUp += HandleTimeUp;
    }

    private void OnDisable()
    {
        inputActions.PlayerMap.Disable();
        inputActions.PlayerMap.Select.performed -= HandlePlayerSelect;
        GameEvents.OnPlacementReady -= HandlePlacementReady;
        GameEvents.OnCorrectSelection -= HandleCorrectSelection;
        GameEvents.OnWrongSelection -= HandleWrongSelection;
        GameEvents.OnRoundStart -= HandleRoundStart;
        GameEvents.OnTimeUp -= HandleTimeUp;
    }

    private void HandleRoundStart()
    {
        _activeMonsters = spawner.GetActiveMonsters();
    }

    private void HandleTimeUp()
    {
        _canSelect = false;
        GameEvents.RoundEnd();
        Invoke(nameof(StartNextRound), 1.5f);
    }

    private void HandlePlacementReady()
    {
        var all = new List<Monster>(_activeMonsters.Values);
        _targetMonster = all[Random.Range(0, all.Count)];
        GameEvents.TargetSelected(_targetMonster);
        GameEvents.CluesReady(_targetMonster.monsterParts);
        _canSelect = true;
    }

    private void HandleCorrectSelection(float similarity)
    {
        Debug.Log($"Correct! Similarity: {similarity * 100f}%");
        _canSelect = false;
        StartCoroutine(DelayRoundEnd(roundEndDelay));
    }

    private void HandleWrongSelection(float similarity)
    {
        Debug.Log($"Wrong! Similarity: {similarity * 100f}%");
        _canSelect = false;
        StartCoroutine(DelayRoundEnd(roundEndDelay));
    }

    private IEnumerator DelayRoundEnd(float roundEndDelay)
    {
        yield return new WaitForSeconds(roundEndDelay);
        GameEvents.RoundEnd();
        Invoke(nameof(StartNextRound), 0.5f);
    }

    private void StartNextRound()
    {
        spawner.RebuildGrid();
    }

    private void HandlePlayerSelect(InputAction.CallbackContext context)
    {
        if (!_canSelect) return;

        RaycastHit2D rayHit = Physics2D.GetRayIntersection(
            _mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue()));

        if (!rayHit.collider) return;

        foreach (var kvp in _activeMonsters)
        {
            Monster monster = kvp.Value;
            if (monster.gameObject == rayHit.collider.gameObject)
            {
                float similarity = _targetMonster.GetSimilarity(monster);

                if (monster.monsterID == _targetMonster.monsterID)
                {
                    GameEvents.CorrectSelection(similarity);
                    bloodParticleSystem.transform.position = rayHit.collider.transform.position;
                    bloodParticleSystem.Play();
                    if (_audioSource && bloodAudio)
                    {
                        _audioSource.PlayOneShot(bloodAudio);
                    }
                }
                else
                {
                    GameEvents.WrongSelection(similarity);
                }

                return;
            }
        }
    }
}