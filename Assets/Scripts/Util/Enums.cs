using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Layers
{
    Default = 0,
    TransparentFX,
    IgnoreRaycast,
    SolidGround,
    Water,
    UI,
    SnowBuddy,
    DeathBarrier,
    Projectile,
    Enemy,
    Bird,
}

public enum Status
{
    OnGround = 0,
    InAir,
    OnSnow,
}
