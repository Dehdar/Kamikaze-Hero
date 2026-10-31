using UnityEngine;
using System.Collections;

public class MultiMissileForward : MonoBehaviour
{
    public int Speed;
    public int RotateSpeed;
    public int HorizontalMovement;
    public int HorizontalSpeed;
    private Rigidbody missileRigidbody;

    void Start()
    {
        missileRigidbody = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        missileRigidbody.MovePosition(transform.position + new Vector3(-1 * Speed, HorizontalMovement * HorizontalSpeed) * Time.deltaTime);
        transform.Rotate(new Vector3(0, 0, 1) * Time.deltaTime * RotateSpeed, Space.Self);

    }
}
