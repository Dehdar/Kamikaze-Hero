using UnityEngine;

public class MissileForward : MonoBehaviour
{
    public int Speed;
    public int RotateSpeed;
    private Rigidbody missileRigidbody;

    void Start()
    {
        missileRigidbody = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        missileRigidbody.MovePosition(transform.position + Vector3.left * Speed * Time.deltaTime);
        transform.Rotate(new Vector3(1, 0, 0) * Time.deltaTime * RotateSpeed, Space.Self);
    }
}
