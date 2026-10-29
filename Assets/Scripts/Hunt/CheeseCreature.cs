using System.Collections;
using UnityEngine;

// Cheese: runs like Flour. Rolls into cheese holes, then pops out like whack-a-mole.
public class CheeseCreature : IngredientCreature1
{

    [Header("Cheese roll - Pravin can tune this")]
    [SerializeField] private float _rollTime = 0.6f; // seconds to roll thin

    [SerializeField] private float _popHeight = 0.5f; // how far it pokes out of the hole
    [SerializeField] private float _rumbleAmount = 0.08f; // how far the hole shakes
    private bool _inHole;
    private bool _poppedUp;
    private Vector3 _normalScale;

    protected override void Awake()
    {
        base.Awake();
        _normalScale = transform.localScale;
    }

    protected override void UpdateIdle()    { if (!_inHole) base.UpdateIdle(); }
    protected override void UpdateFleeing() { if (!_inHole) base.UpdateFleeing(); }
    protected override void UpdateStunned() { if (!_inHole) base.UpdateStunned(); }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_inHole || CurrentState != State.Fleeing) return;

        if (other.TryGetComponent(out CheeseHoleGroup group))
            StartCoroutine(WhackAMole(group));
    }

    private IEnumerator WhackAMole(CheeseHoleGroup group)
    {
        _inHole = true;
        _rb.linearVelocity = Vector2.zero;
        _rb.bodyType = RigidbodyType2D.Kinematic;

        // run to the nearest hole first
        Vector3 hole = group.NearestHolePosition(transform.position);
        Vector3 target = new Vector3(hole.x, transform.position.y, transform.position.z);

        while (Vector3.Distance(transform.position, target) > 0.05f)
        {
            transform.position = Vector3.MoveTowards(transform.position, target, _data.fleeSpeed * Time.deltaTime);
            yield return null;
        }

        // roll into a thin roll, then vanish
        for (float t = 0f; t < _rollTime; t += Time.deltaTime)
        {
            float x = Mathf.Lerp(1f, 0.2f, t / _rollTime);
            transform.localScale = new Vector3(_normalScale.x * x, _normalScale.y, _normalScale.z);
            yield return null;
        }
        SetVisible(false);
        transform.localScale = _normalScale;

        for (int i = 0; i < _data.popUpCount; i++)
        {
            yield return new WaitForSeconds(_data.hideTime);
            
            Transform popHole = group.RandomHole();
            yield return Rumble(popHole);

            transform.position = popHole.position + Vector3.up * _popHeight;
            SetVisible(true);
            _poppedUp = true;

            yield return new WaitForSeconds(_data.popUpTime);

            _poppedUp = false;
            SetVisible(false);
        }

        Debug.Log($"{name} got away through the cheese holes");
        Escape();
    }

    private IEnumerator Rumble(Transform hole)
    {
        Vector3 start = hole.localPosition;

        for (float t = 0f; t < _data.rumbleTime; t += Time.deltaTime)
        {
            hole.localPosition = start + (Vector3)Random.insideUnitCircle * _rumbleAmount;
            yield return null;
        }

        hole.localPosition = start;
    }

    public override void Stun()
    {
        if (!_inHole)
        {
            base.Stun();
            return;
        }

        if (!_poppedUp) return; // hiding, the bolt misses

        StopAllCoroutines();
        Debug.Log($"{name} whacked!");
        Catch();
    }

    private void SetVisible(bool visible)
    {
        foreach (Renderer r in GetComponentsInChildren<Renderer>())
            r.enabled = visible;
        _col.enabled = visible;
    }
}