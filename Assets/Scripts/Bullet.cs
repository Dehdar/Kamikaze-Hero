using UnityEngine;

/// <summary>
/// Moves a player bullet straight to the right.
/// </summary>
public class Bullet : MonoBehaviour
{
    public int Speed;
    private Rigidbody bulletRigidbody;

    void Start()
    {
        bulletRigidbody = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        bulletRigidbody.MovePosition(transform.position + Vector3.right * Speed * Time.deltaTime);
    }
}
