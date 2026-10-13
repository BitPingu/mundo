using UnityEngine;
using System.Collections;

public class Slime : Enemy
{
    protected override void Awake()
    {
        // call base class
        base.Awake();
    }

    public override void Idle()
    {
        // call base class
        base.Idle();

        if (!CannotMove)
            Patrol();
    }
}

