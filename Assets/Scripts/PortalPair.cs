using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PortalPair : MonoBehaviour
{
    public PortalController lPortal;
    public PortalController rPortal;

    public AudioClip portalSound;

    private bool on = true;

    private void FixedUpdate()
    {
        var lPortalHit = lPortal.FindCollisions();
        var rPortalHit = rPortal.FindCollisions();

        if (on)
        {
            ResolveCollisions(lPortalHit, rPortalHit);
        }
        else if (!on)
        {
            CheckOnStatus(lPortalHit, rPortalHit);
        }
    }

    private void ResolveCollisions(RaycastHit2D lPortalHit, RaycastHit2D rPortalHit)
    {
        if (lPortalHit)
        {
            on = false;
            lPortalHit.transform.position = rPortal.transform.position;
            AudioPlayer.instance.PlaySound(portalSound);
        }
        else if (rPortalHit)
        {
            on = false;
            rPortalHit.transform.position = lPortal.transform.position;
            AudioPlayer.instance.PlaySound(portalSound);
        }
    }

    private void CheckOnStatus(RaycastHit2D lPortalHit, RaycastHit2D rPortalHit)
    {
        if (!lPortalHit && !rPortalHit)
        {
            on = true;
        }
    }
}
