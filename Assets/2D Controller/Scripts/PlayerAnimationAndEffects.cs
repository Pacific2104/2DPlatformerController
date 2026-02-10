using System.Collections;
using UnityEngine;

public class PlayerAnimationAndEffects : MonoBehaviour
{
    [SerializeField] SpriteRenderer _sprite;
    [SerializeField] Transform _camTarget;
    [SerializeField, Tooltip("How much time to wait before the camera target start switching")]
    float _camTargetSwitchTime;
    [SerializeField, Tooltip("Time it takes for the camera target to lerp")]
    public float _camTargetSmoothTime;
    [SerializeField] Animator _animator;
    [SerializeField] CameraShakeStats dashShake;

    public string speedkey;
    public string jumpkey;
    public string groundedkey;

    public SpriteRenderer _staminaBar;

    float _staminaBarWidth;
    float _staminaRefillTime;

    bool _grounded;
    float _goingBackTime;
    Vector3 _camTargetPos;
    Coroutine _cameraLerpRoutine;

    IPlayerInterface _player;

    private void Awake()
    {
        if (_sprite == null)
        {
            if (!TryGetComponent<SpriteRenderer>(out _sprite))
            {
                Debug.LogError("Sprite renderer not found");
            }
        }
        if (_animator == null)
        {
            if (!TryGetComponent<Animator>(out _animator))
            {
                Debug.LogError("Animator not found");
            }
        }
        _player = GetComponentInParent<IPlayerInterface>();
        if (_player == null)
        {
            Debug.LogError("Player script not found in parent");
        }
        _camTargetPos = _camTarget.localPosition;
    }
    private void Start()
    {
        _staminaBarWidth = _staminaBar.transform.localScale.x;
        _staminaRefillTime = GetComponentInParent<PlayerController>()._stats.dashBuffer;
    }
    private void OnEnable()
    {
        if (_player != null)
        {
            _player.Jumped += Jumped;
            _player.Grounded += GroundedChanged;
            _player.Dashed += Dashed;
            _player.HardFall += HardGrounded;
            _player.WallSmash += SmashedInWall;
        }
    }
    private void OnDisable()
    {
        if (_player != null)
        {
            _player.Jumped -= Jumped;
            _player.Grounded -= GroundedChanged;
            _player.Dashed -= Dashed;
            _player.HardFall -= HardGrounded;
            _player.WallSmash -= SmashedInWall;
        }
    }
    private void LateUpdate()
    {
        if (_player == null) return;

        HandleSpriteFlip();
        HandleMoveAnimation();
    }
    void HandleMoveAnimation()
    {
        float absVelocity = Mathf.Abs(_player.InputDirection.x);
        _animator.SetFloat(speedkey, Mathf.Lerp(0, 1, absVelocity));
    }
    void HandleSpriteFlip()
    {
        if (_player.InputDirection.x != 0)
        {
            _sprite.flipX = _player.InputDirection.x < 0;
            HandleCameraTarget();
        }
    }
    void HandleCameraTarget()
    {
        _goingBackTime += Mathf.Sign(_player.InputDirection.x) * Time.deltaTime;
        _goingBackTime = Mathf.Clamp(_goingBackTime, -_camTargetSwitchTime, _camTargetSwitchTime);
        if (Mathf.Abs(_goingBackTime) >= _camTargetSwitchTime)
        {
            _cameraLerpRoutine ??= StartCoroutine(LerpCameraTarget());
        }
    }
    IEnumerator LerpCameraTarget()
    {
        Vector3 _ref = Vector3.zero;
        Vector3 targetPos = _camTargetPos * Mathf.Sign(_player.InputDirection.x);
        if (targetPos != _camTarget.localPosition)
        {
            float t = _camTargetSmoothTime;
            while (targetPos != _camTarget.localPosition)
            {
                Vector3 tempPos = Vector3.SmoothDamp(_camTarget.localPosition, targetPos, ref _ref, t);
                _camTarget.localPosition = tempPos;
                t -= Time.deltaTime;
                yield return null;
            }
            _camTarget.localPosition = targetPos;
        }
        _cameraLerpRoutine = null;
    }
    void Jumped()
    {
        SoundManager.instance.PlaySfx(SFX.Jump);
        _animator.SetTrigger(jumpkey);
        _animator.ResetTrigger(groundedkey);
    }
    void GroundedChanged(bool grounded)
    {
        _grounded = grounded;
        if(grounded)
            _animator.SetTrigger(groundedkey);
    }
    void HardGrounded()
    {
        CameraController.ShakeDirectional(Vector2.down);
    }
    void SmashedInWall()
    {
        CameraController.ShakeDirectional(Vector2.right);
    }
    void Dashed()
    {
        SoundManager.instance.PlaySfx(SFX.Dash);
        CameraController.ShakeDirectional(_player.PlayerVelocity, dashShake);
        StartCoroutine(RefillStamina());
    }
    IEnumerator RefillStamina()
    {
        _staminaBar.transform.localScale = new Vector3(0, _staminaBar.transform.localScale.y, _staminaBar.transform.localScale.z);
        _staminaBar.color = Color.white;
        yield return null;

        Vector3 initialScale = _staminaBar.transform.localScale;
        Vector3 targetScale = new Vector3(_staminaBarWidth, initialScale.y, initialScale.z);
        float elapsedTime = 0f;

        while (elapsedTime < _staminaRefillTime)
        {
            float t = elapsedTime / _staminaRefillTime;
            // Use Mathf.Lerp to interpolate between start and end values
            float newXScale = Mathf.Lerp(0, _staminaBarWidth, t);
            _staminaBar.transform.localScale = new Vector3(newXScale, initialScale.y, initialScale.z);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Ensure final scale is set
        _staminaBar.transform.localScale = targetScale;


        // Fade out the sprite
        Color initialColor = _staminaBar.color;
        Color targetColor = new Color(initialColor.r, initialColor.g, initialColor.b, 0f);
        elapsedTime = 0f;

        while (elapsedTime < 0.1f)
        {
            float t = elapsedTime / .1f;
            _staminaBar.color = Color.Lerp(initialColor, targetColor, t);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Ensure final color is set
        _staminaBar.color = targetColor;
    }
}