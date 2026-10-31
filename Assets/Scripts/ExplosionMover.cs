using UnityEngine;

/// <summary>
/// Moves an explosion effect to the left so it drifts with the scrolling scene.
/// </summary>
public class ExplosionMover : MonoBehaviour
{
    public int Speed;
    private Rigidbody missileRigidbody;

    void Start() {
        missileRigidbody = GetComponent<Rigidbody>();
    }
    void Update()
    {
        missileRigidbody.MovePosition(transform.position + Vector3.left * Speed * Time.deltaTime);
    }
}
