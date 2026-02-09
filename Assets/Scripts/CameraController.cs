using NaughtyAttributes;
using System.Collections;
using UnityEngine;

[System.Serializable]
public struct CameraShakeStats
{
    public float _magnitude;
    public float _duration;
}
public class CameraController : MonoBehaviour
{
    [Header("Follow Settings")]
    [Tooltip("Object used to follow the target smoothly.")]
    [SerializeField] Transform m_follower;

    [Tooltip("Target to follow during gameplay.")]
    [SerializeField] Transform m_target;

    [Tooltip("Speed of camera smoothing (Lerp).")]
    [SerializeField] float m_lerpSpeed = 5f;

    [Tooltip("Horizontal camera limits (X axis).")]
    [SerializeField] Vector2 m_horizontalClamp;

    [Tooltip("Vertical camera limits (Z axis).")]
    [SerializeField] Vector2 m_vertcialClamp;

    private Vector3 m_targetPos, m_desiredPos;

    [Header("Shake Settings")]
    [SerializeField] Transform m_shaker;
    [SerializeField] CameraShakeStats m_defaultStats;

    private Vector3 m_shakerOriginalPos;
    private Coroutine m_shakeRoutine;
    private bool m_isShaking;

    private static CameraController instance;
    private void Awake()
    {
        instance = this;
        Vector3 startPos = m_target.position;
        m_follower.position = new Vector3(startPos.x, startPos.y, -10f);
    }
    private void FixedUpdate()
    {
        if (m_target == null || m_isShaking) return;
        m_targetPos = m_target.position;

        // Clamp X and Z
        float clampedX = Mathf.Clamp(m_targetPos.x, m_horizontalClamp.x, m_horizontalClamp.y);
        float clampedY = Mathf.Clamp(m_targetPos.y, m_vertcialClamp.x, m_vertcialClamp.y);
        m_desiredPos = new Vector3(clampedX, clampedY, m_follower.position.z);

        // Smooth Lerp movement
        m_follower.position = Vector3.Lerp(m_follower.position, m_desiredPos, Time.fixedDeltaTime * m_lerpSpeed);
    }

    [Button]
    public static void Shake()
    {
        ShakeDirectional(Vector2.one, instance.m_defaultStats);
    }
    public static void Shake(CameraShakeStats stats)
    {
        ShakeDirectional(Vector2.one, stats);
    }
    public static void ShakeDirectional(Vector2 direction)
    {
        ShakeDirectional(direction, instance.m_defaultStats);
    }
    public static void ShakeDirectional(Vector2 direction, CameraShakeStats stats)
    {
        Vector2 dir = direction + Random.insideUnitCircle / 10;
        instance.StartShake(dir, stats);
    }
    void StartShake(Vector2 direction, CameraShakeStats stats)
    {
        if (m_shakeRoutine != null)
        {
            StopCoroutine(m_shakeRoutine);
            m_shakeRoutine = null;
        }
        m_shakeRoutine = StartCoroutine(ShakeCoroutine(direction, stats));
    }
    IEnumerator ShakeCoroutine(Vector2 direction, CameraShakeStats stats)
    {
        m_isShaking = true;
        m_shakerOriginalPos = m_shaker.localPosition;
        float elapsed = 0.0f;
        direction += Random.insideUnitCircle * 0.1f;
        while (elapsed < stats._duration)
        {
            Vector2 shake = stats._magnitude * direction * Random.insideUnitCircle;

            m_shaker.transform.localPosition = new Vector3(shake.x, shake.y, m_shakerOriginalPos.z);

            elapsed += Time.deltaTime;

            yield return null;
        }
        m_isShaking = false;
        m_shaker.transform.localPosition = m_shakerOriginalPos;
    }
}
