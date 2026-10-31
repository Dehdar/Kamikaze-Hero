using UnityEngine;

public class PropelSpinner : MonoBehaviour
{
    public int Speed;

    void Update()
    {
        transform.Rotate(Vector3.forward, Speed * Time.deltaTime);
    }
}
