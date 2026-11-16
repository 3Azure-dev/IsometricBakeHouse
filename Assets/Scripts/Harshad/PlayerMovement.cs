using System.Security.Cryptography.X509Certificates;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private Transform visualRoot;
    [SerializeField] private float turnSpeed = 360f;

    private Rigidbody rb;
    private Vector3 movement;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        if (BlocksPlayerWhileOpen.IsBlocked)
        {
            movement = Vector3.zero;
            return;
        }


        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        movement = new Vector3(horizontal, 0f, vertical).normalized;
    }

    private void FixedUpdate()
    {
        if (movement.sqrMagnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(movement, Vector3.up);

            visualRoot.rotation = Quaternion.RotateTowards(
                visualRoot.rotation,
                targetRotation,
                turnSpeed * Time.fixedDeltaTime
            );
        }
        Vector3 velocity = movement * moveSpeed;
        velocity.y = rb.linearVelocity.y;   // keep gravity working
        rb.linearVelocity = velocity;
    }
}