using System.Collections;
using UnityEngine;

public class BlueSlime : Enemy
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
        {
            if (GetComponentInChildren<Chase>().NearbyPlayer)
                Chase(GetComponentInChildren<Chase>().NearbyPlayer);
            else
                Patrol();
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
                StartCoroutine(CallChargeAttack());

                // reset art cooldown time
                _artCooldownCounter = _artCooldownTime+1;
                _chargeAttack = false;
                EndTurn();
            }
            else
            {
                TargetOpponent(); // choose target
                StartCoroutine(CallAttack());
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

        yield return new WaitForSeconds(2f);

        PassTurn();
    }

    protected override IEnumerator CallChargeAttack()
    {
        // battle dialogue
        string text = charName + " used Body Slam on " + CurrentOpponent.charName + "!";
        DialogueController.Instance.BattleDialogue(this, text, "");

        // attack
        yield return StartCoroutine(Attack("Charge Attack"));
        StartCoroutine(AttackFinish());
    }
}
