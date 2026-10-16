using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(PlayerController))]
public class PartyBase : FighterBase
{
    public List<PartyBase> Party = new List<PartyBase>();

    protected PlayerController _input { get; set; }

    // stats
    [SerializeField] private int _maxExp = 100;
    private int _currentExp = 0;

    // UI
    public Bar EBar;
    public GameObject BattleHUD;

    private bool _battlePrompt, _targeting, _lowHealth;
    public bool IsSparring { get; set; }
    protected bool _cannotSwitch { get; set; }

    [SerializeField] protected Dialogue _levelDialogue, _lowHealthDialogue;

    protected override void Start()
    {
        // call base class
        base.Start();

        // set components
        _input = GetComponent<PlayerController>();

        // add to party
        Party.Add(this);
    }

    public override void Battle()
    {
        // call base class
        base.Battle();

        // party member turn
        if (BattleTurn)
        {
            if (!IsSparring)
            {
                // show HUD
                BattleHUD.SetActive(true);

                // battle dialogue
                if (!_battlePrompt)
                {
                    string text = "What will " + charName + " do?";
                    if (CurrentHealth <= MaxHealth*.25f)
                    {
                        DialogueController.Instance.BattleDialogue(this, text, "tired");
                    }
                    else
                    {
                        DialogueController.Instance.BattleDialogue(this, text, "show");
                    }
                    _battlePrompt = true;
                }

                // attack
                if (_input.E)
                {
                    if (CurrentOpponent.Allies.Count > 0 && !_targeting)
                    {
                        // multiple enemies
                        string text = "Target which one?";
                        DialogueController.Instance.BattleDialogue(this, text, "show");
                        _targeting = true;
                    }
                    else
                    {
                        StartCoroutine(CallAttack());
                        EndTurn();
                    }
                }

                // switch
                if (!_targeting && !_cannotSwitch && Allies.Count > 0 && !Allies[0].FinishTurn 
                    && (_input.Right || _input.Left))
                {
                    StartCoroutine(SwitchTurn());
                }

                // target
                if (_targeting)
                {
                    // target icon
                    CurrentOpponent.target.enabled = true;
                    if (_input.Right || _input.Left)
                    {
                        CurrentOpponent.target.enabled = false;

                        // target selected opponent
                        SetCurrentOpponent(CurrentOpponent.Allies[0]);
                        foreach (FighterBase ally in Allies)
                        {
                            ally.SetCurrentOpponent(CurrentOpponent);
                        }

                        CurrentOpponent.target.enabled = true;
                    }
                }
            }
            else
            {
                // act as enemy
                StartCoroutine(CallAttack());
                BattleTurn = false;
            }
        }
    }

    private IEnumerator SwitchTurn()
    {
        Allies[0].GetComponent<PartyBase>()._cannotSwitch = true;

        EndTurn();
        PassTurn();

        yield return new WaitForSeconds(.1f);

        Allies[0].GetComponent<PartyBase>()._cannotSwitch = false;
    }

    protected override void EndTurn()
    {
        BattleTurn = false;
        _battlePrompt = false;
        _targeting = false;
        CurrentOpponent.target.enabled = false;
        BattleHUD.SetActive(false);
    }

    protected override IEnumerator AttackFinish()
    {
        if (CurrentOpponent.CurrentHealth == 0)
        {
            CurrentOpponent.Die();
            yield return new WaitForSeconds(2f);

            // exp
            foreach (PartyBase member in Party)
            {
                if (member.CurrentHealth > 0)
                    yield return StartCoroutine(member.GainExperience(40));
            }

            if (CurrentOpponent.Allies.Count == 0)
            {
                Revive(); // revive fallen members
                EndBattle();
                yield break;
            }
            else
            {
                ContinueBattle();
            }
        }

        FinishTurn = true;
        PassTurn();
    }

    public override void Damage(int damageAmount)
    {
        // call base class
        base.Damage(damageAmount);

        if (!_lowHealth && CurrentHealth <= MaxHealth*.25f && !IsSparring)
        {
            _lowHealth = true;
            StartCoroutine(LowHealthDialogue());
        }
    }

    private IEnumerator LowHealthDialogue()
    {
        yield return new WaitForSeconds(1f);
        SecondaryDialogueController.Instance.StartDialogue(_lowHealthDialogue, new List<CharacterBase>{this}, 1.5f);
    }

    public override void Die()
    {
        if (IsSparring)
        {
            StartCoroutine(Defeat());
            return;
        }

        // call base class
        base.Die();

        // exit battle
        CurrentOpponent.Opponents.Remove(this);
        foreach (FighterBase opponentAlly in CurrentOpponent.Allies)
            opponentAlly.Opponents.Remove(this);
        Opponents.Clear();

        // set dead anim
        Move(Vector2.zero);
        Anim.SetTrigger("Dead");

        string text = charName + " was defeated!";
        DialogueController.Instance.BattleDialogue(this, text, "");
    }

    private IEnumerator Defeat()
    {
        Anim.Rebind();

        // for sparring matches
        yield return new WaitForSeconds(1f);
        Anim.enabled = false;

        string text = charName + " was defeated!";
        DialogueController.Instance.BattleDialogue(this, text, "");
    }

    private void Revive()
    {
        foreach (PartyBase member in Party)
        {
            if (member.CurrentHealth == 0)
            {
                Allies.Add(member);
                member.Anim.SetTrigger("Dead");
                member.Heal(1);
                member.StateMachine.Initialize(member.IdleState);
            }
        }
    }

    public IEnumerator GainExperience(int amount)
    {
        string text = charName + " gained " + amount + " exp.";
        DialogueController.Instance.BattleDialogue(this, text, "");

        _currentExp += amount;

        EBar.GetComponent<Image>().enabled = true;

        // update exp bar
        yield return StartCoroutine(EBar.UpdateBar(_maxExp, _currentExp));

        if (_currentExp >= _maxExp)
        {
            yield return StartCoroutine(LevelUp());
        }
        else
        {
            yield return new WaitForSeconds(1f);
            Debug.Log("Until next level: " + (_maxExp-_currentExp) + " exp");
        }

        EBar.GetComponent<Image>().enabled = false;
    }

    private IEnumerator LevelUp()
    {
        // increase level
        Level++;

        // update stats
        MaxHealth = Level * 20;
        CurrentHealth = MaxHealth;
        Strength = Level * 3 + 3;

        // reset exp
        _currentExp = 0;
        _maxExp = Level * 100;

        yield return new WaitForSeconds(.3f);

        // glow up effect
        Sprite.material.SetColor("_FlashColor", new Color(0f, 0.9529412f, 1f));
        CallDamageFlash();

        yield return new WaitForSeconds(.5f);

        Sprite.material.SetColor("_FlashColor", Color.white);

        StartCoroutine(HBar.UpdateBar(MaxHealth, CurrentHealth));
        StartCoroutine(EBar.UpdateBar(_maxExp, _currentExp));

        string text = charName + " grew to level " + Level + "!";
        DialogueController.Instance.BattleDialogue(this, text, "");

        yield return new WaitForSeconds(2f);

        if (GetComponent<Companion>())
            GetComponent<Companion>().EventDialogue = _levelDialogue;
    }
}

