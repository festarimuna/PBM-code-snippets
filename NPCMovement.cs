using System.Collections;
using UnityEngine;
using UnityEngine.AI;

// OVERVIEW

// Gives an example of how I handle behaviour for NPCS in the game

// Handles NPC movement behaviour for friendly, enemy NPCs
// Handles wander, i.e. NPC roaming either 1) around original area or 2) around map
// Handles patrol, i.e. NPC moving between two points 

// Attach to NPC gameObject
// Ensure NPC gameObject childed to Group containing "Wander" or "Patrol" to have wander, patrol moving behaviour (else none)

public class NPCMovement : MonoBehaviour
{
    // private NF classes
    private ControllerEnemy controllerEnemy;
    private Fixes fixes;
    private GroupThink groupThink;
    private SheetEnemy sheetEnemy;
    private SheetNPC sheetNPC;

    // private Unity classes
    private Animator anim;
    private GameObject player;
    private NavMeshPath path;
    private NavMeshAgent agent;
    private System.Random rnd = new System.Random();
    private Vector3 originalWanderCenter;

    // private Unity classes
    private GameObject group;
    private Vector3 destination;

    public bool moving, initialised, active;

    // private fields
    private bool wanderFree, wandering, destPoint1, destPoint2;
    private static readonly float pi = Mathf.PI;
    private float rotMax, rotMin, rotSpeed, radiusPatrol, radiusWander, stopRange, timePatrol, timeWander;


    // Setup and Unity methods
    // called by ControllerEnemy.cs (enemy) or SheetNPC.cs (friendly)
    public void Initialise()
    {
        SetClassReferences();
        groupThink.SetGroup(gameObject);
        SetFields();
        StartBehaviour();

        initialised = true;
    }

    private void SetClassReferences()
    {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
        fixes = GameObject.Find("XMLManager").GetComponent<Fixes>();
        groupThink = GetComponent<GroupThink>();
        originalWanderCenter = gameObject.transform.position;
        path = new NavMeshPath();
        player = GameObject.Find("Player");

        HandleSetFriendlyOrEnemyReferences();
    }

    private void HandleSetFriendlyOrEnemyReferences()
    {
        if (gameObject.tag == "Enemy")
        {
            SetEnemyReferences();
        }

        if (gameObject.tag == "NPC")
        {
            SetFriendlyReferences();
        }
    }

    private void SetEnemyReferences()
    {
        controllerEnemy = GetComponent<ControllerEnemy>();
        sheetEnemy = GetComponent<SheetEnemy>();
    }

    private void SetFriendlyReferences()
    {
        sheetNPC = GetComponent<SheetNPC>();
    }

    private void SetFields()
    {
        destPoint1 = true;
        destPoint2 = false;
        group = groupThink.Group;
        rotMax = 180f;
        rotMin = -180f;
        rotSpeed = 1f;
        wandering = false;

        if (gameObject.tag == "Enemy")
        {
            SetEnemyFields();
        }

        if (gameObject.tag == "NPC")
        {
            SetNPCFields();
        }
    }

    private void SetEnemyFields()
    {
        radiusPatrol = sheetEnemy.radiusPatrol;
        radiusWander = sheetEnemy.radiusWander;
        stopRange = sheetEnemy.stopRangeGeneral;
        timePatrol = sheetEnemy.timePatrol;
        timeWander = sheetEnemy.timeWander;
        wanderFree = sheetEnemy.WanderFree;
    }

    private void SetNPCFields()
    {
        radiusPatrol = sheetNPC.RadiusPatrol;
        radiusWander = sheetNPC.RadiusWander;
        stopRange = sheetNPC.StopRangeGeneral;
        timePatrol = sheetNPC.TimePatrol;
        timeWander = sheetNPC.TimeWander;
        wanderFree = sheetNPC.WanderFree;
    }


    private void Update()
    {
        if (moving)
        {
            CheckStop();
        }
    } // not ideal implementation, but minimal performance hit


    // public interface
    // called by Initialise or ControllerEnemy.cs (if out of attack range after combat)
    public void StartBehaviour()
    {
        anim.SetBool("isMoving", false);
        active = true;

        StartPatrolWander();
    }
    // called by ControllerEnemy.cs (if in attack range before combat) - friendly NPCs never stop behaviour
    public void StopBehaviour()
    {
        active = false;

        StopAllCoroutines();
    }


    // patrol, wander
    private void StartPatrolWander()
    {
        if (group.name.Contains("Patrol"))
        {
            HandleStartPatrol();
        }

        else if (group.name.Contains("Wander"))
        {
            StartCoroutine(WanderForever());
        }
    }

    private void HandleStartPatrol()
    {
        if (gameObject.tag == "Enemy")
        {
            // if patrol destination set
            if (sheetEnemy.PatrolDestination != new Vector3(0f, 0f, 0f))
            {
                StartCoroutine(Patrol(sheetEnemy.PatrolDestination, gameObject.transform.position));
            }
            // otherwise get destination
            else
            {
                StartCoroutine(Patrol(ReturnReachablePoint(radiusPatrol), gameObject.transform.position));
            }
        }

        if (gameObject.tag == "NPC")
        {
            // if patrol destination set
            if (sheetNPC.PatrolDestination != new Vector3(0f, 0f, 0f))
            {
                StartCoroutine(Patrol(sheetNPC.PatrolDestination, gameObject.transform.position));
            }
            // otherwise get destination
            else
            {
                StartCoroutine(Patrol(ReturnReachablePoint(radiusPatrol), gameObject.transform.position));
            }
        }
    }

