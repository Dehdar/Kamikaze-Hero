using UnityEngine;

public class BackgroundMover : MonoBehaviour
{
    public float ScrollSpeed;
    public float TilSizeZ;
    public float AccelerationSpeed;
    private Vector3 startPosition;

    public void Reset()
    {
        ScrollSpeed = -0.4f;
        AccelerationSpeed = -0.02f;
    }

    void Start()
    {
        startPosition = transform.position;
    }

    void Update()
    {
        ScrollSpeed += AccelerationSpeed * Time.deltaTime;
        float newPosition = Mathf.Repeat(Time.time * ScrollSpeed, TilSizeZ);
        transform.position = startPosition + Vector3.right * newPosition;
    }
}
