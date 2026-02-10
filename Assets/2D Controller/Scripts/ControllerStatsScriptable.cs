using UnityEngine;

[CreateAssetMenu(fileName = "ControllerStats", menuName = "New Controller Stats")]
public class ControllerStatsScriptable : ScriptableObject
{
    [Space]
    [Tooltip("Players layer mask to ignore in raycast")]
    public LayerMask playerLayer;
    [Tooltip("Offset of the ground detection ray needed in so that ray extends beyond the player collider")]
    public float groundCheckRayOffset;
    [Tooltip("Configure input deadzone to deal with stick drift on controllers")]
    public float deadZone;

    [Space(2), Header("Movement")]
    [Tooltip("Max speed player can reach")]
    public float maxSpeed;
    [Tooltip("Values by which player will accelerate to max speed")]
    public float acceleration;
    [Tooltip("Values by which player will deaccelerate on ground")]
    public float groundDeceleration;
    [Tooltip("Values by which player will deaccelerate in air")]
    public float airDeceleration;
    [Tooltip("Speed under which player will turnaround quicker than usual")]
    public float quickTurnAroundSpeed;

    [Space(2), Header("Jumps")]
    [Tooltip("Numer of times player can jump")]
    public int maxJumpCount;
    [Tooltip("Players jump velocity")]
    public float jumpPower;
    [Tooltip("Constante downward force applied while the player is grounded (helps with edges)")]
    public float groundingAcceleration;
    [Tooltip("Constante downward force applied while the player is in air (snappier jump)")]
    public float fallAcceleration;
    [Tooltip("Players max fall velocity")]
    public float maxFallSpeed;
    [Tooltip("Buffer time between consecutive jumps")]
    public float jumpBuffer;
    [Tooltip("Extra downward multiplier that comes in effect if the jump key key is released early")]
    public float jumpEndEarlyMultiplier;
    [Tooltip("Time buffer after player reaches max fall speed to wait to trigger hard fall")]
    public float hardFallTimeBuffer;
    [Tooltip("Max fall velocity triggered after falling for a while")]
    public float hardFallSpeed;
    [Tooltip("Time buffer where player can jump after leaving the edge")]
    public float coyoteTime;

    [Space(2), Header("Dash")]
    public bool dashEnabled;
    [Tooltip("Instantanious velocity player reaches while dashing")]
    public float dashVelocity;
    [Tooltip("Buffer time between consecutive dashes")]
    public float dashBuffer;
}   
