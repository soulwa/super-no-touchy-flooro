using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PortalController : RaycastController
{

    public LayerMask transportMask;

    private bool inUse;

    protected override void Start()
    {
        base.Start();
        UpdateRaycastOrigins();
    }

    private void FixedUpdate()
    {
        UpdateRaycastOrigins();
    }

    public RaycastHit2D FindCollisions()
    {
        float rayLength = SKIN_WIDTH;

        //just horizontal for now
        for (int i = 0; i < horizontalRayCount; i++)
        {
            Vector2 rayOriginLeft = raycastOrigins.bottomLeft + Vector2.up * (horizontalRaySpace * i);
            Vector2 rayOriginRight = raycastOrigins.bottomRight + Vector2.up * (horizontalRaySpace * i);
            RaycastHit2D hitLeft = Physics2D.Raycast(rayOriginLeft, Vector2.left, rayLength, transportMask);
            RaycastHit2D hitRight = Physics2D.Raycast(rayOriginRight, Vector2.right, rayLength, transportMask);

            if (hitLeft) return hitLeft;

            else if (hitRight) return hitRight;
        }

        for (int i = 0; i < verticalRayCount; i++)
        {
            Vector2 rayOriginUp = raycastOrigins.topLeft + Vector2.right * (verticalRaySpace * i);
            Vector2 rayOriginDown = raycastOrigins.bottomLeft + Vector2.right * (verticalRaySpace * i);
            RaycastHit2D hitUp = Physics2D.Raycast(rayOriginUp, Vector2.up, rayLength, transportMask);
            RaycastHit2D hitDown = Physics2D.Raycast(rayOriginDown, Vector2.down, rayLength, transportMask);

            Debug.DrawRay(rayOriginUp, Vector2.up, Color.blue);
            Debug.DrawRay(rayOriginDown, Vector2.up, Color.blue);

            if (hitUp) return hitUp;

            else if (hitDown) return hitDown;
        }

        return new RaycastHit2D();
    }
}
