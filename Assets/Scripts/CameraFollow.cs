using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    //required classes for focus
    public Player target;

    float height;
    float width;

    //camera locking movement
    public Vector2 spawnPos;
    public GameObject cameraBoundingWalls;
    public Vector2 xPosLimits;
    public Vector2 yPosLimits;
    public bool lockedX;
    public bool lockedY;
    public bool twoFocus;
    public float focusPointX;

    //constants
    const float X_MOVE_INCREMENT = 4f;

    //variables to alter focus area
    public Vector2 focusAreaSize;
    public float yOffset;

    //focus area to be used
    private FocusArea focusArea;

    //for smooth camera movement
    public float xLookAhead;
    public float xSmoothTime;
    public float ySmoothTime;
    private float currentLookAhead;
    private float targetLookAhead;
    private float lookAheadDir;
    private bool lookingAhead;

    //for smoothdamp ref values
    private float xSmoothVelocity;
    private float ySmoothVelocity;

    private bool scrolling;
    private Vector3 lastFixedPosition;
    private float maxScrollX;

    public bool colorChanging;
    private Color startColor;
    public Color endColor;

    private void Start()
    {
        height = Camera.main.orthographicSize * 2;
        width = height * Camera.main.aspect;

        focusArea = new FocusArea(target.boxCollider.bounds, focusAreaSize);
        spawnPos = transform.position;

        xPosLimits = new Vector2(
            cameraBoundingWalls.transform.Find("lwall").transform.position.x,
            cameraBoundingWalls.transform.Find("rwall").transform.position.x
            );

        yPosLimits = new Vector2(
            cameraBoundingWalls.transform.Find("floor").transform.position.y,
            cameraBoundingWalls.transform.Find("ceiling").transform.position.y
            );

        startColor = Camera.main.backgroundColor;
    }

    private void LateUpdate()
    {
        // Debug.Log("scrolling = " + scrolling + ", lfp = " + lastFixedPosition + ", cur pos = " + transform.position);

        if (!scrolling)
        {
            focusArea.UpdateFocus(target.boxCollider.bounds);
            Vector2 focusPos = focusArea.center + Vector2.up * yOffset;

            if (focusArea.velocity.x != 0)
            {
                lookAheadDir = Mathf.Sign(focusArea.velocity.x);
                if (Mathf.Sign(focusArea.velocity.x) == Mathf.Sign(target.GetUserInput().x) && target.GetUserInput().x != 0)
                {
                    lookingAhead = false;
                    targetLookAhead = xLookAhead * lookAheadDir;
                }
                else
                {
                    if (!lookingAhead)
                    {
                        lookingAhead = true;
                        targetLookAhead = currentLookAhead + (lookAheadDir * xLookAhead - currentLookAhead) / X_MOVE_INCREMENT;
                    }
                }
            }

            currentLookAhead = Mathf.SmoothDamp(currentLookAhead, targetLookAhead, ref xSmoothVelocity, xSmoothTime);
            focusPos.y = Mathf.SmoothDamp(transform.position.y, focusPos.y, ref ySmoothVelocity, ySmoothTime);

            focusPos += Vector2.right * currentLookAhead;
            transform.position = (Vector3)focusPos + Vector3.forward * -10;
        }
        
        if (Input.GetButtonDown("ScrollR"))
        {
            CheckBounds();
            lastFixedPosition = transform.position;
            scrolling = true;
            // Debug.Log("lfp set to " + lastFixedPosition + " by transform.pos, which is " + transform.position);
            return;
        }

        if (Input.GetButton("ScrollR"))
        {
            if (!Input.GetButton("ScrollP"))
            {
                transform.position += (Vector3.right);
                
                maxScrollX = transform.position.x;  
            }
        }

        else
        {
            if (scrolling)
            {
                if (!Input.GetButton("ScrollP"))
                {
                    transform.position -= (Vector3.right * ((maxScrollX - lastFixedPosition.x) / 8f));
                    if (transform.position.x <= lastFixedPosition.x)
                    {
                        transform.position = lastFixedPosition;
                        scrolling = false;
                    }
                }
            }
        }

        if (colorChanging)
        {
            float dstCovered;
            if (yPosLimits.y - spawnPos.y > spawnPos.y - yPosLimits.x)
            {
                dstCovered = transform.position.y / (yPosLimits.y - height / 2);
            }
            else
            {
                dstCovered = transform.position.y / (yPosLimits.x + height / 2);
            }
            
            Camera.main.backgroundColor = Color.Lerp(startColor, endColor, dstCovered);
        }
        
        CheckBounds();
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(1, 0, 0, 0.3f);
        Gizmos.DrawCube(focusArea.center, focusAreaSize);
    }

    private void CheckBounds()
    {
        if (lockedX)
        {
            if (twoFocus && Mathf.Abs(transform.position.x - focusPointX) < Mathf.Abs(transform.position.x - spawnPos.x))
            {
                transform.position = new Vector3(focusPointX, transform.position.y, transform.position.z);
            }
            else
            {
                transform.position = new Vector3(spawnPos.x, transform.position.y, transform.position.z);
            }
        } 
        else
        {
            if (transform.position.x - width / 2 < xPosLimits.x) //left wall
            {
                transform.position = new Vector3(xPosLimits.x + width / 2, transform.position.y, transform.position.z);
            }
            else if (transform.position.x + width / 2 > xPosLimits.y) //right wall
            {
                transform.position = new Vector3(xPosLimits.y - width / 2, transform.position.y, transform.position.z);
            }
        }

        if (lockedY)
        {
            transform.position = new Vector3(transform.position.x, spawnPos.y, transform.position.z);
        }
        else
        {
            if (transform.position.y - height / 2 < yPosLimits.x) //floor
            {
                transform.position = new Vector3(transform.position.x, yPosLimits.x + height / 2, transform.position.z);
            }
            else if (transform.position.y + height / 2 > yPosLimits.y) //ceiling
            {
                transform.position = new Vector3(transform.position.x, yPosLimits.y - height / 2, transform.position.z);               
            }
        }
    }

    struct FocusArea
    {
        public Vector2 center;
        public Vector2 velocity;
        float top, bottom;
        float left, right;

        public FocusArea(Bounds targetBounds, Vector2 size)
        {
            left = targetBounds.center.x - size.x / 2;
            right = targetBounds.center.x + size.x / 2;
            top = targetBounds.min.y + size.y;
            bottom = targetBounds.min.y;

            velocity = Vector2.zero;
            center = new Vector2((left + right) / 2, (top + bottom) / 2);
        }

        public void UpdateFocus(Bounds targetBounds)
        {
            float xShift = 0;
            if (targetBounds.min.x < left)
            {
                xShift = targetBounds.min.x - left;
            }
            else if (targetBounds.max.x > right)
            {
                xShift = targetBounds.max.x - right;
            }
            left += xShift;
            right += xShift;

            float yShift = 0;
            if (targetBounds.min.y < bottom)
            {
                yShift = targetBounds.min.y - bottom;
            }
            else if (targetBounds.max.y > top)
            {
                yShift = targetBounds.max.y - top;
            }
            top += yShift;
            bottom += yShift;
            center = new Vector2((left + right) / 2, (top + bottom) / 2);
            velocity = new Vector2(xShift, yShift);
        }
    }
}