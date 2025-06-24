using System;
using System.Collections;
using UnityEngine;

public interface IPlayerInterface
{
    public event Action Jumped;
    public event Action<bool> Grounded;
    public event Action Dashed;
    public Vector2 InputDirection { get; }
    public Vector2 PlayerVelocity { get; }
}
public class PlayerController : MonoBehaviour, IPlayerInterface
{
    public ControllerStatsScriptable stats;

    Vector2 _inputVelocity;
    public Vector2 InputVelocity { set { _inputVelocity = value; } }

    float _time = 0;

    Collider2D _col;
    Rigidbody2D _rb;

    public Vector2 InputDirection => _inputVelocity;
    public Vector2 PlayerVelocity => _rb.velocity;
    public event Action Jumped;
    public event Action<bool> Grounded;
    public event Action Dashed;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>() ? GetComponent<Rigidbody2D>() : gameObject.AddComponent<Rigidbody2D>();
        _rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        _rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        _col = GetComponent<Collider2D>() ? GetComponent<Collider2D>() : gameObject.AddComponent<Collider2D>();
    }
    private void OnDisable()
    {
        StopAllCoroutines();
    }
    private void FixedUpdate()
    {
        _rb.velocity = _currentVelocity;

        CheckGrounded();
        CheckCeiling();
        HorizontalVelocity();
        HandleDash();
        Gravity();

        _time += Time.deltaTime;
    }

    #region HORIZONTAL MOVEMENT
    Vector2 _currentVelocity;
    float _deceleration;
    public void HorizontalVelocity()
    {
        if(_dashing) return;
        if (Mathf.Abs(_inputVelocity.x) < stats.deadZone || Mathf.Abs(_rb.velocity.x) > stats.maxSpeed)
        {
            _deceleration = _grounded ? stats.groundDeceleration : stats.airDeceleration;
            _currentVelocity.x = Mathf.MoveTowards(_currentVelocity.x, 0, _deceleration * Time.deltaTime);
        }
        else
        {
            _currentVelocity.x = Mathf.MoveTowards(_currentVelocity.x, stats.maxSpeed * _inputVelocity.x, stats.acceleration * Time.deltaTime);
        }
    }
    #endregion

    #region JUMP
    float _jumpPressTime = 0;
    float _jumpReleaseTime = 0;
    int _jumpCount = 0;
    bool _inCoyoteTime = true;
    bool _jumpEndEarly = false;
    Coroutine _jumpCoroutine = null;
    Coroutine _coyoteCoroutine = null;
    public void JumpInput()
    {
        if (_jumpCount == 0)
        {
            if (_grounded && stats.jumpBuffer <= (_time - _jumpPressTime))
            {
                Jump();
            }
        }
        else if(_jumpCount > 0 && _jumpCount < stats.maxJumpCount)
        {
            Jump();
        }
    }
    void Jump()
    {
        Jumped?.Invoke();
        _jumpPressTime = _time;
        _jumpEndEarly = false;
        JumpEnd();
        _jumpCoroutine = StartCoroutine(JumpRoutine());
        _jumpCount++;
    }
    public void JumpReleased()
    {
        JumpEnd();
        _jumpReleaseTime = _time;
    }
    void JumpEnd()
    {
        if (_jumpCoroutine != null)
        {
            StopCoroutine(_jumpCoroutine);
            _jumpCoroutine = null;
            if (_rb.velocity.y > 0 && (_jumpPressTime - _jumpReleaseTime) < 1.0f)
            {
                _jumpEndEarly = true;
            }
        }
    }
    IEnumerator JumpRoutine()
    {
        float t = 0f;
        while (t < 0.1f && !_dashing)
        {
            t += Time.deltaTime;
            _currentVelocity.y = stats.jumpPower;    
            yield return null;
        }
    }
    IEnumerator CoyoteTime()
    {
        _inCoyoteTime = true;
        yield return new WaitForSeconds(stats.coyoteTime);
        _grounded = false;
        _inCoyoteTime = false;
    }
    #endregion

    #region DASH
    bool _dashing;
    bool _dashInput;
    bool _canDash;
    int _dashedFrames = 0;
    int _dashInputFrames = 0;
    float _dashedTime = 0;
    Vector2 _dashVelocity;
    public void DashInput()
    {
        if(stats.dashEnabled)
            _dashInput = true;
    }
    void HandleDash()
    {
        if(_dashInput && _canDash && stats.dashBuffer <= (_time - _dashedTime))
        {
            Vector2 dir = _inputVelocity.normalized;
            if (_dashInputFrames < 3)
            {
                _dashInputFrames++;
                return;
            }
            if (dir != Vector2.zero)
            {
                _dashInputFrames = 0;
                _dashInput = false;
                _dashVelocity = dir * stats.dashVelocity;
                _canDash = false;
                _dashing = true;
                _dashedTime = _time;
                _jumpEndEarly = false;
                _currentVelocity = Vector2.zero;
                Dashed?.Invoke();
            }
        }
        if (_dashing)
        {
            _currentVelocity = _dashVelocity;
            _dashedFrames++;
            if (_dashedFrames > 5)
            {
                _dashing = false;
                _dashedFrames = 0;
                _dashInput = false;
                if (_grounded) _canDash = true;
            }
        }
        _dashInputFrames = 0;
        _dashInput = false;
    }
    #endregion

    #region GRAVITY
    bool _grounded = true;
    void Gravity()
    {
        if(_dashing) return;
        if (_grounded && !_inCoyoteTime)
        {
            _currentVelocity.y = -stats.groundingAcceleration;
        }
        else
        {
            if (_jumpEndEarly)
            {
                _currentVelocity.y = Mathf.MoveTowards(_currentVelocity.y, -stats.maxFallSpeed, stats.fallAcceleration * Time.deltaTime * stats.jumpEndEarlyMultiplier);
            }
            else
            {
                _currentVelocity.y = Mathf.MoveTowards(_currentVelocity.y, -stats.maxFallSpeed, stats.fallAcceleration * Time.deltaTime);
            }
        }
    }
    #endregion

    #region BOUND CHECKS
    void CheckGrounded()
    {
        if (Physics2D.CircleCast(_col.bounds.center, _col.bounds.size.x / 2, Vector2.down, _col.bounds.size.y / 2 - stats.groundCheckRayOffset, ~stats.playerLayer))
        {
            if (_coyoteCoroutine != null && !_grounded)
            {
                StopCoroutine(_coyoteCoroutine);
                _coyoteCoroutine = null;
                _grounded = true;
                _inCoyoteTime = false;
                _jumpEndEarly = false;
                _canDash = true;
                _jumpCount = 0;
                Grounded?.Invoke(true);
            }
        }
        else
        {
            if (_dashing)
            {
                Grounded?.Invoke(false);
                _grounded = false;
            }
            else if(_coyoteCoroutine == null)
            {
                _coyoteCoroutine = StartCoroutine(CoyoteTime());
                Grounded?.Invoke(false);
            }
        }
    }
    void CheckCeiling()
    {
        if (Physics2D.CircleCast(_col.bounds.center, _col.bounds.size.x / 2, Vector2.up, _col.bounds.size.y / 2 - stats.groundCheckRayOffset, ~stats.playerLayer))
        {
            if (_rb.velocity.y >= 0)
            {
                _currentVelocity.y = Mathf.MoveTowards(_currentVelocity.y, 0, stats.fallAcceleration * Time.deltaTime);
                JumpEnd();
                _jumpEndEarly = true;
                _dashing = false;
            }
        }
    }
    #endregion
}