using UnityEngine;
using UnityEngine.AI;

public class GoToTarget : MonoBehaviour
{
    private NavMeshAgent agent;
    [SerializeField]
    private float weaponRange;
    private CharacterBehavior character;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        character = GetComponent<CharacterBehavior>();
        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = false;
        agent.updateUpAxis = false;

        weaponRange = character.GetWeaponRange();
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void ChaseTarget(Transform target)
    {
        agent.SetDestination(target.position);

        float distance = Vector3.Distance(transform.position, target.position);


        // back off if within weapon range
        if (distance < weaponRange)
        {


            // calculate direction away from the target point
            Vector3 direction = (transform.position - target.position).normalized;
            Vector3 wallDirection = GetDirectionAwayFromWall();
            Vector3 combinedDirection = (direction + wallDirection).normalized;

            // how far we want to flee
            float fleeDistance = 4f;

            // target flee position in world space
            Vector3 fleeTarget = transform.position + combinedDirection * fleeDistance;

            // find a valid point on the mesh near fleeTarget
            NavMeshHit hit;
            float sampleRadius = agent.radius * 10f + 0.5f;

            // basically trys to find the best valid position on the navmesh to flee to
            if (NavMesh.SamplePosition(fleeTarget, out hit, sampleRadius, NavMesh.AllAreas))
            {
                // only set destination if we get a valid navmesh position
                agent.SetDestination(hit.position);

            }
            else
            {
                Debug.Log("No valid NavMesh position found to flee to, moving away manually.");
                // This doesn't try to pathfind, it just moves the agent controller a little
                agent.Move(combinedDirection * agent.speed * Time.deltaTime);
            }
        }

    }

    private Vector3 GetDirectionAwayFromWall()
    {
        NavMeshHit hit;
        // Find the closest edge to the agent
        if (NavMesh.FindClosestEdge(agent.transform.position, out hit, NavMesh.AllAreas))
        {
            // This is the closest wall point on the NavMesh
            Vector3 closestWallPos = hit.position;
            Vector3 directionAwayFromWall = (agent.transform.position - closestWallPos).normalized;
            return directionAwayFromWall;
        }
        else
        {
            // No wall found stop movement
            return Vector3.zero;
        }
    }
}
