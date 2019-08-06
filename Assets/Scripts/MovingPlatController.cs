using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingPlatController : RaycastController, IResettable, ILockable
{
    private bool locked;

    public LayerMask passengerMask;

    public Vector2[] localWaypoints;
    private Vector2[] globalWaypoints;

    public bool cyclic;
    public bool respawning;
    public float speed;
    public float waitTime;
    [Range(0, 3)]
    public float easeAmount;

    private int fromWaypointIndex;
    private float percentBetweenWaypoints;
    private float nextMoveTime;
    private Vector2 startPoint;
    public Vector2 spawnPoint;

    List<Passenger> passengers = new List<Passenger>();
    Dictionary<Transform, Player> passengerTransforms = new Dictionary<Transform, Player>();

    protected override void Start()
    {
        base.Start();

        nextMoveTime = Time.time + waitTime;

        globalWaypoints = new Vector2[localWaypoints.Length];
        for (int i = 0; i < localWaypoints.Length; i++)
        {
            globalWaypoints[i] = localWaypoints[i] + (Vector2)transform.position;
        }
        startPoint = globalWaypoints[0];
        spawnPoint = transform.position;
        if (spawnPoint != startPoint)
        {
            percentBetweenWaypoints = Vector2.Distance(transform.position, globalWaypoints[0]) / Vector2.Distance(globalWaypoints[0], globalWaypoints[1]);
            percentBetweenWaypoints = Mathf.Clamp01(percentBetweenWaypoints);
        }
    }

    private void FixedUpdate()
    {
        if (!locked)
        {
            UpdateRaycastOrigins();

            Vector2 velocity = CalculatePlatformMovement();

            CalculatePassengerMovement(velocity);

            foreach (Passenger p in passengers)
            {
                Debug.Log("passenger on " + transform.name + ": " + p.transform.name);
            }

            MovePassengers(true);
            transform.Translate(velocity);
            MovePassengers(false);
        }
    }

    public void Lock()
    {
        locked = true;
    }

    public void Unlock()
    {
        locked = false;
    }

    private float Ease(float x)
    {
        float a = easeAmount + 1;
        return Mathf.Pow(x, a) / (Mathf.Pow(x, a) + Mathf.Pow(1 - x, a));
    }

    private Vector2 CalculatePlatformMovement()
    {
        if (Time.time < nextMoveTime)
        {
            return Vector2.zero;
        }

        fromWaypointIndex %= globalWaypoints.Length;
        int toWaypointIndex = (fromWaypointIndex + 1) % globalWaypoints.Length;
        float dstBetweenWaypoints = Vector2.Distance(globalWaypoints[fromWaypointIndex], globalWaypoints[toWaypointIndex]);
        percentBetweenWaypoints += Time.fixedDeltaTime * speed / dstBetweenWaypoints;
        percentBetweenWaypoints = Mathf.Clamp01(percentBetweenWaypoints);
        float easedPercent = Ease(percentBetweenWaypoints);
        
        Vector2 newPos = Vector2.Lerp(globalWaypoints[fromWaypointIndex], globalWaypoints[toWaypointIndex], easedPercent);

        if (percentBetweenWaypoints >= 1)
        {
            percentBetweenWaypoints = 0;
            fromWaypointIndex++;

            if (respawning) //do not use this option w eased percent- messes up prediction for where plat will be
            {
                transform.position = startPoint;
                fromWaypointIndex = 0;
                return Vector2.zero;
            }

            if (!cyclic)
            {
                if (fromWaypointIndex >= globalWaypoints.Length - 1)
                {
                    fromWaypointIndex = 0;
                    System.Array.Reverse(globalWaypoints);
                }
            }
            nextMoveTime = Time.time + waitTime;
        }
        return newPos - (Vector2)transform.position;
    }

    struct Passenger
    {
        public Vector2 velocity;
        public Transform transform;
        public bool onPlatform;
        public bool moveBeforePlat;

        public Passenger(Vector2 velocity, Transform transform, bool onPlatform, bool moveBeforePlat)
        {
            this.velocity = velocity;
            this.transform = transform;
            this.onPlatform = onPlatform;
            this.moveBeforePlat = moveBeforePlat;
        }
    }

    private void MovePassengers(bool moveBeforePlat)
    {
        foreach (Passenger p in passengers)
        {
            if (!passengerTransforms.ContainsKey(p.transform))
            {
                passengerTransforms.Add(p.transform, p.transform.GetComponent<Player>());
            }

            if (p.moveBeforePlat == moveBeforePlat)
            {
                passengerTransforms[p.transform].Move(p.velocity, p.onPlatform);
            }
        }
    }

    private void CalculatePassengerMovement(Vector2 velocity)
    {
        HashSet<Transform> movedPassengers = new HashSet<Transform>();
        passengers.Clear();

        float yDir = Mathf.Sign(velocity.y);
        float xDir = Mathf.Sign(velocity.x);
        float gravDir = (EntityManager.instance.gravityFlipped ? -1 : 1);

        //vert ride in same dir
        if (velocity.y != 0)
        {
            float rayLength = Mathf.Abs(velocity.y) + SKIN_WIDTH;
            
            for (int i = 0; i < verticalRayCount; i++)
            {
                Vector2 rayOrigin = (yDir == -1) ? raycastOrigins.bottomLeft : raycastOrigins.topLeft;
                rayOrigin += Vector2.right * (horizontalRaySpace * i);
                RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up * yDir, rayLength, passengerMask);

                if (hit && hit.distance != 0)
                {   //bad code- should be more general check, but only moving player now: should never be passenger of 2
                    if (!movedPassengers.Contains(hit.transform))
                    {
                        movedPassengers.Add(hit.transform);
                        float xPush = yDir == 1 ? velocity.x : 0;
                        float yPush = velocity.y - (hit.distance - SKIN_WIDTH) * yDir;

                        passengers.Add(new Passenger(new Vector2(xPush, yPush), hit.transform, yDir * gravDir == 1, true));
                        //Debug.Log("passenger moving before: " + (yDir * gravDir == 1).ToString());
                    }
                }
            }
        }

        //horz push
        if (velocity.x != 0)
        {
            float rayLength = Mathf.Abs(velocity.x) + SKIN_WIDTH;

            for (int i = 0; i < horizontalRayCount; i++)
            {
                Vector2 rayOrigin = (xDir == -1) ? raycastOrigins.bottomLeft : raycastOrigins.bottomRight;
                rayOrigin += Vector2.up * (horizontalRaySpace * i);
                RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * xDir, rayLength, passengerMask);

                if (hit && hit.distance != 0)
                {
                    if (!movedPassengers.Contains(hit.transform))
                    {
                        movedPassengers.Add(hit.transform);
                        float xPush = velocity.x - (hit.distance - SKIN_WIDTH) * xDir;
                        float yPush = -SKIN_WIDTH;

                        passengers.Add(new Passenger(new Vector2(xPush, yPush), hit.transform, false, true));
                    }
                }
            }
        }

        //horz ride, vert riding down
        if (yDir * gravDir == -1 || velocity.x != 0 && velocity.y == 0)
        {
            float rayLength = 2 * SKIN_WIDTH;
            Vector2 originDir = (gravDir == 1 ? raycastOrigins.topLeft : raycastOrigins.bottomLeft);
            Vector2 hitDir = (gravDir == 1 ? Vector2.up : Vector2.down);

            for (int i = 0; i < verticalRayCount; i++)
            {
                Vector2 rayOrigin = originDir + Vector2.right * (verticalRaySpace * i);
                RaycastHit2D hit = Physics2D.Raycast(rayOrigin, hitDir, rayLength, passengerMask);

                if (hit && hit.distance != 0)
                {
                    //Debug.Log("passenger riding down");
                    if (!movedPassengers.Contains(hit.transform))
                    {
                        movedPassengers.Add(hit.transform);
                        float xPush = velocity.x;
                        float yPush = velocity.y;

                        passengers.Add(new Passenger(new Vector2(xPush, yPush), hit.transform, true, false));
                    }
                }
            }
        }
    }

    public void Reset()
    {
        Debug.Log("mpc Reset " + transform.name);
        transform.position = spawnPoint;

        if (globalWaypoints[0] != startPoint)
        {
            System.Array.Reverse(globalWaypoints);
        }

        if (spawnPoint != startPoint)
        {
            percentBetweenWaypoints = Vector2.Distance(transform.position, globalWaypoints[0]) / Vector2.Distance(globalWaypoints[0], globalWaypoints[1]);
            percentBetweenWaypoints = Mathf.Clamp01(percentBetweenWaypoints);
        }
        else
        {
            percentBetweenWaypoints = 0;
        }

        fromWaypointIndex = 0;
        nextMoveTime = Time.time + waitTime;
    }

    private void OnDrawGizmos()
    {
        if (localWaypoints != null)
        {
            Gizmos.color = Color.red;
            float size = 0.3f;

            for (int i = 0; i < localWaypoints.Length; i++)
            {
                Vector2 globalPos = Application.isPlaying ? globalWaypoints[i] : localWaypoints[i] + (Vector2)transform.position;
                Gizmos.DrawLine(globalPos - Vector2.up * size, globalPos + Vector2.up * size);
                Gizmos.DrawLine(globalPos - Vector2.right * size, globalPos + Vector2.right * size);
            }
        }
    }
}