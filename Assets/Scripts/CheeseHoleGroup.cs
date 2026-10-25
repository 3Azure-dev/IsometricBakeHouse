using UnityEngine;

// A group of cheese holes. Only Cheese can go in.
[RequireComponent(typeof(Collider2D))]
public class CheeseHoleGroup : MonoBehaviour
{
    [SerializeField] private Transform[] _holes;

    private void Awake()
    {
        GetComponent<Collider2D>().isTrigger = true;
    }

    public Vector3 RandomHolePosition()
    {
        return _holes[Random.Range(0, _holes.Length)].position;
    }

    private void OnDrawGizmos()
    {
        if (_holes == null) return;
        Gizmos.color = new Color(1f, 0.85f, 0.2f);
        foreach (Transform hole in _holes)
            if (hole != null) Gizmos.DrawWireSphere(hole.position, 0.3f);
    }

    public Vector3 NearestHolePosition(Vector3 from)
    {
        Vector3 best = _holes[0].position;
        foreach (Transform hole in _holes)
            if (Mathf.Abs(hole.position.x - from.x) < Mathf.Abs(best.x - from.x))
                best = hole.position;
        return best;
    }
}