using UnityEngine;

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
        else
            Move(Vector2.zero);
    }

    public override void Battle()
    {
        // call base class
        base.Battle();

        // slime turn
        if (BattleTurn)
        {
            TargetOpponent(); // choose target
            StartCoroutine(CallAttack());
            EndTurn();
        }
    }
}

