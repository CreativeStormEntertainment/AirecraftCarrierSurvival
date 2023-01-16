using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DebugPoiSpawner : MonoBehaviour
{
    [SerializeField]
    private ESandboxObjectiveType objectiveType = default;
    [SerializeField]
    private EMissionDifficulty difficulty = default;
    [SerializeField]
    private SandboxNode node = null;
    [SerializeField]
    private Button button = null;

    private void Awake()
    {
        button.onClick.AddListener(Spawn);
    }

    private void Spawn()
    {
        SandboxManager.Instance.PoiManager.DebugSpawnPoi(objectiveType, node, difficulty);
    }
}
