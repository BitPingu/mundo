using System.Collections;
using UnityEngine;

public class BlueSlime : Enemy
{
    public override void Idle()
    {
        // call base class
        base.Idle();

        if (!CannotMove)
        {
            if (GetComponentInChildren<Chase>().NearbyPlayer)
                Chase(GetComponentInChildren<Chase>().NearbyPlayer);
            else
                Patrol();
        }
        else
        {
            Move(Vector2.zero);
        }
    }

    public override void Battle()
    {
        // call base class
        base.Battle();

        // blue slime turn
        if (BattleTurn)
        {
            if (_artCooldownCounter == 0 && !_chargeAttack)
            {
                // combat art charge
                StartCoroutine(StartChargeAttack());
                EndTurn();
            }
            else if (_artCooldownCounter == 0 && _chargeAttack)
            {
                // combat art release
                TargetOpponent(); // choose target
                StartCoroutine(CallCombatArt(Arts[0]));

                // reset art cooldown time
                _artCooldownCounter = _artCooldownTime+1;
                _chargeAttack = false;
                EndTurn();
            }
            else
            {
                TargetOpponent(); // choose target
                StartCoroutine(CallAttack(1f, .3f, .2f));
                EndTurn();
            }
        }
    }

    private IEnumerator StartChargeAttack()
    {
        _chargeAttack = true;

        // battle dialogue
        string text = charName + " is charging up!";
        DialogueController.Instance.BattleDialogue(this, text, "");

        yield return new WaitForSeconds(1.5f);

        PassTurn();
    }
}
