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
    [SerializeField] private int _maxAP = 3;
    private int _currentAP;
    [SerializeField] private int _maxExp = 100;
    private int _currentExp = 0;

    // UI
    public Bar ABar, EBar;
    public GameObject BattleHUD;

    private bool _battlePrompt, _noAP, _targeting, _attack, _chargeAttack;
    protected bool _lowHealth;
    public bool DisableAttacks { get; set; }
    public bool DisableArts { get; set; }
    public bool DisableSwitch { get; set; }

    [SerializeField] protected Dialogue _levelDialogue, _lowHealthDialogue;

    protected override void Start()
    {
        // call base class
        base.Start();

        // set components
        _input = GetComponent<PlayerController>();

        // set parameters
        _currentAP = _maxAP;

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
            // battle dialogue
            if (!_battlePrompt)
            {
                // show HUD
                BattleHUD.SetActive(true);
                foreach (Transform child in BattleHUD.transform)
                {
                    child.gameObject.SetActive(true);
                }
                BattlePrompt("What will " + charName + " do?");
                _battlePrompt = true;

                if (DisableAttacks)
                    BattleHUD.transform.GetChild(0).gameObject.SetActive(false);
                if (DisableArts)
                    BattleHUD.transform.GetChild(1).gameObject.SetActive(false);
            }

            // attack
            if (_input.E && !_chargeAttack && !DisableAttacks)
            {
                if (CurrentOpponent.Allies.Count > 0 && !_targeting)
                {
                    // multiple enemies
                    BattlePrompt("Target which one?");
                    _targeting = true;
                    _attack = true;
                    BattleHUD.transform.GetChild(1).gameObject.SetActive(false);
                }
                else
                {
                    StartCoroutine(CallAttack(1f, .1f, .2f));
                    EndTurn();
                    _attack = false;
                }
            }

            // combat art
            if (_input.R && !_attack && !DisableArts)
            {
                if (_currentAP == 0)
                {
                    StartCoroutine(NoAP());
                    _noAP = true;
                }
                else if (!_chargeAttack)
                {
                    BattlePrompt("> " + Arts[0].artName + ": " + Arts[0].description);
                    _chargeAttack = true;
                    BattleHUD.transform.GetChild(0).gameObject.SetActive(false);
                }
                else
                {
                    if (CurrentOpponent.Allies.Count > 0 && !_targeting)
                    {
                        // multiple enemies
                        BattlePrompt("Target which one?");
                        _targeting = true;
                    }
                    else
                    {
                        StartCoroutine(ABar.UpdateBar(_maxAP, --_currentAP));
                        StartCoroutine(CallCombatArt(Arts[0]));
                        EndTurn();
                        _chargeAttack = false;
                    }
                }
            }

            // switch
            if (!_attack && !_chargeAttack && !DisableSwitch && Allies.Count > 0 && !Allies[0].FinishTurn 
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
    }

    private void BattlePrompt(string text)
    {
        if (CurrentHealth <= MaxHealth*.25f)
        {
            DialogueController.Instance.BattleDialogue(this, text, "tired");
        }
        else
        {
            DialogueController.Instance.BattleDialogue(this, text, "show");
        }
    }

    private IEnumerator NoAP()
    {
        if (_noAP)
            yield break;

        BattlePrompt("Not enough AP!");

        yield return new WaitForSeconds(1f);

        if (BattleTurn)
            BattlePrompt("What will " + charName + " do?");

        _noAP = false;
    }

    private IEnumerator SwitchTurn()
    {
        Allies[0].GetComponent<PartyBase>().DisableSwitch = true;

        EndTurn();
        PassTurn();

        yield return new WaitForSeconds(.1f);

        Allies[0].GetComponent<PartyBase>().DisableSwitch = false;
    }

    protected override void EndTurn()
    {
        BattleTurn = false;
        _battlePrompt = false;
        _targeting = false;
        if (CurrentOpponent.target)
            CurrentOpponent.target.enabled = false;
        BattleHUD.SetActive(false);
    }

    protected override IEnumerator AttackFinish()
    {
        if (CurrentOpponent.CurrentHealth == 0)
        {
            int exp = CurrentOpponent.RewardExp;

            CurrentOpponent.Die();
            yield return new WaitForSeconds(2f);

            // exp
            if (exp > 0)
            {
                foreach (PartyBase member in Party)
                {
                    if (member.CurrentHealth > 0)
                        yield return StartCoroutine(member.GainExperience(exp));
                }
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

    protected IEnumerator LowHealthDialogue()
    {
        yield return new WaitForSeconds(1f);
        SecondaryDialogueController.Instance.StartDialogue(_lowHealthDialogue, new List<CharacterBase>{this}, 1.5f);
    }

    public override void Die()
    {
        string text = charName + " was defeated!";
        DialogueController.Instance.BattleDialogue(this, text, "");

        foreach (FighterBase ally in Allies)
            ally.Allies.Remove(this);

        // exit battle
        CurrentOpponent.Opponents.Remove(this);
        foreach (FighterBase opponentAlly in CurrentOpponent.Allies)
            opponentAlly.Opponents.Remove(this);
        Opponents.Clear();

        // set dead anim
        Anim.SetTrigger("Dead");
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
            yield return new WaitForSeconds(1.5f);
            // Debug.Log("Until next level: " + (_maxExp-_currentExp) + " exp");
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

