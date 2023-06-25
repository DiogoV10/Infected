using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class ZombieAI : MonoBehaviour
{
    private NavMeshAgent agent;
    private Animator anim;
    private scr_CharacterController scrCC;
    private PlayerStats playerStats;

    private bool isAware = false;
    private bool isAlwaysAware = false;
    private bool isDetecting = false;

    [Range(0f, 1f)] private float awareChance = 0.5f;
    [Range(0f, 1f)] private float alwaysAwareChance = 0.90f;

    private float loseTimer = 0f;
    private int waypointIndex = 0;

    private Vector3 wanderPoint;

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

    [Header("Attack")]
    [SerializeField] public float attackRange = 1.5f;
    [SerializeField] public float attackCooldown = 2f;
    private bool isAttacking = false;
    private float attackTimer = 0f;
    private bool isDelayingAttack = false;
    private bool hasHitPlayer = false;

    // Start is called before the first frame update
    public void Start()
    {
        scrCC = GameObject.Find("Player").GetComponent<scr_CharacterController>();

        playerStats = GameObject.Find("Player").GetComponent<PlayerStats>();

        anim = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();

        wanderPoint = RandomWanderPoint();

        if (Random.value <= alwaysAwareChance)
        {
            isAlwaysAware = true;
            OnAware();
        }else if (Random.value <= awareChance)
        {
            OnAware();
        }
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

            if (Vector3.Distance(transform.position, scrCC.transform.position) <= attackRange && !isAttacking)
            {
                StartAttackDelay();
            }
            else if(Vector3.Distance(transform.position, scrCC.transform.position) > attackRange && isAttacking)
            {
                isAttacking = false;
            }
        }
        else
        {
            Wander();
            anim.SetBool("Aware", false);
            agent.speed = wanderSpeed;
        }

        anim.SetBool("Walking", true);

        //FindObjectOfType<AudioManager>().PlaySound("ZombieChase");

        if (!isAlwaysAware)
        {
            SearchForPlayer();
        }

        // Update attack cooldown
        if (isAttacking)
        {
            attackTimer += Time.deltaTime;
            if (attackTimer >= attackCooldown)
            {
                isAttacking = false;
                attackTimer = 0f;
            }
        }
    }

    public void StartAttackDelay()
    {
        if (!isDelayingAttack)
        {
            float attackDelay = Random.Range(0f, .5f); // Adjust the range as needed
            StartCoroutine(AttackDelayCoroutine(attackDelay));
        }
    }

    private IEnumerator AttackDelayCoroutine(float delay)
    {
        isDelayingAttack = true;
        yield return new WaitForSeconds(delay);
        AttackPlayer();
        isDelayingAttack = false;
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
                CreateRandomWaypoints();
            }
        }
    }

    public void CreateRandomWaypoints()
    {
        int numWaypoints = Random.Range(2, 11);

        Transform[] newWaypoints = new Transform[numWaypoints];

        for (int i = 0; i < waypoints.Length; i++)
        {
            newWaypoints[i] = waypoints[i];
        }

        for (int i = waypoints.Length; i < numWaypoints; i++)
        {
            Vector3 randomPoint = RandomWanderPoint();
            GameObject waypointObj = new GameObject("Waypoint");
            waypointObj.transform.position = randomPoint;
            newWaypoints[i] = waypointObj.transform;
        }

        waypoints = newWaypoints;
        waypointIndex = 0;
    }

    public Vector3 RandomWanderPoint()
    {
        Vector3 randomPoint = (Random.insideUnitSphere * wanderRadius) + transform.position;

        NavMeshHit navHit;

        NavMesh.SamplePosition(randomPoint, out navHit, wanderRadius, -1);

        return new Vector3(navHit.position.x, transform.position.y, navHit.position.z);
    }

    public void AttackPlayer()
    {
        if (!isAttacking)
        {
            // Trigger attack animation
            anim.SetTrigger("Attack");
            isAttacking = true;
        }
    }

    public void ResetAttack()
    {
        hasHitPlayer = false;
    }

    void OnTriggerEnter(Collider other)
    {
        if (isAttacking && other.CompareTag("Player") && !hasHitPlayer)
        {
            // Perform actions when the zombie hits the player
            // For example, you can decrease the player's health or trigger a game over condition.
            Debug.Log("Zombie hit the player!");

            playerStats.Health();

            FindObjectOfType<AudioManager>().PlaySound("ZombieAttack");
            hasHitPlayer = true;
        }
    }
}
