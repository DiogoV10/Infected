using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sound : MonoBehaviour
{
    [SerializeField] public AudioClip shootSound;
    [SerializeField] public LayerMask zombieMask;
    private scr_CharacterController scrCC;
    private WeaponController weaponController;
    private SphereCollider sphereCollider;
    private DefaultInput defaultInput;

    [SerializeField] public float soundIntensity = 5f;
    [SerializeField] public float walkEnemyPerceptionRadius = 1f;
    [SerializeField] public float sprintEnemyPerceptionRadius = 1.5f;

    private AudioSource audioSource;

    public void Initialize(DefaultInput input)
    {
        defaultInput = input;

        //defaultInput.Character.Attack.performed += e => Attack();
    }

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        scrCC = GetComponent<scr_CharacterController>();
        sphereCollider = GetComponent<SphereCollider>();
        weaponController = GetComponentInChildren<WeaponController>();
    }

    private void Update()
    {
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

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Enemy"))
        {
            other.GetComponent<ZombieAI>().OnAware();
        }
    }
}
