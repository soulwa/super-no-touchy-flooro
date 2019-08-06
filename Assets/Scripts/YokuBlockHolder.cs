using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class YokuBlockHolder : MonoBehaviour, IResettable, ILockable
{
    private bool locked;

    public GameObject[] cycles;
    public int emptyCycles;
    public float cycleTime;
    
    private int currentCycle;
    private float cycleTimer;

    private void Start()
    {
        cycleTimer = cycleTime;
        currentCycle = 0;
        ActivateBlocks(currentCycle);
    }

    private void Update()
    {
        if (locked) return;

        cycleTimer -= Time.deltaTime;
        if (cycleTimer <= 0)
        {
            currentCycle++;
            if (currentCycle == cycles.Length + emptyCycles)
            {
                currentCycle = 0;
            }
            cycleTimer = cycleTime;
            ActivateBlocks(currentCycle);
        }
    }

    public void Reset()
    {
        currentCycle = 0;
        cycleTimer = cycleTime;
        ActivateBlocks(currentCycle);
    }

    public void Lock()
    {
        locked = true;
    }

    public void Unlock()
    {
        locked = false;
    }
    
    private void ActivateBlocks(int index)
    {
        for (int i = 0; i < cycles.Length; i++)
        {
            if (i == index)
            {
                foreach (VanishingEntity v in cycles[i].GetComponentsInChildren(typeof(VanishingEntity), true))
                {
                    v.Appear();
                }
            }
            else
            {
                foreach (VanishingEntity v in cycles[i].GetComponentsInChildren(typeof(VanishingEntity), true))
                {
                    v.Disappear();
                }
            }
        }
    }
}
