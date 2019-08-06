using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Goal : MonoBehaviour
{
    public Sprite greenFlag;
    public AudioClip completeSound;

    private SpriteRenderer spriteR;
    private bool completed;

    private void Start()
    {
        spriteR = GetComponent<SpriteRenderer>();
    }

    public void GoalComplete()
    {
        if (!completed)
        {
            spriteR.sprite = greenFlag;
            AudioPlayer.instance.PlaySound(completeSound);
            completed = true;
        }
    }
}
