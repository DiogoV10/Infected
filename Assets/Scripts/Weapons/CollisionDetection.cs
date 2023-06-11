using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionDetection : MonoBehaviour
{
    [SerializeField] public WeaponController wc;
    //[SerializeField] public GameObject HitParticle;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Limb") && wc.IsAttacking)
        {
            //Instantiate(HitParticle, 
            //    new Vector3(other.transform.position.x,transform.position.y,other.transform.position.z), 
            //    other.transform.rotation);
            Limb limb = other.GetComponent<Limb>();
            limb.GetHit();
        }
    }
}
