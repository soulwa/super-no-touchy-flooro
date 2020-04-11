using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class DynamicActor : RaycastController
{
    protected Vector2 velocity;

    // horizontal collisions for an actor who might have 0 x velocity, like the player
    // important to note horz collisions usually handles collisions inside the player, as it checks for overlap with the player itself.
    protected RaycastHit2D[] HorizontalCollisions(Vector2 moveDst, LayerMask collisionMask, float xDir)
    {
        List<RaycastHit2D> xCollisions = new List<RaycastHit2D>();

        float rayLength = Mathf.Abs(moveDst.x) + SKIN_WIDTH;

        if (Mathf.Abs(moveDst.x) < SKIN_WIDTH)
        {
            rayLength = 2 * SKIN_WIDTH; //cast farther if not moving
        }

        for (int i = 0; i < horizontalRayCount; i++)
        {
            Vector2 rayOrigin = (xDir == -1) ? raycastOrigins.bottomLeft : raycastOrigins.bottomRight;
            rayOrigin += Vector2.up * (horizontalRaySpace * i);
            RaycastHit2D[] hits = Physics2D.RaycastAll(rayOrigin, Vector2.right * xDir, rayLength, collisionMask);

            Debug.DrawRay(rayOrigin, Vector2.right * xDir, Color.red);

            if (hits.Length != 0)
            {
                foreach (RaycastHit2D hit in hits)
                {
                    // we only want to consider the closest thing hit, don't want to hit the same thing multiple times
                    if (hit.distance < rayLength)
                    {
                        rayLength = hit.distance;
                        xCollisions.Add(hit);
                    }
                }
            }
        }

        xCollisions.Concat(Physics2D.BoxCastAll(transform.position, Vector2.one * 0.985f, 0, Vector2.zero, 0, collisionMask));

        return xCollisions.ToArray();
    }

    // horizontal collisions where x direction is determined by velocity
    protected RaycastHit2D[] HorizontalCollisions(Vector2 moveDst, LayerMask collisionMask)
    {
        return HorizontalCollisions(moveDst, collisionMask, Mathf.Sign(moveDst.x));
    }

    protected RaycastHit2D[] VerticalCollisions(Vector2 moveDst, LayerMask collisionMask)
    {
        List<RaycastHit2D> yCollisions = new List<RaycastHit2D>();

        float yDir = Mathf.Sign(moveDst.y);
        float rayLength = Mathf.Abs(moveDst.y) + SKIN_WIDTH;

        for (int i = 0; i < verticalRayCount; i++)
        {
            Vector2 rayOrigin = (yDir == -1) ? raycastOrigins.bottomLeft : raycastOrigins.topLeft;
            rayOrigin += Vector2.right * (verticalRaySpace * i + moveDst.x);
            RaycastHit2D[] hits = Physics2D.RaycastAll(rayOrigin, Vector2.up * yDir, rayLength, collisionMask);

            Debug.DrawRay(rayOrigin, Vector2.up * yDir, Color.red);

            if (hits.Length != 0)
            {
                foreach (RaycastHit2D hit in hits)
                {
                    if (hit.distance < rayLength)
                    {
                        rayLength = hit.distance;
                        yCollisions.Add(hit);
                    }
                }
            }
        }
        return yCollisions.ToArray();
    }

    protected virtual void ResolveCollisions() { }

    protected virtual void Move() { }

    // this will determine all of the DynamicActors and static gameobjects this actor will collide with
    // will be a little intensive to calculate 
    // only use if the actor is a bullet- fast and likely to miss other things
    protected RaycastHit2D[] AllCollisions(Vector2 moveDst, LayerMask collisionMask)
    {
        return new RaycastHit2D[10];
    }
}
