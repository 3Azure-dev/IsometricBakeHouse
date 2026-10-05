using System.Collections.Generic;
using UnityEngine;

public class CreatureSpawner : MonoBehaviour
{
    [SerializeField] private IngredientCreature _creaturePrefab;

    [Tooltip("A creature that falls below this height goes back to its spawn point")]
    [SerializeField] private float _fallHeight = -10f;

    private readonly List<IngredientCreature> _creatures = new();
    private readonly List<Vector3> _homes = new();

    private void Start()
    {
        // every child of this object is a spawn point
        foreach (Transform point in transform)
        {
            IngredientCreature creature = Instantiate(_creaturePrefab, point.position, Quaternion.identity);
            _creatures.Add(creature);
            _homes.Add(point.position);
        }
    }

    private void Update()
    {
        for (int i = 0; i < _creatures.Count; i++)
        {
            IngredientCreature creature = _creatures[i];
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