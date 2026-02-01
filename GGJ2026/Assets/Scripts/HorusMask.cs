using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;

public class HorusMask : MonoBehaviour
{
    [Tooltip("How long the mask buff lasts once picked up.")]
    public float buffDuration = 5f;
    [Tooltip("Radius in world units within which breakable blocks will be affected.")]
    public float effectRadius = 3f;
    [Tooltip("Alpha value to apply to affected breakable sprites (0 = fully transparent, 1 = opaque).")]
    [Range(0f, 1f)]
    public float targetAlpha = 0.5f;

    [Tooltip("Radius of hover around the player.")]
    public float hoverRadius = 1f;
    [Tooltip("Speed of hover motion.")]
    public float hoverSpeed = 2f;

    [Tooltip("Multiplier applied to the virtual camera lens while the buff is active.")]
    public float cameraZoomFactor = 1.25f;
    [Tooltip("How fast the virtual camera lens transitions to/from the zoom.")]
    public float cameraZoomLerpSpeed = 6f;

    // Hover / pickup state
    private Transform targetPlayer;
    private float timer;
    private bool pickedUp;
    private bool isActive;

    // Cinemachine virtual camera handling (use Component + reflection to avoid hard dependency)
    private Component vcam;
    private FieldInfo mLensField;
    private FieldInfo orthoLensField;
    private FieldInfo fovLensField;
    private object originalLensValue;
    private bool usingCinemachine;

    // Fallback: modify Unity Camera if Cinemachine internals are unavailable
    private Camera unityCamera;
    private float originalCameraValue;
    private bool usingUnityCamera;
    private bool cameraIsOrtho;

    // Track sprite renderers we've modified and their original colors so we can restore them
    private readonly Dictionary<SpriteRenderer, Color> modifiedRenderers = new Dictionary<SpriteRenderer, Color>();

    private void Update()
    {
        // If attached to a player, hover around them
        if (targetPlayer != null)
        {
            timer += Time.deltaTime;
            float angle = timer * hoverSpeed * Mathf.PI * 2f;
            Vector3 offset = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0) * hoverRadius;
            transform.position = targetPlayer.position + offset;
        }

        // While active, update affected breakable objects each frame
        if (isActive)
        {
            UpdateAffectedRenderers();
        }

        // If we're using a fallback Unity Camera, smoothly lerp its size/FOV
        if (usingUnityCamera && unityCamera != null)
        {
            if (cameraIsOrtho)
            {
                float current = unityCamera.orthographicSize;
                float original = originalCameraValue;
                float target = isActive ? original * cameraZoomFactor : original;
                float next = Mathf.Lerp(current, target, Time.deltaTime * cameraZoomLerpSpeed);
                unityCamera.orthographicSize = next;
            }
            else
            {
                float current = unityCamera.fieldOfView;
                float original = originalCameraValue;
                float target = isActive ? original * cameraZoomFactor : original;
                float next = Mathf.Lerp(current, target, Time.deltaTime * cameraZoomLerpSpeed);
                unityCamera.fieldOfView = next;
            }
        }

