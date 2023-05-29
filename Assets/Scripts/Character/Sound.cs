using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static scr_CharacterController;

public class Sound : MonoBehaviour
{
    [SerializeField] public AudioClip shootSound;
    [SerializeField] public LayerMask zombieMask;
    private scr_CharacterController scrCC;
    private SphereCollider sphereCollider;

    [SerializeField] public float soundIntensity = 5f;
    [SerializeField] public float walkEnemyPerceptionRadius = 1f;
    [SerializeField] public float sprintEnemyPerceptionRadius = 1.5f;

    private AudioSource audioSource;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        scrCC = GetComponent<scr_CharacterController>();
        sphereCollider = GetComponent<SphereCollider>();
    }

    private void Update()
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

    private void Fire()
    {
        audioSource.PlayOneShot(shootSound);
        Collider[] zombies = Physics.OverlapSphere(transform.position, soundIntensity, zombieMask);

        for (int i = 0; i < zombies.Length; i++)
        {
            zombies[i].GetComponent<ZombieAI>().OnAware();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Enemy"))
        {
            other.GetComponent<ZombieAI>().OnAware();
        }
    }
}
