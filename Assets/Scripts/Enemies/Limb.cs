using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Limb : MonoBehaviour
{
    Enemy enemyScript;

    [SerializeField] Limb[] childlimbs;

    [SerializeField] GameObject limbPrefab;
    [SerializeField] GameObject woundHole;

    [SerializeField] GameObject bloodPrefab;

    // Start is called before the first frame update
    void Start()
    {
        enemyScript = transform.root.GetComponent<Enemy>();

        if (woundHole != null)
        {
            woundHole.SetActive(false);
        }
    }


    public void GetHit()
    {
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
