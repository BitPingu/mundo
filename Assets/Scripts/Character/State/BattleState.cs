using UnityEngine;
using UnityEngine.UI;

public class BattleState : StateBase
{
    private FighterBase _fighter;

    // pass in any parameters you need in the constructors
    public BattleState(FighterBase fighter, StateMachine stateMachine) : base(fighter, stateMachine)
    {
        _fighter = fighter;
        this.stateMachine = stateMachine;
    }

    // code that runs when we first enter the state
    public override void EnterState()
    {
        base.EnterState();

        // battle anim
        if (_fighter.GetComponent<PartyBase>())
            _fighter.Anim.SetLayerWeight(1, 1);

        // show health bar
        _fighter.HBar.gameObject.GetComponent<Image>().enabled = true;
    }

    // code that runs when we exit the state
    public override void ExitState()
    {
        base.ExitState();

        // battle anim
        if (_fighter.GetComponent<PartyBase>())
            _fighter.Anim.SetLayerWeight(1, 0);

        // hide health bar
        _fighter.HBar.gameObject.GetComponent<Image>().enabled = false;
    }

    public override void FrameUpdate()
    {
        base.FrameUpdate();

        // battle
        if (_fighter.Opponents.Count > 0)
            _fighter.Battle();

        // exit battle
        if (_fighter.Opponents.Count == 0)
            character.StateMachine.ChangeState(character.IdleState);

        // death
        if (_fighter.CurrentHealth <= 0)
            character.StateMachine.End();
    }
}

