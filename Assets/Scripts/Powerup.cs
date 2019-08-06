using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Powerup : DestroyableEntity
{
    public AudioClip consumeSound;

    public PowerupType type;

    public PowerupType GetPowerupType()
    {
        return type;
    }

    public override void Consume()
    {
        AudioPlayer.instance.PlaySound(consumeSound);
        base.Consume();
    }
}
