using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Level
{
    public int id;

    private GameObject levelGeometry;

    public Level(int id)
    {
        this.id = id;
    }

    public void SetGeometry(GameObject levelGeometry)
    {
        this.levelGeometry = levelGeometry;
    }

    public GameObject GetGeometry()
    {
        return levelGeometry;
    }
}
