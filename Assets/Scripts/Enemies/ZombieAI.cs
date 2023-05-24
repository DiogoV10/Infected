using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZombieAI : MonoBehaviour
{
    public GameObject Target;
    public Animator anim;

    public float speed = 1f;

    // Start is called before the first frame update
    void Start()
    {
        anim = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if (anim != null && anim.isActiveAndEnabled)
        {
            transform.LookAt(Target.gameObject.transform.position);
            transform.Translate(Vector3.forward * Time.deltaTime * speed);
            anim.SetBool("Walking", true);
        }
    }
}
