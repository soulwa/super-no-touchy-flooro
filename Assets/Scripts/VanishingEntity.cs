using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VanishingEntity : MonoBehaviour
{
    public Sprite outline;
    private Sprite original;

    private SpriteRenderer spriteRenderer;
    private BoxCollider2D boxCollider;
    private MovingPlatController mpc;
    private LayerMask originalLayerMask;
    private Vector2 mpcSpawn;

    private void Awake()
    {
        boxCollider = GetComponent<BoxCollider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        original = spriteRenderer.sprite;
        originalLayerMask = gameObject.layer;

        mpc = GetComponent<MovingPlatController>();
    }

    public void Appear()
    {
        gameObject.layer = originalLayerMask;
        spriteRenderer.sprite = original;

        if (mpc)
        {
            mpc.Unlock();
            Debug.Log("mpc Unlocked " + transform.name);
        }
    }

    public void Disappear()
    {
        if (mpc)
        {
            mpc.Lock();
            Debug.Log("mpc Locked " + transform.name);
        }

        gameObject.layer = LayerMask.NameToLayer("Vanish");
        spriteRenderer.sprite = outline;
    }
}
