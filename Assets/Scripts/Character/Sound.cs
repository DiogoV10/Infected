using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static scr_CharacterController;

public class Sound : MonoBehaviour
{
    public AudioClip shootSound;
    public LayerMask zombieMask;
    private scr_CharacterController scrCC;
    private SphereCollider sphereCollider;

    public float soundIntensity = 5f;
    public float walkEnemyPerceptionRadius = 1f;
    public float sprintEnemyPerceptionRadius = 1.5f;

    private AudioSource audioSource;

    public void Start()
    {
        audioSource = GetComponent<AudioSource>();
        scrCC = GetComponent<scr_CharacterController>();
        sphereCollider = GetComponent<SphereCollider>();
    }

    public void Update()
    {
        if (scrCC.crouched)
        {
            Fire();
        }

        if (scrCC.sprinting)
        {
            sphereCollider.radius = sprintEnemyPerceptionRadius;
        }
        else
        {
            sphereCollider.radius = walkEnemyPerceptionRadius;
        }
    }

    public void Fire()
    {
        audioSource.PlayOneShot(shootSound);
        Collider[] zombies = Physics.OverlapSphere(transform.position, soundIntensity, zombieMask);

        for (int i = 0; i < zombies.Length; i++)
        {
            zombies[i].GetComponent<ZombieAI>().OnAware();
        }
    }

    public void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Enemy"))
        {
            other.GetComponent<ZombieAI>().OnAware();
        }
    }
}
