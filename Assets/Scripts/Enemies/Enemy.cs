using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    private Animator anim;
    private CapsuleCollider capsuleCollider;

    private List<Rigidbody> ragdollRigids;

    [SerializeField] private Material deathMaterial;

    [SerializeField] private int health = 100;

    private float fadeDuration = 3f;
    private float destructionDelay = 5f;
    private bool limbCanCut = true;
    private bool limbCutOff = false;
    private bool isDead = false;
    private float lastLimbCutOffTime = 0f;

    // Start is called before the first frame update
    void Start()
    {
        anim = GetComponent<Animator>();
        capsuleCollider = GetComponent<CapsuleCollider>();

        ragdollRigids = new List<Rigidbody>(transform.GetComponentsInChildren<Rigidbody>());
        ragdollRigids.Remove(GetComponent<Rigidbody>());

        DeactivateRagdoll();
    }

    // Update is called once per frame
    void Update()
    {
        if (health <= 0) GetKilled();

        if (!isDead) return;

        if (!CanStartCoroutine()) return;

        if (!limbCanCut) return;

        StartCoroutine(FadeAndDestroy());
    }

    void ActivateRagdoll()
    {
        anim.enabled = false;
        capsuleCollider.enabled = false;
        for(int i = 0; i < ragdollRigids.Count; i++)
        {
            ragdollRigids[i].useGravity = true;
            ragdollRigids[i].isKinematic = false;
        }
    }

    void DeactivateRagdoll()
    {
        anim.enabled = true;
        capsuleCollider.enabled = true;
        for (int i = 0; i < ragdollRigids.Count; i++)
        {
            ragdollRigids[i].useGravity = false;
            ragdollRigids[i].isKinematic = true;
        }
    }

    public void GetKilled()
    {
        ActivateRagdoll();

        if (GameObject.FindGameObjectWithTag("WaveSpawner") != null)
        {
            GameObject.FindGameObjectWithTag("WaveSpawner").GetComponent<WaveSpawner>().spawnedEnemies.Remove(gameObject);
        }

        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in renderers)
        {
            renderer.material = deathMaterial;
        }

        isDead = true;
    }

    // Method to call when a limb is cut off
    public void LimbCutOff()
    {
        limbCutOff = true;
    }

    public bool CanLimbCut()
    {
        return limbCanCut;
    }

    private bool CanStartCoroutine()
    {
        Limb[] limbs = GetComponentsInChildren<Limb>();
        bool hasLimbs = limbs != null && limbs.Length > 0;

        if (limbCutOff)
        {
            lastLimbCutOffTime = Time.time;
            limbCutOff = false;
            return false;
        }

        if (!hasLimbs || Time.time - lastLimbCutOffTime >= destructionDelay)
        {
            return true;
        }

        return false;
    }

    private IEnumerator FadeAndDestroy()
    {
        limbCanCut = false;

        yield return new WaitForSeconds(destructionDelay);

        Renderer[] renderers = GetComponentsInChildren<Renderer>();

        Material[] materials;

        // Fade out the enemy
        float fadeTimer = 0f;
        float initialAlpha = 1f;

        while (fadeTimer < fadeDuration)
        {
            float alpha = Mathf.Lerp(initialAlpha, 0f, fadeTimer / fadeDuration);

            foreach (Renderer renderer in renderers)
            {
                if (renderer.enabled)
                {
                    materials = renderer.materials;

                    for (int i = 0; i < materials.Length; i++)
                    {
                        Color color = materials[i].color;
                        color.a = alpha;
                        materials[i].color = color;
                    }
                }
            }

            fadeTimer += Time.deltaTime;
            yield return null;
        }

        Destroy(gameObject);
    }
}
