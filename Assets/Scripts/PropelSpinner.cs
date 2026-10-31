using UnityEngine;

/// <summary>
/// Spins the aircraft propeller.
/// </summary>
public class PropelSpinner : MonoBehaviour
{
    public int Speed;

    void Update()
    {
        transform.Rotate(Vector3.forward, Speed * Time.deltaTime);
    }
}
