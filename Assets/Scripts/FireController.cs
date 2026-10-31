using UnityEngine;

/// <summary>
/// Tracks whether the on-screen fire button is held (wired via EventTrigger in the Game scene).
/// </summary>
public class FireController : MonoBehaviour
{
    // True when the fire button was pressed with the first finger, so PlayerMovement
    // knows to read movement from the second touch instead.
    public bool IsFirstTouchFire;
    private bool touched;

    public void OnPointerDown()
    {
        if (Input.touchCount == 1)
        {
            IsFirstTouchFire = true;
        }
        touched = true;
    }

    public void OnPointerUp()
    {
        IsFirstTouchFire = false;
        touched = false;
    }

    public bool IsFirePressed()
    {
        return touched;
    }
}
