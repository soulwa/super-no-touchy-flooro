using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EntityManager : MonoBehaviour
{
    public static EntityManager instance = null;

    public PowerPlatform platform;
    public Text deathText;
    public Text deathCount;

    public bool gravityFlipped = false;
    public bool levelComplete = false;

    private List<IResettable> resettables = new List<IResettable>();

    private void Awake()
    {
        //singleton pattern from unity's roguelike tut
        if (instance == null) instance = this;
        else if (instance != this) Destroy(gameObject);

        //find all objects in scene to be reset
        resettables = FindObjectsOfType<MonoBehaviour>().OfType<IResettable>().ToList();
        
        if (GameManager.instance.ClassicMode())
        {
            deathText.text = "lives";
        }
    }

    public void UpdateDeathCount(int deaths)
    {
        deathCount.text = deaths.ToString();
    }

    public void CreatePlatform(Vector2 pos, Quaternion rot)
    {
        var clone = Instantiate(platform, pos, rot);
        resettables.Add(clone);
    }

    public void ResetLevelEntities()
    {
        foreach (IResettable resettable in resettables)
        {
            if (!(resettable is Switch))
            {
                resettable.Reset();
            }
        }
        foreach (IResettable resettable in resettables)
        {
            if (resettable is Switch)
            {
                resettable.Reset();
            }
        }
        resettables.RemoveAll(r => r is PowerPlatform);
    }
}