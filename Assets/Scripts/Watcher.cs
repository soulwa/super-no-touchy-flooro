using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Watcher : MonoBehaviour, IResettable
{
    private Vector2 velocity;
    private float xVelocitySmooth;
    public float moveSpeed;

    private bool active = false;

    private Vector2 spawnPos;

    public Player player;
    private BoxCollider2D boxCollider;

    private void Start()
    {
        spawnPos = transform.position;
        player = FindObjectOfType<Player>();
        velocity = Vector2.zero;
        boxCollider = GetComponent<BoxCollider2D>();
    }

    private void FixedUpdate()
    {
        if (Mathf.Approximately(player.transform.position.y, transform.position.y) && !active)
        {
            if (player.transform.position.x > transform.position.x)
            {
                Debug.Log(gameObject.name + " is active " + player.transform.position.y);
                active = true;
            }
        }
        else if ( (transform.position.y - player.transform.position.y) > 1 && active)
        {
            active = false;
        }

        if (active && Mathf.Abs(player.transform.position.x - transform.position.x) >= 5)
        {
            float xVelocityTarget = moveSpeed * Mathf.Sign(player.transform.position.x - transform.position.x);
            velocity.x = Mathf.SmoothDamp(velocity.x, xVelocityTarget, ref xVelocitySmooth, 0.1f);
        }
        else
        {
            velocity.x = 0;
            xVelocitySmooth = 0;
        }

        transform.Translate(velocity * Time.fixedDeltaTime);
    }

    public void Reset()
    {
        transform.position = spawnPos;
    }
}
