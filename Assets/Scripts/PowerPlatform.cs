using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerPlatform : MonoBehaviour, IResettable
{
    public void Reset()
    {
        Destroy(gameObject);
    }
}
