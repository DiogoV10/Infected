using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    private Animator anim;
    private CapsuleCollider capsuleCollider;

    List<Rigidbody> ragdollRigids;

    [SerializeField] public int health = 100;

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
    }
}
