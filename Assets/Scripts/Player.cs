using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class Player : DynamicActor, ILockable, IResettable
{
    private enum CollisionType
    {
        VERTICAL,
        HORIZONTAL
    }

    private struct CollisionInfo
    {
        public bool above, below;
        public bool left, right;
        public bool collided;
        public bool collidedWater;

        public int faceDir;

        public bool hitHazard;

        public void Reset()
        {
            collided = false;
            above = below = false;
            left = right = false;
            hitHazard = false;
            collidedWater = false;
        }
    }

    private Vector2 userInput;
    private Vector2 lastDirectionalUserInput;

    public LayerMask collisionMask;
    private CollisionInfo collisions;

    const float X_VELOCITY_THRESHHOLD = 0.5f; //unused anim

    //variables to handle normal jumping
    public float gravity;
    private float jumpVelocityMax;
    private float jumpVelocityMin;
    public float jumpHeightMax = 4;
    public float jumpHeightMin = 1;
    public float timeToJumpApex = 0.4f;

    //later level conditions: very messy code associated
    private bool inQuicksand;
    private const float QS_SPEED = 2;
    private bool inWater;
    private float waterGravity;
    private float swimHeight = 1;
    private float swimVelocity;
    public bool windy;
    private const float WIND_SPEED = 4;

    //adding leniency to jumping after falling off plat 
    [SerializeField]
    private bool canJump;
    private readonly float jumpLeniencyTime = 0.1f;
    private float jumpLeniencyTimer;

    //variables to handle acceleration from smoothDamp
    private float aerialAcceleration = 0.2f;
    private float groundAcceleration = 0.1f;

    //variables to handle velocity/horz. movement
    private float moveSpeed = 8;
    private float xVelocitySmooth;
    // private Vector2 velocity;
    private const float TERMINAL_VEL = -27.5f; //ALSO TRY WITH 30F FOR TWISTY, test 2 builds
    private const float WATER_TERMINAL_VEL = -12f;

    //variables to handle wall jumping
    private bool wallSliding;
    private int wallDir;
    private float wallSlideSpeed = 3;
    public Vector2 wallClimb; //jump with direction facing wall
    public Vector2 wallJumpSmall; //neutral jump
    public Vector2 wallJumpBig; //jump facing away from wall
    private readonly float wallHangTime = 0.25f;
    private float hangTimer;

    //restrict player movement
    //death system needs a whole fucking rework, very messy w artifacts of GM controlling everything
    private bool locked;
    private float deathTime = 0.1f;
    private float deathTimer;
    private bool invincible = false;

    //handling player's powerups
    private LinkedList<PowerupType> powerups = new LinkedList<PowerupType>();
    private int powerupSize = 3;
    private Image[] powerupsUI;
    public float yPlatformOffset;
    public float xPlatformOffset;
    public bool greedy;

    private readonly float dashTime = 0.3f;
    private float dashTimer;
    private bool dashing;
    private const float DASH_SPEED = 20f;
    private float linearDrag;
    private float dragSmooth;
    private const float DRAG_MAX = 2;

    private const float teleportDistance = 3.5f;
    private bool teleporting;
    public LayerMask teleportMask;
    private readonly float teleportTime = 0.2f;
    private float teleportTimer;

    public bool gravityFlipped = false;
    private bool canFlipGravity = true;

    //moving plat interactions
    public bool isPassenger = false;

    //death 
    private Vector2 spawnPoint;
    public bool dead;

    //sounds
    public AudioClip playerJumpSound;
    public AudioClip makePlatformSound;
    public AudioClip playerDieSound;
    public AudioClip playerDashSound;
    public AudioClip playerTeleportSound;
    public AudioClip flipGravitySound;

    public bool debug;

    public event Action onPlayerDeath;

    protected override void Start()
    {
        base.Start();
        collisions.faceDir = 1;

        spawnPoint = transform.position;

        gravity = -(2 * jumpHeightMax) / Mathf.Pow(timeToJumpApex, 2);
        jumpVelocityMax = -gravity * timeToJumpApex;
        jumpVelocityMin = Mathf.Sqrt(2 * -gravity * jumpHeightMin);

        waterGravity = -(2 * swimHeight) / (timeToJumpApex * timeToJumpApex);
        swimVelocity = -waterGravity * timeToJumpApex;

        hangTimer = wallHangTime;
        jumpLeniencyTimer = jumpLeniencyTime;
        dashTimer = dashTime;

        var pwUIcontainer = GameObject.Find("powerups");
        if (pwUIcontainer != null)
        {
            powerupsUI = pwUIcontainer.GetComponentsInChildren<Image>();
            UpdatePowerupUI();
        }
    }

    private void Update()
    {
        SetUserInput(new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")));

        if (userInput != Vector2.zero)
        {
            lastDirectionalUserInput = userInput;
        }

        if (Input.GetButtonDown("Jump"))
        {
            OnJumpKeyDown();
        }

        if (Input.GetButtonUp("Jump") && !dashing) //this is why dashing is broken sometimes?
        {
            OnJumpKeyUp();
        }

        if (Input.GetButtonDown("Powerup"))
        {
            OnPowerupKeyDown();
        }

        if (Input.GetButtonDown("Reset"))
        {
            OnHazard();
        }
    }

    private void OnPowerupKeyDown()
    {
        if (powerups.Count == 0)
        {
            return;
        }

        switch (powerups.Last.Value)
        {
            case PowerupType.PLATFORM:
                FormPlatform();
                break;
            case PowerupType.DASH:
                Dash();
                break;
            case PowerupType.TELEPORT:
                Teleport();
                break;
            case PowerupType.FLOOR:
                Floor();
                break;
        }

        powerups.RemoveLast();
        UpdatePowerupUI();
    }

    private void UpdatePowerupUI()
    {
        if (powerupsUI != null)
        {
            for (int i = 0; i < powerupsUI.Length; i++)
            {
                switch (powerups.ElementAtOrDefault(i))
                {
                    case PowerupType.PLATFORM:
                        powerupsUI[i].color = Color.red;
                        break;
                    case PowerupType.DASH:
                        powerupsUI[i].color = Color.blue;
                        break;
                    case PowerupType.TELEPORT:
                        powerupsUI[i].color = Color.green;
                        break;
                    case PowerupType.FLOOR:
                        powerupsUI[i].color = Color.yellow;
                        break;
                    default:
                        powerupsUI[i].color = Color.clear;
                        break;
                }
            }
        }
    }

    private void FixedUpdate()
    {
        if (!locked && !teleporting) //change to dead? original purpose of lock was about death, so find better way to lock
        {
            CalculateVelocity();
            if (!inQuicksand) HandleWallSliding();

            if (!collisions.below && canJump)
            {
                jumpLeniencyTimer -= Time.fixedDeltaTime;
                if (jumpLeniencyTimer <= 0)
                {
                    canJump = false;
                    jumpLeniencyTimer = jumpLeniencyTime;
                }
            }
            else if ((collisions.below && !canJump) || (gravityFlipped && collisions.above && !canJump))
            {
                canJump = true;
            }

            if (dashing)
            {
                dashTimer -= Time.fixedDeltaTime;
                if (dashTimer <= 0)
                {
                    dashing = false;
                    dashTimer = dashTime;
                }
            }

            //reset state flags here, wait until better collide "exit" system...
            inQuicksand = false;

            Move(velocity * Time.fixedDeltaTime);

            if (!collisions.collidedWater && inWater)
            {
                inWater = false;
            }

            if (collisions.above || collisions.below)
            {
                velocity.y = 0;
            }
        }
        else if (locked && dead)
        {
            deathTimer -= Time.fixedDeltaTime;
            if (deathTimer <= 0)
            {
                Unlock();
                dead = false;
            }
        }
        else if (teleporting)
        {
            teleportTimer -= Time.fixedDeltaTime;
            if (teleportTimer <= 0)
            {
                teleporting = false;
                GetComponent<SpriteRenderer>().enabled = true;
                teleportTimer = teleportTime;
            }
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

    public void Reset()
    {
        if (gravityFlipped) FlipGravity(false);
        Lock();
        deathTimer =  deathTime;
        transform.position = spawnPoint;
        
        velocity = Vector2.zero;
        xVelocitySmooth = 0;

        powerups.Clear();
        UpdatePowerupUI();

        dashing = false;
    }

    public Vector2 GetUserInput()
    {
        return userInput;
    }

    private void SetUserInput(Vector2 input)
    {
        userInput = input;
    }

    public void OnJumpKeyDown()
    {
        if (wallSliding)
        {
            if (userInput.x == wallDir)
            {
                velocity.x = wallClimb.x * -wallDir;
                velocity.y = wallClimb.y;
            }
            else if (userInput.x == 0)
            {
                velocity.x = wallJumpSmall.x * -wallDir;
                velocity.y = wallJumpSmall.y;
            }
            else
            {
                velocity.x = wallJumpBig.x * -wallDir;
                velocity.y = wallJumpBig.y;
            }
            AudioPlayer.instance.PlaySound(playerJumpSound);
        }

        if (canJump || inQuicksand || inWater)
        {
            if (inQuicksand)
            {
                velocity.y = jumpVelocityMax * 0.8f;
            }
            else if (inWater)
            {
                if (Physics2D.OverlapPoint(boxCollider.bounds.max, LayerMask.GetMask("GWater")))
                {
                    velocity.y = swimVelocity;
                }
                else
                {
                    velocity.y = jumpVelocityMax;
                }
            }
            else
            {
                velocity.y = jumpVelocityMax;
            }
            AudioPlayer.instance.PlaySound(playerJumpSound);
        }
    }


    public void OnJumpKeyUp()
    {
        if (!gravityFlipped && velocity.y > jumpVelocityMin)
        {
            velocity.y = jumpVelocityMin;
        }
        else if (gravityFlipped && velocity.y < jumpVelocityMin)
        {
            velocity.y = jumpVelocityMin;
        }
    }

    private void FormPlatform()
    {
        Vector3 platformPosition;
        if (velocity.x > 0) platformPosition = Vector3.down * yPlatformOffset + Vector3.right * xPlatformOffset;
        else if (velocity.x < 0) platformPosition = Vector3.down * yPlatformOffset + Vector3.left * xPlatformOffset;
        else platformPosition = Vector3.down * yPlatformOffset;

        EntityManager.instance.CreatePlatform(transform.position + platformPosition, transform.rotation);
        AudioPlayer.instance.PlaySound(makePlatformSound);
    }

    private void Dash() //maybe use particles or trail renderer in future for dash
    {
        dashing = true;

        Vector2 dashVelocity = new Vector2(userInput.x, userInput.y).normalized * DASH_SPEED;

        if (dashVelocity == Vector2.zero)
        {
            dashVelocity.x = lastDirectionalUserInput.x * DASH_SPEED;
        }

        velocity = dashVelocity;
        linearDrag = DRAG_MAX;

        AudioPlayer.instance.PlaySound(playerDashSound);
    }

    private void Teleport()
    {
        teleporting = true;

        velocity = Vector2.zero;

        float finalTeleportDistance = teleportDistance;

        Vector2 teleportDir = Vector2.zero;
        if (userInput.x != 0 && userInput.y == 0)
        {
            teleportDir.x = userInput.x;
        }
        else if (userInput.y != 0)
        {
            teleportDir.y = userInput.y;
        }
        else if (lastDirectionalUserInput.x != 0 && lastDirectionalUserInput.y == 0)
        {
            teleportDir.x = lastDirectionalUserInput.x;
        }
        else
        {
            teleportDir.y = lastDirectionalUserInput.y;
        }

        RaycastHit2D hit = Physics2D.BoxCast(transform.position, Vector2.one * 0.985f, 0, teleportDir, finalTeleportDistance +
            boxCollider.bounds.extents.x, teleportMask);

        if (hit)
        {
            Debug.Log(hit.transform.name);
            if (hit.distance + (teleportDir.x == 0 ? hit.collider.bounds.size.y : hit.collider.bounds.size.x)
                > finalTeleportDistance)
            {
                finalTeleportDistance = hit.distance +
                    (teleportDir.x == 0 ? hit.collider.bounds.size.y : hit.collider.bounds.size.x);
            }

            Collider2D colliderHit;
            do
            {
                colliderHit = Physics2D.OverlapBox((Vector2)transform.position + teleportDir * finalTeleportDistance,
                    Vector2.one * 0.959f, 0, teleportMask);
                if (colliderHit)
                {
                    Debug.Log(colliderHit.name + ", " + finalTeleportDistance);
                }
            } while (colliderHit && ++finalTeleportDistance < 15f);
        }

        GetComponent<SpriteRenderer>().enabled = false;
        if (finalTeleportDistance < 15f)
        {
            transform.position += (Vector3)(teleportDir * finalTeleportDistance);
        }
        AudioPlayer.instance.PlaySound(playerTeleportSound);
    }

    private void Floor()
    {
        invincible = true;
    }

    private void FlipGravity(bool noisy = true)
    {
        gravityFlipped = !gravityFlipped;
        EntityManager.instance.gravityFlipped = !EntityManager.instance.gravityFlipped;
        yPlatformOffset = -yPlatformOffset;

        gravity = -gravity;
        jumpVelocityMax = -gravity * timeToJumpApex;
        jumpVelocityMin = Mathf.Sqrt(Mathf.Abs(2 * gravity * jumpHeightMin)) * -Mathf.Sign(gravity);
        wallSlideSpeed = -wallSlideSpeed;
        wallJumpSmall.y = -wallJumpSmall.y;
        wallJumpBig.y = -wallJumpBig.y;
        wallClimb.y = -wallClimb.y;
        if (noisy) AudioPlayer.instance.PlaySound(flipGravitySound);
    }

    private void OnHazard()
    {
        dead = true;
        onPlayerDeath();
        AudioPlayer.instance.PlaySound(playerDieSound);
    }

    private void CalculateVelocity()
    {
        linearDrag = Mathf.SmoothDamp(linearDrag, 0, ref dragSmooth, 0.8f);
        if (dashing)
        {
            velocity *= (1 - linearDrag * Time.fixedDeltaTime);
        }
        else
        {
            float xVelocityTarget = userInput.x * moveSpeed - (windy ? WIND_SPEED : 0);
            if (greedy && powerups.Count > powerupSize)
            {
                xVelocityTarget = userInput.x * (moveSpeed - (powerups.Count > 5 ? 4 : 2));
            }

            velocity.x = Mathf.SmoothDamp(velocity.x, xVelocityTarget, ref xVelocitySmooth,
                (collisions.below) ? groundAcceleration : aerialAcceleration);

            if (inQuicksand)
            {
                if (velocity.y < -QS_SPEED) velocity.y = 0;
                velocity.y = Mathf.MoveTowards(velocity.y, -QS_SPEED, 2f);
            }
            else if (inWater)
            {
                velocity.y += waterGravity * Time.fixedDeltaTime;
            }
            else
            {
                float pwupWeight = 0;
                if (greedy && powerups.Count > powerupSize)
                {
                    pwupWeight = Mathf.Sign(gravity) * (powerups.Count > 5 ? 40 : 20);
                }

                velocity.x *= (1 - linearDrag * Time.fixedDeltaTime);

                velocity.y += (gravity + pwupWeight) * Time.fixedDeltaTime;

            }

            if (inWater && velocity.y < WATER_TERMINAL_VEL)
            {
                velocity.y = WATER_TERMINAL_VEL;
            }

            if (velocity.y < TERMINAL_VEL)
            {
                velocity.y = TERMINAL_VEL;
            }

            groundAcceleration = 0.1f;
        }
    }

    private void HandleWallSliding()
    {
        wallDir = (collisions.left) ? -1 : 1;
        wallSliding = false;

        if ((collisions.left || collisions.right) && !collisions.below &&
            (!gravityFlipped ? velocity.y < 0 : velocity.y > 0))
        {
            wallSliding = true;

            if (!gravityFlipped && velocity.y < wallSlideSpeed)
            {
                velocity.y = -wallSlideSpeed;
            }
            else if (gravityFlipped && velocity.y > wallSlideSpeed)
            {
                velocity.y = -wallSlideSpeed;
            }

            if (hangTimer > 0)
            {
                xVelocitySmooth = 0;
                velocity.x = 0;
                if (userInput.x != wallDir && userInput.x != 0)
                {
                    hangTimer -= Time.fixedDeltaTime;
                }
                else hangTimer = wallHangTime;
            }
            else hangTimer = wallHangTime;
        }
    }

    public void Move(Vector2 moveDst, bool standingOnPlatform = false)
    {
        collisions.Reset(); //hasn't collided yet
        UpdateRaycastOrigins(); //change collisions

        if (velocity.x != 0)
        {
            collisions.faceDir = (int)Mathf.Sign(velocity.x);
        }

        RaycastHit2D[] xHits = HorizontalCollisions(moveDst, collisionMask, collisions.faceDir);
        foreach (RaycastHit2D hit in xHits)
        {
            ResolveCollisions(hit, CollisionType.HORIZONTAL, ref moveDst, collisions.faceDir);
        }
        RaycastHit2D[] yHits = VerticalCollisions(moveDst, collisionMask);
        foreach (RaycastHit2D hit in yHits)
        {
            ResolveCollisions(hit, CollisionType.VERTICAL, ref moveDst, Mathf.Sign(moveDst.y));
        }

        if (!collisions.collided)
        {
            if (!canFlipGravity) canFlipGravity = true;
            inQuicksand = false;
            inWater = false;
        }

        if (!collisions.hitHazard) transform.Translate(moveDst);

        if (standingOnPlatform)
        {
            if (gravityFlipped)
            {
                collisions.above = true;
            }
            else
            {
                collisions.below = true; //fix moving platform interaction
            }
        }
    }

    void ResolveCollisions(RaycastHit2D hit, CollisionType collisionType, ref Vector2 moveDst, float moveDir) //, ref float rayLength)
    {
        collisions.collided = true;

        if (collisionType == CollisionType.HORIZONTAL && hit.distance == 0)
        {
            if (hit.transform.tag != "Hazard")
            {
                return;
            }
        }

        if (hit.transform.tag != "Gravity Switch")
        {
            if (!canFlipGravity) canFlipGravity = true;
        }

        switch (hit.transform.tag)
        {
            case "Hazard":
                if (!collisions.hitHazard) //fix after making collisions end after 1 ray... doesn't really make sense to continue w/o slopes
                {
                    if (!invincible)
                    {
                        OnHazard();
                        collisions.hitHazard = true;
                    }
                }
                break;
            case "Powerup":
                Powerup pw = hit.collider.GetComponent<Powerup>();
                powerups.AddLast(pw.GetPowerupType());
                if (powerups.Count > powerupSize && !greedy)
                {
                    powerups.RemoveFirst();
                }
                UpdatePowerupUI();
                pw.Consume();
                return;
            case "Goal":
                EntityManager.instance.levelComplete = true;
                hit.collider.GetComponent<Goal>().GoalComplete();
                return;
            case "Through":
                if (collisionType == CollisionType.VERTICAL)
                {
                    if (moveDir == 1 || hit.distance == 0 || userInput.y == -1) //watch for bugs from removing fallingThroughPlat, invoke after .5 sec delay (seblague)
                        return;
                    break;
                }
                else return;
            case "Yoku":
                if (collisionType == CollisionType.VERTICAL)
                {
                    if (hit.distance == 0)
                        return;
                }
                break;
            case "Ice":
                if (collisionType == CollisionType.VERTICAL)
                {
                    groundAcceleration = 0.75f;
                }
                break;
            case "Gravity Switch":
                if (canFlipGravity)
                {
                    FlipGravity();
                    canFlipGravity = false;
                    return;
                }
                else return;
            case "Entity Switch":
                hit.collider.GetComponent<Switch>().Consume();
                return;
            case "Quicksand":
                inQuicksand = true;
                return;
            case "Water":
                if (!inWater)
                {
                    inWater = true;
                    this.velocity.y = Mathf.Clamp(this.velocity.y, -2f, 10f);
                }
                collisions.collidedWater = true;
                return;
            case "Checkpoint":
                spawnPoint = hit.transform.position;
                hit.collider.GetComponent<Goal>().GoalComplete();
                return;
            default:
                break;
        }

        if (collisionType == CollisionType.VERTICAL)
        {
            moveDst.y = Mathf.Min(Mathf.Abs(moveDst.y), (hit.distance - SKIN_WIDTH)) * moveDir;

            if (!gravityFlipped)
            {
                collisions.below = moveDir == -1;
                collisions.above = moveDir == 1;
            }
            else
            {
                collisions.below = moveDir == 1;
                collisions.above = moveDir == -1;
            }
        }
        else
        {
            moveDst.x = Mathf.Min(Mathf.Abs(moveDst.x), (hit.distance - SKIN_WIDTH)) * moveDir;

            collisions.left = moveDir == -1;
            collisions.right = moveDir == 1;
        }
    }
}
