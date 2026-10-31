using UnityEngine;

/// <summary>
/// Scrolls the tiled background to the side, slowly speeding up over time.
/// </summary>
public class BackgroundMover : MonoBehaviour
{
    public float ScrollSpeed;
    public float TileSize;
    public float AccelerationSpeed;
    private Vector3 startPosition;

    // Restores the starting speed. Called by MenuController on restart; Unity also calls
    // Reset() in the editor when the component is added or reset in the inspector.
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
        float newPosition = Mathf.Repeat(Time.time * ScrollSpeed, TileSize);
        transform.position = startPosition + Vector3.right * newPosition;
    }
}
