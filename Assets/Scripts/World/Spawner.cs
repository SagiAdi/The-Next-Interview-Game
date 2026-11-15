using System.Collections.Generic;
using UnityEngine;

// Spawns copies of the given prefabs (one is chosen at random) at free points inside an area,
// at a fixed interval. It never spawns inside a wall or on top of the player.
public class Spawner : MonoBehaviour
{
    [SerializeField] private GameObject[] prefabs;
    [SerializeField] private Vector2 areaMin = new Vector2(-6f, -3f);
    [SerializeField] private Vector2 areaMax = new Vector2(6f, 3f);
    [SerializeField] private float interval = 5f;
    [SerializeField] private int maxAlive = 4;

    [SerializeField, Tooltip("A new object needs this much free space around it.")]
    private float freeRadius = 0.5f;

    private readonly List<GameObject> alive = new List<GameObject>();
    private readonly List<Collider2D> overlaps = new List<Collider2D>();
    private ContactFilter2D solidOnly;
    private float timer;

    // Raised for every object right after it is created.
    public event System.Action<GameObject> Spawned;

    private void Awake()
    {
        solidOnly = new ContactFilter2D();
        solidOnly.useTriggers = false;
        timer = interval;
    }

    private void Update()
    {
        alive.RemoveAll(item => item == null);

        timer -= Time.deltaTime;
        if (timer > 0f || prefabs == null || prefabs.Length == 0 || alive.Count >= maxAlive)
        {
            return;
        }

        timer = interval;
        SpawnAtFreePoint();
    }

    private void SpawnAtFreePoint()
    {
        for (int attempt = 0; attempt < 20; attempt++)
        {
            Vector2 point = new Vector2(Random.Range(areaMin.x, areaMax.x), Random.Range(areaMin.y, areaMax.y));
            if (Physics2D.OverlapCircle(point, freeRadius, solidOnly, overlaps) > 0)
            {
                continue;
            }

            GameObject prefab = prefabs[Random.Range(0, prefabs.Length)];
            GameObject spawned = Instantiate(prefab, point, Quaternion.identity, transform);
            alive.Add(spawned);
            Spawned?.Invoke(spawned);
            return;
        }
    }
}
