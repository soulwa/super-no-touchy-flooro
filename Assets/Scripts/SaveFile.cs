using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable()]
public class SaveFile
{
    public bool wasInGame = false;
    public int currentLevel = 0;
    public bool inClassicMode = false;
    public int deathCount = 0;
    public int lifeCount = 0;

    public HashSet<int> standardLevelIndicesCompleted = new HashSet<int>();
    public HashSet<int> classicLevelIndicesCompleted = new HashSet<int>();
}
