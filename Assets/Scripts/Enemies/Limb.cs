using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Limb : MonoBehaviour
{
    private Enemy enemyScript;

    [SerializeField] Limb[] childlimbs;

    [SerializeField] GameObject limbPrefab;

    [SerializeField] GameObject bloodPrefab;

    private float fadeDuration = 3f;
    private float destructionDelay = 5f;

    private bool isHit = false;

    // Start is called before the first frame update
    void Start()
    {
        enemyScript = transform.root.GetComponent<Enemy>();
    }


    public void GetHit()
    {
        if (!enemyScript.CanLimbCut() || isHit) return;

        isHit = true;

        if (childlimbs.Length > 0)
        {
            foreach(var limb in childlimbs)
            {
                if (limb != null)
                {
                    limb.GetHit();
                }
            }
        }
        else { DisableMeshRenderer(transform); }


        if (limbPrefab != null)
        {
            Vector3 headPosition = transform.position;
            headPosition.y = 0;
            Quaternion headRotation = transform.rotation;
            float limbRotationX = limbPrefab.transform.rotation.eulerAngles.x;
            float limbRotationZ = -headRotation.eulerAngles.z;
            Quaternion limbRotation = Quaternion.Euler(limbRotationX, headRotation.eulerAngles.y, limbRotationZ);
            GameObject newLimb = Instantiate(limbPrefab, headPosition, limbRotation);

            enemyScript.LimbCutOff();
            StartCoroutine(FadeAndDestroy(newLimb));
        }


        CapsuleCollider collider = GetComponent<CapsuleCollider>();

        if (collider != null)
        {
            collider.enabled = false;
        }


        if (transform.childCount > 0)
        {
            Transform child = transform.GetChild(0);
            SkinnedMeshRenderer skinnedMeshRenderer = child.GetComponent<SkinnedMeshRenderer>();

            if (skinnedMeshRenderer != null)
            {
                skinnedMeshRenderer.enabled = false;
                gameObject.tag = "Untagged";
            }
        }


        enemyScript.GetKilled();
    }

    private IEnumerator FadeAndDestroy(GameObject limb)
    {
        yield return new WaitForSeconds(destructionDelay); // Wait for the destruction delay

        MeshRenderer meshRenderer = limb.GetComponentInChildren<MeshRenderer>();

        Material[] materials = meshRenderer.materials;

        float fadeTimer = 0f;
        float initialAlpha = 1f;

        while (fadeTimer < fadeDuration)
        {
            float alpha = Mathf.Lerp(initialAlpha, 0f, fadeTimer / fadeDuration); // Calculate the alpha value for fading

            // Modify the alpha value of each material in the MeshRenderer
            for (int i = 0; i < materials.Length; i++)
            {
                Color color = materials[i].color;
                color.a = alpha;
                materials[i].color = color;
            }

            fadeTimer += Time.deltaTime;
            yield return null;
        }

        Destroy(limb); // Destroy the limb object
        Destroy(this);
    }

    void DisableMeshRenderer(Transform parent)
    {
        foreach (Transform child in parent)
        {
            if (child.name.StartsWith("Z_"))
            {
                SkinnedMeshRenderer meshRenderer = child.GetComponent<SkinnedMeshRenderer>();
                if (meshRenderer != null)
                {
                    meshRenderer.enabled = false;
                }
            }

            DisableMeshRenderer(child);
        }
    }
}