    public IEnumerator Patrol(Vector3 point1, Vector3 point2)
    {
        int roll;

        Vector3 destination = new Vector3(0f, 0f, 0f);

        while (ReturnIfNPCAlive())
        {
            roll = rnd.Next(1, 101);

            if (roll > 25) // 75% chance of beginning patrol
            {
                if (destPoint1) // default dest, set in SetFields()
                {
                    // if not within range of destination
                    if (fixes.ReturnDistanceToDestination(gameObject, point1) >= stopRange)
                    {
                        destination = point1;
                    }
                    else
                    {
                        destPoint1 = false;
                        destPoint2 = true;
                    }
                }
                if (destPoint2)
                {
                    // if not within range of destination
                    if (fixes.ReturnDistanceToDestination(gameObject, point2) >= stopRange)
                    {
                        destination = point2;
                    }
                    else
                    {
                        destPoint1 = true;
                        destPoint2 = false;
                    }
                }
                Move(destination);
            }
            yield return new WaitForSeconds(timePatrol);
        }
    } // move NPC between point1, point2; switching when 1) within range of each or 2) timePatrol passed

    private IEnumerator WanderForever()
    {
        int roll;

        while (ReturnIfNPCAlive())
        {
            roll = rnd.Next(1, 101);

            if (ReturnIfNPCAlive() && roll > 25) // 75% chance of wandering 
            {
                float interval = ReturnRandomInterval(timeWander);
                yield return new WaitForSeconds(interval);
                Wander();
            }
        }
    }

    private void Wander()
    {
        wandering = true;
        Vector3 destination = ReturnReachablePoint(radiusWander);

        Move(destination);
        wandering = false;
    }


    // movement methods
    private void Move(Vector3 dest)
    {
        destination = dest;
        moving = true;
        anim.SetBool("isMoving", true);

        if (gameObject.tag == "Enemy")
        {
            controllerEnemy.MoveToPos(dest);
        }

        if (gameObject.tag == "NPC")
        {
            sheetNPC.MoveToPos(dest);
        }
    }

    private void TurnAround()
    {
        // Smooth rotation
        Quaternion rotDesired = ReturnRotationAngle(fixes.ReturnFloatInRange(rotMin, rotMax));

        StartCoroutine(SmoothRotation(rotDesired));
    }

    private IEnumerator SmoothRotation(Quaternion rotDesired)
    {
        while (gameObject.transform.eulerAngles.y != rotDesired.eulerAngles.y)
        {
            transform.rotation = Quaternion.Lerp(transform.rotation, rotDesired, Time.deltaTime * rotSpeed);
            yield return new WaitForEndOfFrame();
        }
    }

    private void CheckStop()
    {
        float distanceLeft = Vector3.Distance(destination, transform.position);

        if (distanceLeft <= 5f)
        {
            moving = false;
            anim.SetBool("isMoving", false);
        }
    }


    // destination helper methods
    private Vector3 ReturnReachablePoint(float radius)
    {
        Vector3 randomDestination = ReturnRandomPointInCircle(radius);
        // set new randomDestination while randomDestination unreachable
        while (!ReturnPointReachable(randomDestination))
        {
            randomDestination = ReturnRandomPointInCircle(radius);
        }

        return randomDestination;
    }

    private Vector3 ReturnRandomPointInCircle(float radius)
    {
        Vector3 center = ReturnAppropriateWanderCenter();

        double roll = rnd.Next(0, 101) * 3.6; // roll between 0-360 degrees
        float angle = (float)roll;
        Vector3 pos;

        pos.x = center.x + radius * Mathf.Sin(angle * Mathf.Deg2Rad);
        pos.y = center.y;
        pos.z = center.z + radius * Mathf.Cos(angle * Mathf.Deg2Rad);

        return pos;
    }

    private bool ReturnPointReachable(Vector3 posTarget)
    {
        Vector3 pos = gameObject.transform.position;

        agent.CalculatePath(posTarget, path);

        if (path.status == NavMeshPathStatus.PathComplete)
        {
            return true;
        }

        if (path.status == NavMeshPathStatus.PathPartial)
        {
            return false;
        }

        if (path.status == NavMeshPathStatus.PathInvalid)
        {
            return false;
        }

        return false;
    }

    private Quaternion ReturnRotationAngle(float rotation)
    {
        Quaternion rotationAngle = new Quaternion
        {
            eulerAngles = new Vector3(0, rotation, 0)
        };

        return rotationAngle;
    }
    
    private Vector3 ReturnAppropriateWanderCenter()
    {
        Vector3 center = gameObject.transform.position;

        if (wanderFree == false)
        {
            center = originalWanderCenter;
        }
        else
        {
            center = gameObject.transform.position;
        }

        return center;
    } // return current or original position depending on whether NPC should wander entire map or around original area

    
    // helper methods
    private bool ReturnIfNPCAlive()
    {
        if (gameObject.tag == "Enemy")
        {
            if (sheetEnemy.CurrentHealth > 0)
            {
                return true;
            }
        }

        if (gameObject.tag == "NPC")
        {
            return true;
        }

        return false;
    }

    private float ReturnRandomInterval(float time)
    {
        float intervalRandom = Random.value * (Random.value * time);

        return intervalRandom;
    } // return random interval for NPC to wait until continuing behaviour
}