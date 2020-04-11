using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class RaycastController : MonoBehaviour
{
    protected const float SKIN_WIDTH = 0.015f;
    private const float DST_BETWEEN_RAYS = 0.1f;

    [HideInInspector]
    public int horizontalRayCount;
    [HideInInspector]
    public int verticalRayCount;

    protected float horizontalRaySpace;
    protected float verticalRaySpace;

    public BoxCollider2D boxCollider;
    protected RaycastOrigins raycastOrigins;

    protected struct RaycastOrigins
    {
        public Vector2 topLeft, topRight;
        public Vector2 bottomLeft, bottomRight;
    }

    protected virtual void Awake()
    {
        boxCollider = GetComponent<BoxCollider2D>();
    }

    public void GetBoxCollider()
    {
        boxCollider = GetComponent<BoxCollider2D>();
    }

    protected virtual void Start()
    {
        CalculateRaySpacing();
    }

    protected void UpdateRaycastOrigins()
    {
        Bounds bounds = boxCollider.bounds;
        bounds.Expand(SKIN_WIDTH * -2);

        raycastOrigins.topLeft = new Vector2(bounds.min.x, bounds.max.y);
        raycastOrigins.topRight = new Vector2(bounds.max.x, bounds.max.y);
        raycastOrigins.bottomLeft = new Vector2(bounds.min.x, bounds.min.y);
        raycastOrigins.bottomRight = new Vector2(bounds.max.x, bounds.min.y);
    }

    protected void CalculateRaySpacing()
    {
        Bounds bounds = boxCollider.bounds;
        bounds.Expand(SKIN_WIDTH * -2);

        float boundsWidth = bounds.size.x;
        float boundsHeight = bounds.size.y;

        horizontalRayCount = Mathf.RoundToInt(boundsHeight / DST_BETWEEN_RAYS);
        verticalRayCount = Mathf.RoundToInt(boundsWidth / DST_BETWEEN_RAYS);

        horizontalRaySpace = bounds.size.y / (horizontalRayCount - 1);
        verticalRaySpace = bounds.size.x / (verticalRayCount - 1);
    }
}
