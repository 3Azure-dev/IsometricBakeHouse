using System.Collections.Generic;
using UnityEngine;

public class CreatureSpawner : MonoBehaviour
{
    [Tooltip("The creature types this level can spawn")]
    [SerializeField] private IngredientCreature1[] _creaturePrefabs;

    [Tooltip("Every creature type spawns at least this many times. The rest of the points are random")]
    [SerializeField] private int _minPerType = 3;

    [Tooltip("A creature that falls below this height goes back to its spawn point")]
    [SerializeField] private float _fallHeight = -10f;

    private readonly List<IngredientCreature1> _creatures = new();
    private readonly List<Vector3> _homes = new();

    private void Start()
    {
        // 1. make a list of what to spawn
        List<IngredientCreature1> toSpawn = new();

        // first, the guaranteed ones
        foreach (IngredientCreature1 prefab in _creaturePrefabs)
            for (int n = 0; n < _minPerType; n++)
                toSpawn.Add(prefab);

        // then fill the leftover points with random types
        while (toSpawn.Count < transform.childCount)
            toSpawn.Add(_creaturePrefabs[Random.Range(0, _creaturePrefabs.Length)]);

        if (toSpawn.Count > transform.childCount)
        {
            Debug.LogError($"{name}: not enough spawn points! Need at least {toSpawn.Count}");
            return;
        }

        // 2. shuffle the list
        for (int i = 0; i < toSpawn.Count; i++)
        {
            int swap = Random.Range(i, toSpawn.Count);
            (toSpawn[i], toSpawn[swap]) = (toSpawn[swap], toSpawn[i]);
        }

        // 3. every child of this object is a spawn point
        for (int i = 0; i < transform.childCount; i++)
        {
            Vector3 home = transform.GetChild(i).position;
            IngredientCreature1 creature = Instantiate(toSpawn[i], home, Quaternion.identity);
            _creatures.Add(creature);
            _homes.Add(home);
        }
    }

    private void Update()
    {
        for (int i = 0; i < _creatures.Count; i++)
        {
            IngredientCreature1 creature = _creatures[i];
            if (!creature.gameObject.activeSelf) continue; // already caught

            if (creature.transform.position.y < _fallHeight)
            {
                creature.transform.position = _homes[i];
                creature.GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero;
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        // red line = the fall height
        Gizmos.color = Color.red;
        Gizmos.DrawLine(new Vector3(-1000f, _fallHeight, 0f), new Vector3(1000f, _fallHeight, 0f));
    }
}