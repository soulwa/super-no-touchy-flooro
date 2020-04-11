using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionSystem : MonoBehaviour
{
    public static CollisionSystem instance = null;

    private void Awake()
    {
        //singleton pattern 
        if (instance == null) instance = this;
        else if (instance != this) Destroy(gameObject);

        DontDestroyOnLoad(this);
    }

    private void FixedUpdate()
    {
        //foreach(DynamicActor d in List<DynamicActors>())
        //{
        //    RaycastHit2D hit = d.BoxcastAll();
        //    if (hit)
        //    {
        //        d.ResolveCollisions(hit);
        //    }
        //}
    }

    public void ForceCollisionSystem()
    {
        //foreach (DynamicActor d in List<DynamicActors>())
        //{
        //    RaycastHit2D hit = d.BoxcastAll();
        //    if (hit)
        //    {
        //        d.ResolveCollisions(hit);
        //    }
        //}
    }
}
