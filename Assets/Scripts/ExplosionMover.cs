using UnityEngine;

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
