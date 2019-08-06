using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Switch : DestroyableEntity
{
    public enum SwitchType
    {
        Appearing,
        Disappearing
    }

    public SwitchType type;
    public List<VanishingEntity> vanishingEntities;

    public AudioClip switchNoise;

    private void Start()
    {
        ResetSwitch();
    }

    private void ResetSwitch()
    {
        switch (type)
        {
            case SwitchType.Appearing:
                foreach (VanishingEntity v in vanishingEntities)
                {
                    v.Disappear();
                }
                break;
            case SwitchType.Disappearing:
                foreach (VanishingEntity v in vanishingEntities)
                {
                    v.Appear();
                }
                break;
        }
    }

    public override void Consume()
    {
        switch(type)
        {
            case SwitchType.Appearing:
                foreach(VanishingEntity v in vanishingEntities)
                {
                    v.Appear();
                }
                AudioPlayer.instance.PlaySound(switchNoise);
                break;
            case SwitchType.Disappearing:
                foreach(VanishingEntity v in vanishingEntities)
                {
                    v.Disappear();
                }
                AudioPlayer.instance.PlaySound(switchNoise);
                break;
        }
        base.Consume();
    }

    public override void Reset()
    {
        ResetSwitch();
        base.Reset();
    }
}
