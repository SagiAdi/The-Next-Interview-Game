using System.Collections.Generic;
using UnityEngine;

// Spawns copies of one of the given prefabs (chosen at random) at random points
// inside an area, at a fixed interval. It never spawns inside walls or on the player.
public class Spawner : MonoBehaviour
{
    [SerializeField] private GameObject[] prefabs;
    [SerializeField] private Vector2 areaMin = new Vector2(-6f, -3f);
    [SerializeField] private Vector2 areaMax = new Vector2(6f, 3f);
    [SerializeField] private float interval = 5f;
    [SerializeField] private int maxAlive = 4;

    readonly List<GameObject> alive = new List<GameObject>();
    readonly List<Collider2D> hits = new List<Collider2D>();
    ContactFilter2D solidOnly;
    float timer;

    // Used by the setup wizard to configure the spawner.
    public void Setup(GameObject[] prefabs, Vector2 areaMin, Vector2 areaMax, float interval, int maxAlive)
    {
        this.prefabs = prefabs;
        this.areaMin = areaMin;
        this.areaMax = areaMax;
        this.interval = interval;
        this.maxAlive = maxAlive;
    }

    void Awake()
    {
        solidOnly = new ContactFilter2D();
        solidOnly.useTriggers = false;
        timer = interval;
    }

    void Update()
    {
        alive.RemoveAll(item => item == null);

        timer -= Time.deltaTime;
        if (timer > 0f || prefabs == null || prefabs.Length == 0 || alive.Count >= maxAlive) return;
        timer = interval;

        for (int attempt = 0; attempt < 20; attempt++)
        {
            Vector2 p = new Vector2(Random.Range(areaMin.x, areaMax.x), Random.Range(areaMin.y, areaMax.y));
            if (Physics2D.OverlapCircle(p, 0.5f, solidOnly, hits) == 0)
            {
                GameObject prefab = prefabs[Random.Range(0, prefabs.Length)];
                alive.Add(Instantiate(prefab, p, Quaternion.identity, transform));
                return;
            }
        }
    }
}
