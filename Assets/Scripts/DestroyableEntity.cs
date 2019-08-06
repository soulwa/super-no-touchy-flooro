using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyableEntity : MonoBehaviour, IResettable
{
    public virtual void Consume()
    {
        gameObject.SetActive(false);
    }

    public virtual void Reset()
    {
        gameObject.SetActive(true);
    }
}
