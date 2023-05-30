using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using static scr_CharacterController;

public class ZombieAI : MonoBehaviour
{
    private NavMeshAgent agent;
    private Animator anim;

    private bool isAware = false;
    private bool isDetecting = false;

    private float loseTimer = 0f;
    private int waypointIndex = 0;

    private Vector3 wanderPoint;


    [Header("References")]
    [SerializeField] public scr_CharacterController scrCC;

    [SerializeField] public enum WanderType { Random, Waypoint };
    [Header("Wander")]
    [SerializeField] public WanderType wanderType = WanderType.Random;
    [SerializeField] public Transform[] waypoints;

    [Header("Settings")]
    [SerializeField] public float wanderSpeed = 1.25f;
    [SerializeField] public float chaseSpeed = 3f;
    [SerializeField] public float fov = 120f;
    [SerializeField] public float viewDistance = 10f;
    [SerializeField] public float wanderRadius = 5f;
    [SerializeField] public float loseThreshold = 10f;

    // Start is called before the first frame update
    public void Start()
    {
        anim = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();

        wanderPoint = RandomWanderPoint();
    }

    // Update is called once per frame
    public void Update()
    {
        if (anim == null || !anim.isActiveAndEnabled) 
        {
            agent.speed = 0;
            return;
        }

        if (isAware)
        {
            agent.SetDestination(scrCC.transform.position);
            anim.SetBool("Aware", true);
            agent.speed = chaseSpeed;

            if (!isDetecting)
            {
                loseTimer += Time.deltaTime;

                if (loseTimer >= loseThreshold)
                {
                    isAware = false;
                    loseTimer = 0;
                }
            }
        }
        else
        {
            Wander();
            anim.SetBool("Aware", false);
            agent.speed = wanderSpeed;
        }

        anim.SetBool("Walking", true);

        SearchForPlayer();
    }

    public void SearchForPlayer()
    {
        if (Vector3.Angle(Vector3.forward, transform.InverseTransformPoint(scrCC.transform.position)) < fov / 2f)
        {
            if (Vector3.Distance(scrCC.transform.position, transform.position) < viewDistance)
            {
                RaycastHit hit;

                if (Physics.Linecast(transform.position, scrCC.transform.position, out hit, -1))
                {
                    if (hit.transform.CompareTag("Player"))
                    {
                        OnAware();
                    }
                    else
                    {
                        isDetecting = false;
                    }
                }
                else
                {
                    isDetecting = false;
                }
            }
            else
            {
                isDetecting = false;
            }
        }
        else
        {
            isDetecting = false;
        }
    }

    public void OnAware()
    {
        isAware = true;
        isDetecting = true;
        loseTimer = 0;
    }

    public void Wander()
    {
        if (wanderType == WanderType.Random)
        {
            if (agent.remainingDistance < 0.5f || !agent.hasPath)
            {
                wanderPoint = RandomWanderPoint();
                agent.SetDestination(wanderPoint);
            }
        }
        else
        {
            if (waypoints.Length >= 2)
            {
                if (Vector3.Distance(waypoints[waypointIndex].position, transform.position) < 2f)
                {
                    if (waypointIndex == waypoints.Length - 1)
                    {
                        waypointIndex = 0;
                    }
                    else
                    {
                        waypointIndex++;
                    }
                }
                else
                {
                    agent.SetDestination(waypoints[waypointIndex].position);
                }
            }
            else
            {
                Debug.LogWarning("Please assign more than 1 waypoint to tge AI: " + gameObject.name);
            }
        }

        
    }

    public Vector3 RandomWanderPoint()
    {
        Vector3 randomPoint = (Random.insideUnitSphere * wanderRadius) + transform.position;

        NavMeshHit navHit;

        NavMesh.SamplePosition(randomPoint, out navHit, wanderRadius, -1);

        return new Vector3(navHit.position.x, transform.position.y, navHit.position.z);
    }
}
