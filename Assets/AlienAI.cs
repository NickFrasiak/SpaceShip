using UnityEngine;
using UnityEngine.AI;

public class AlienAI : MonoBehaviour
{
    [Header("Patrol Settings")]
    public Transform[] patrolPoints;      // Empty GameObjects as patrol points
    public float patrolSpeed = 2f;

    [Header("Chase Settings")]
    public float detectionRadius = 10f;
    public float fieldOfView = 90f;       // degrees
    public float chaseSpeed = 4f;

    private int currentPatrolIndex = 0;
    private NavMeshAgent agent;
    private Transform player;
    private bool chasingPlayer = false;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
        agent.speed = patrolSpeed;
        GoToNextPatrolPoint();
    }

    void Update()
    {
        if (player == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        // Check if player is visible
        if (CanSeePlayer(distanceToPlayer))
        {
            chasingPlayer = true;
            agent.speed = chaseSpeed;
            agent.SetDestination(player.position);
        }
        else if (chasingPlayer && distanceToPlayer > detectionRadius * 2f)
        {
            // Lost player, return to patrol
            chasingPlayer = false;
            agent.speed = patrolSpeed;
            GoToNextPatrolPoint();
        }

        // Continue patrolling if not chasing
        if (!chasingPlayer && !agent.pathPending && agent.remainingDistance < 0.5f)
        {
            GoToNextPatrolPoint();
        }
    }

    bool CanSeePlayer(float distance)
    {
        if (distance > detectionRadius) return false;

        Vector3 directionToPlayer = (player.position - transform.position).normalized;
        float angle = Vector3.Angle(transform.forward, directionToPlayer);

        if (angle < fieldOfView * 0.5f)
        {
            // Check line of sight
            if (Physics.Raycast(transform.position + Vector3.up, directionToPlayer, out RaycastHit hit, detectionRadius))
            {
                return hit.collider.CompareTag("Player");
            }
        }
        return false;
    }

    void GoToNextPatrolPoint()
    {
        if (patrolPoints.Length == 0) return;

        agent.destination = patrolPoints[currentPatrolIndex].position;
        currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
    }
}
