using UnityEngine;

/// <summary>
/// Tilts the tail wing when the player moves up or down (keyboard input).
/// </summary>
public class WingMovement : MonoBehaviour
{
    public GameObject RightWing;
    public int RotateAngle;
    private Quaternion rotateSeed;

    void Start()
    {
        rotateSeed = transform.rotation;
    }

    void Update()
    {
        RotateWing(RightWing);
    }

    private void RotateWing(GameObject tail)
    {
        var vertical = Input.GetAxis("Vertical");
        if (vertical != 0)
        {
            tail.transform.rotation = rotateSeed * Quaternion.AngleAxis(vertical * RotateAngle, Vector3.right);
        }
    }
}
