using System;

/// <summary>
/// The rectangle (world units) the player aircraft is allowed to move in.
/// </summary>
[Serializable]
public class PlayerBoundary
{
    public float HorizontalMax;
    public float HorizontalMin;
    public float VerticalMax;
    public float VerticalMin;
}
