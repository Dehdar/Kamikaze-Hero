using UnityEngine;

/// <summary>
/// Moves the player: arrow keys/WASD in the editor, touch-drag on Android.
/// </summary>
public class PlayerMovement : MonoBehaviour
{
    public int MovementSpeed;
    public PlayerBoundary Boundary;
    public int TouchSensitivityX;
    public int TouchSensitivityY;
    private Rigidbody playerRigidbody;
    private FireController fireController;
    private KamikazeController kamikazeController;

    void Start()
    {
        fireController = GameObject.FindGameObjectWithTag(Tags.FireController).GetComponent<FireController>();
        kamikazeController = GameObject.FindGameObjectWithTag(Tags.KamikazeController).GetComponent<KamikazeController>();
        playerRigidbody = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
#if UNITY_EDITOR
        Move(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
#endif

#if UNITY_ANDROID
        // Move with the one finger that is not holding the fire or kamikaze button:
        // one touch and no button held, two touches and one button held, or three touches.
        if (Input.touchCount == 3 ||
            (Input.touchCount == 2 && !fireController.IsFirePressed() && kamikazeController.IsKamikazeFireTouched()) ||
            (Input.touchCount == 2 && fireController.IsFirePressed() && !kamikazeController.IsKamikazeFireTouched()) ||
            (Input.touchCount == 1 && !fireController.IsFirePressed() && !kamikazeController.IsKamikazeFireTouched()))
        {
            var input = Input.GetTouch(fireController.IsFirstTouchFire ? 1 : 0).deltaPosition;
            Move(input.x / TouchSensitivityX, input.y / TouchSensitivityY);
        }
#endif
    }

    private void Move(float x, float y)
    {
        var newPosition = transform.position + new Vector3(x, y, 0) * MovementSpeed * Time.deltaTime;
        var clampedPosition = ClampPositionAgainstBoundaries(newPosition);
        playerRigidbody.MovePosition(clampedPosition);
    }

    private Vector3 ClampPositionAgainstBoundaries(Vector3 movement)
    {
        movement.x = Mathf.Clamp(movement.x, Boundary.HorizontalMin, Boundary.HorizontalMax);
        movement.y = Mathf.Clamp(movement.y, Boundary.VerticalMin, Boundary.VerticalMax);
        return movement;
    }
}
