using UnityEngine;

public class Slime : Enemy
{
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
            StartCoroutine(CallAttack(1f, .3f, .2f));
            EndTurn();
        }
    }
}

