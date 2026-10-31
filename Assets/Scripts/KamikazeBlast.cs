using System.Collections;
using UnityEngine;

/// <summary>
/// One-shot blast used by the Kamikaze weapon.
/// On start it pushes nearby rigidbodies away, plays every child particle system
/// and fades the child point light in. After <see cref="Duration"/> it stops the
/// particles, fades the light out and finally destroys the whole object.
/// </summary>
[DisallowMultipleComponent]
public class KamikazeBlast : MonoBehaviour
{
    // Layer the blast lives on. Blast particles on this layer never collide with each other.
    private const string BlastLayerName = "FireLayer";

    [Tooltip("Seconds for the light to fade in.")]
    public float FadeInTime = 0.25f;

    [Tooltip("Seconds from start until the blast begins to fade out. Includes the fade-in.")]
    public float Duration = 0.35f;

    [Tooltip("Seconds for the light to fade out after the particles stop emitting.")]
    public float FadeOutTime = 0.5f;

    [Tooltip("Extra seconds after the fade-out before the object is destroyed, so the last particles can finish.")]
    public float CleanupDelay = 2.0f;

    [Tooltip("Explosion force applied to nearby rigidbodies. 0 disables the push.")]
    public float ForceAmount = 2500.0f;

    [Tooltip("Radius of the explosion force.")]
    public float ForceRadius = 10.0f;

    private ParticleSystem[] particleSystems;
    private Light blastLight;
    private float fullLightIntensity;

    private void Awake()
    {
        int blastLayer = LayerMask.NameToLayer(BlastLayerName);
        if (blastLayer >= 0)
        {
            Physics.IgnoreLayerCollision(blastLayer, blastLayer);
        }

        particleSystems = GetComponentsInChildren<ParticleSystem>();

        // The light starts dark and is faded in by PlayBlast.
        blastLight = GetComponentInChildren<Light>();
        if (blastLight != null)
        {
            fullLightIntensity = blastLight.intensity;
            blastLight.intensity = 0.0f;
        }
    }

    private void Start()
    {
        ApplyExplosionForce();

        foreach (ParticleSystem particles in particleSystems)
        {
            particles.Play();
        }

        StartCoroutine(PlayBlast());
    }

    private void ApplyExplosionForce()
    {
        if (ForceAmount <= 0.0f || ForceRadius <= 0.0f)
        {
            return;
        }

        Vector3 center = transform.position;
        foreach (Collider hit in Physics.OverlapSphere(center, ForceRadius))
        {
            Rigidbody body = hit.attachedRigidbody;
            if (body != null)
            {
                body.AddExplosionForce(ForceAmount, center, ForceRadius);
            }
        }
    }

    private IEnumerator PlayBlast()
    {
        // Full strength: fade the light in, then hold until the duration is over.
        for (float elapsed = 0.0f; elapsed < Duration; elapsed += Time.deltaTime)
        {
            float fadeIn = FadeInTime > 0.0f ? Mathf.Clamp01(elapsed / FadeInTime) : 1.0f;
            SetLightIntensity(fullLightIntensity * fadeIn);
            yield return null;
        }

        foreach (ParticleSystem particles in particleSystems)
        {
            particles.Stop();
        }

        // Fade out from wherever the light ended up.
        float startIntensity = blastLight != null ? blastLight.intensity : 0.0f;
        for (float elapsed = 0.0f; elapsed < FadeOutTime; elapsed += Time.deltaTime)
        {
            SetLightIntensity(Mathf.Lerp(startIntensity, 0.0f, elapsed / FadeOutTime));
            yield return null;
        }
        SetLightIntensity(0.0f);

        yield return new WaitForSeconds(CleanupDelay);
        Destroy(gameObject);
    }

    private void SetLightIntensity(float intensity)
    {
        if (blastLight != null)
        {
            blastLight.intensity = intensity;
        }
    }
}
