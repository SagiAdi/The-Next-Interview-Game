using UnityEngine;

// Gives every distraction that the spawner creates a random type, and launches it.
[RequireComponent(typeof(Spawner))]
public class DistractionLauncher : MonoBehaviour
{
    [SerializeField, Tooltip("The kinds of distraction to choose from. Add a new asset here to add a new kind.")]
    private DistractionType[] types;

    private Spawner spawner;

    private void Awake()
    {
        spawner = GetComponent<Spawner>();
    }

    private void OnEnable()
    {
        spawner.Spawned += OnSpawned;
    }

    private void OnDisable()
    {
        spawner.Spawned -= OnSpawned;
    }

    private void OnSpawned(GameObject spawned)
    {
        if (types == null || types.Length == 0)
        {
            return;
        }

        if (spawned.TryGetComponent(out Distraction distraction))
        {
            distraction.Launch(types[Random.Range(0, types.Length)]);
        }
    }
}
