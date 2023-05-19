using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WeaponController : MonoBehaviour
{
    public GameObject SledgeHammer;

    public bool CanAttack = true;
    public bool IsAttacking = false;

    public float AttackCooldown = 1.0f;

    public AudioClip HammerAttackSound;


    void Update()
    {
        if (IsAttacking)
        {
            if (CanAttack)
            {
                HammerAttack();
            }
        }   
    }

    public void HammerAttack()
    {
        IsAttacking = true;
        CanAttack = false;

        Animator anim=SledgeHammer.GetComponent<Animator>();
        anim.SetTrigger("Attack");

        //AudioSource ac = GetComponent<AudioSource>();
        //ac.PlayOneShot(HammerAttackSound);

        StartCoroutine(ResetAttackCooldown());
    }

    IEnumerator ResetAttackCooldown()
    {
        StartCoroutine(ResetAttackBool());
        yield return new WaitForSeconds(AttackCooldown);
        CanAttack = true;
    }

    IEnumerator ResetAttackBool()
    {
        yield return new WaitForSeconds(1.0f);
        IsAttacking = false;
    }
}
