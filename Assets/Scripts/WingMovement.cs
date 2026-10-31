using UnityEngine;
using System.Collections;

public class WingMovement : MonoBehaviour
{
    //public GameObject LeftWing;
    public GameObject RightWing;
    public int RotateAngle;
    private Quaternion rotateSeed;

    void Start()
    {
        rotateSeed = transform.rotation;
    }

    void Update()
    {
        //RotateWing(LeftWing);
        RotateWing(RightWing);
    }

    private void RotateWing(GameObject tail)
    {
        var vertical = Input.GetAxis("Vertical");
        if (vertical != 0)
        {
            tail.transform.rotation = rotateSeed * Quaternion.AngleAxis(vertical * RotateAngle, Vector3.right);
            //0, -0.7, 0, 0.7
        }
    }
}