        // Smoothly modify the virtual camera lens only if a Cinemachine vcam was acquired
        if (usingCinemachine && vcam != null && mLensField != null && originalLensValue != null)
        {
            var lens = mLensField.GetValue(vcam);
            if (lens != null)
            {
                if (orthoLensField != null)
                {
                    float current = (float)orthoLensField.GetValue(lens);
                    float original = (float)orthoLensField.GetValue(originalLensValue);
                    float target = isActive ? original * cameraZoomFactor : original;
                    float next = Mathf.Lerp(current, target, Time.deltaTime * cameraZoomLerpSpeed);
                    orthoLensField.SetValue(lens, next);
                    mLensField.SetValue(vcam, lens);
                }
                else if (fovLensField != null)
                {
                    float current = (float)fovLensField.GetValue(lens);
                    float original = (float)fovLensField.GetValue(originalLensValue);
                    float target = isActive ? original * cameraZoomFactor : original;
                    float next = Mathf.Lerp(current, target, Time.deltaTime * cameraZoomLerpSpeed);
                    fovLensField.SetValue(lens, next);
                    mLensField.SetValue(vcam, lens);
                }
            }
        }
    }

    private void UpdateAffectedRenderers()
    {
        Vector2 center = targetPlayer != null ? (Vector2)targetPlayer.position : (Vector2)transform.position;
        var colliders = Physics2D.OverlapCircleAll(center, effectRadius);
        var currentlyFound = new HashSet<SpriteRenderer>();

        foreach (var col in colliders)
        {
            if (col == null) continue;
            var breakable = col.GetComponentInParent<BreakableObject>();
            if (breakable == null) continue;

            var sr = breakable.GetComponent<SpriteRenderer>();
            if (sr == null) sr = breakable.GetComponentInChildren<SpriteRenderer>();
            if (sr == null) continue;

            currentlyFound.Add(sr);

            if (!modifiedRenderers.ContainsKey(sr))
            {
                modifiedRenderers[sr] = sr.color;
                var c = sr.color;
                c.a = targetAlpha;
                sr.color = c;
            }
            else
            {
                var c = sr.color;
                if (!Mathf.Approximately(c.a, targetAlpha))
                {
                    c.a = targetAlpha;
                    sr.color = c;
                }
            }
        }

        var toRestore = new List<SpriteRenderer>();
        foreach (var kv in modifiedRenderers)
        {
            if (!currentlyFound.Contains(kv.Key))
            {
                toRestore.Add(kv.Key);
            }
        }

        foreach (var sr in toRestore)
        {
            if (sr != null && modifiedRenderers.TryGetValue(sr, out var original))
            {
                sr.color = original;
            }
            modifiedRenderers.Remove(sr);
        }
    }

    private void RestoreAll()
    {
        foreach (var kv in modifiedRenderers)
        {
            var sr = kv.Key;
            if (sr != null)
            {
                sr.color = kv.Value;
            }
        }
        modifiedRenderers.Clear();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // If not yet picked up: allow pickup by the first player that touches it
        if (!pickedUp && collision.CompareTag("Player"))
        {
            var playerController = collision.GetComponent<MovementController2D>();
            if (playerController != null)
            {
                pickedUp = true;
                targetPlayer = collision.transform;
                transform.SetParent(targetPlayer);
                timer = 0f;

                isActive = true;
                StartCoroutine(BuffLifetime());

                AcquireVirtualCameraOnly();

                // Keep collider enabled so the mask can affect blocks while hovering
                return;
            }
        }
    }

    private void AcquireVirtualCameraOnly()
    {
        if (targetPlayer == null) return;

        Component found = null;
        Type vcamType = null;

        // Try to find Cinemachine type in loaded assemblies (robust across package versions)
        foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
        {
            try
            {
                // common full type name
                vcamType = asm.GetType("Cinemachine.CinemachineVirtualCamera") ?? asm.GetType("Cinemachine.CinemachineVirtualCameraBase");
                if (vcamType != null) break;

                // fallback: any type in a Cinemachine namespace with "VirtualCamera" in the name
                foreach (var t in asm.GetTypes())
                {
                    if (t.Namespace != null && t.Namespace.IndexOf("Cinemachine", StringComparison.OrdinalIgnoreCase) >= 0
                        && t.Name.IndexOf("VirtualCamera", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        vcamType = t;
                        break;
                    }
                }
                if (vcamType != null) break;
            }
            catch { /* ignore reflection load errors */ }
        }

        // If we found the runtime type, use GetComponentInChildren(Type)
        if (vcamType != null)
        {
            found = targetPlayer.GetComponentInChildren(vcamType, true);
            if (found == null)
            {
                foreach (var root in SceneManager.GetActiveScene().GetRootGameObjects())
                {
                    found = root.GetComponentInChildren(vcamType, true);
                    if (found != null) break;
                }
            }
        }

        // Last-resort: search components by name (handles edge-cases)
        if (found == null)
        {
            var comps = targetPlayer.GetComponentsInChildren<Component>(true);
            foreach (var comp in comps)
            {
                if (comp == null) continue;
                var name = comp.GetType().Name;
                if (name.IndexOf("Cinemachine", StringComparison.OrdinalIgnoreCase) >= 0
                    || name.IndexOf("VirtualCamera", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    found = comp;
                    break;
                }
            }
        }

        if (found == null)
        {
            Debug.LogWarning("HorusMask: No Cinemachine virtual camera component found in player or scene; virtual camera lens will not be modified.");
            usingCinemachine = false;

            // Fallback to modifying the main Unity Camera if available
            unityCamera = Camera.main;
            if (unityCamera != null)
            {
                usingUnityCamera = true;
                cameraIsOrtho = unityCamera.orthographic;
                originalCameraValue = cameraIsOrtho ? unityCamera.orthographicSize : unityCamera.fieldOfView;
            }
            return;
        }

        vcam = found;
        usingCinemachine = true;

        // reflectively capture lens struct and its fields using the actual runtime type
        var vcamTypeRuntime = vcam.GetType();
        mLensField = vcamTypeRuntime.GetField("m_Lens", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

        // also try a public property named "Lens"
        if (mLensField == null)
        {
            var prop = vcamTypeRuntime.GetProperty("Lens", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (prop != null)
            {
                // create a synthetic FieldInfo-like wrapper using reflection via a small delegate approach:
                // store the property value into originalLensValue and then operate on that object using field lookups below
                originalLensValue = prop.GetValue(vcam);
                if (originalLensValue != null)
                {
                    var lensType = originalLensValue.GetType();
                    orthoLensField = lensType.GetField("OrthographicSize", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                    fovLensField = lensType.GetField("FieldOfView", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                }
            }
        }
        else
        {
            originalLensValue = mLensField.GetValue(vcam);
            if (originalLensValue != null)
            {
                var lensType = originalLensValue.GetType();
                orthoLensField = lensType.GetField("OrthographicSize", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                fovLensField = lensType.GetField("FieldOfView", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            }
        }

        // If we still couldn't locate lens fields, fallback to Unity Camera
        if ((mLensField == null && originalLensValue == null) || (orthoLensField == null && fovLensField == null))
        {
            Debug.LogWarning("HorusMask: Cinemachine lens fields not found; lens modification unavailable. Falling back to main Camera.");
            usingCinemachine = false;
            vcam = null;
            originalLensValue = null;

            unityCamera = Camera.main;
            if (unityCamera != null)
            {
                usingUnityCamera = true;
                cameraIsOrtho = unityCamera.orthographic;
                originalCameraValue = cameraIsOrtho ? unityCamera.orthographicSize : unityCamera.fieldOfView;
            }
        }
    }

    private IEnumerator BuffLifetime()
    {
        yield return new WaitForSeconds(buffDuration);
        isActive = false;

        RestoreAll();
        RestoreVirtualCameraImmediate();

        EndBuff();
    }

    private void RestoreVirtualCameraImmediate()
    {
        if (usingCinemachine && vcam != null && mLensField != null && originalLensValue != null)
        {
            mLensField.SetValue(vcam, originalLensValue);
            vcam = null;
            usingCinemachine = false;
            originalLensValue = null;
        }

        if (usingUnityCamera && unityCamera != null)
        {
            if (cameraIsOrtho)
                unityCamera.orthographicSize = originalCameraValue;
            else
                unityCamera.fieldOfView = originalCameraValue;

            unityCamera = null;
            usingUnityCamera = false;
        }
    }

    public void EndBuff()
    {
        Destroy(gameObject);
    }

    // Optional editor visualization
    private void OnDrawGizmosSelected()
    {
        Vector3 center = (targetPlayer != null) ? targetPlayer.position : transform.position;
        Gizmos.color = new Color(1f, 1f, 0f, 0.35f);
        Gizmos.DrawSphere(center, effectRadius);
    }
}
