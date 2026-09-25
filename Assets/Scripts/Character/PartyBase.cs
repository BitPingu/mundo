using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(PlayerController))]
public class PartyBase : FighterBase
{
    public PlayerController Input { get; set; }

    // stats
    [SerializeField] private int _maxExp = 100;
    private int _currentExp = 0;

    // UI
    public Bar EBar;
    public GameObject BattleHUD;

    private bool _battlePrompt;
    public bool IsSparring { get; set; }
    public bool LeveledUp { get; set; }

    protected override void Start()
    {
        // call base class
        base.Start();

        // set components
        Input = GetComponent<PlayerController>();
    }

    public override void Battle()
    {
        // call base class
        base.Battle();

        // attack
        if (BattleTurn)
        {
            if (!IsSparring)
            {
                // show HUD
                BattleHUD.SetActive(true);

                // multiple enemies
                if (CurrentOpponent.Allies.Count > 0)
                {
                    // target icon
                    CurrentOpponent.target.enabled = true;

                    if (Input.Right || Input.Left)
                    {
                        CurrentOpponent.target.enabled = false;

                        // target selected opponent
                        CurrentOpponent = CurrentOpponent.Allies[0];
                        foreach (FighterBase ally in Allies)
                        {
                            ally.SetCurrentOpponent(CurrentOpponent);
                        }

                        CurrentOpponent.target.enabled = true;
                    }
                }

                // battle dialogue
                if (!_battlePrompt)
                {
                    string text = "What will " + charName + " do?";
                    DialogueController.Instance.BattleDialogue(this, text, true);
                    _battlePrompt = true;
                }

                // attack
                if (Input.E)
                {
                    CallAttack();
                    EndTurn();
                }
            }
            else
            {
                // act as enemy
                CallAttack();
                BattleTurn = false;
            }
        }
    }

    protected void EndTurn()
    {
        BattleTurn = false;
        _battlePrompt = false;
        CurrentOpponent.target.enabled = false;
        BattleHUD.SetActive(false);
    }

    public override void Damage(int damageAmount)
    {
        // call base class
        base.Damage(damageAmount);

        if (CurrentHealth <= 0)
        {
            if (IsSparring)
                StartCoroutine(Defeat());
            else
                StartCoroutine(Die());
        }
    }

    public IEnumerator Defeat()
    {
        // for sparring matches
        yield return new WaitForSeconds(1.5f);
        Anim.Rebind();
        Anim.enabled = false;

        string text = charName + " was defeated!";
        DialogueController.Instance.BattleDialogue(this, text, false);
    }

    public override IEnumerator Die()
    {
        yield return new WaitForSeconds(1.5f);

        string text = charName + " was defeated!";
        DialogueController.Instance.BattleDialogue(this, text, false);

        // set dead anim
        Anim.Rebind();
        Anim.SetTrigger("Dead");

        // Die
        foreach (FighterBase ally in Allies)
            ally.Allies.Remove(this);
        CurrentOpponent.Opponents.Remove(this);
        foreach (FighterBase opponent in CurrentOpponent.Allies)
            opponent.Opponents.Remove(this);
    }

    public void GainExperience(int amount)
    {
        _currentExp += amount;

        string text = charName + " gained " + amount + " exp.";
        DialogueController.Instance.BattleDialogue(this, text, false);

        StartCoroutine(ShowEBar());

        if (_currentExp >= _maxExp)
        {
            StartCoroutine(LevelUp());
        }
        else
        {
            Debug.Log("Until next level: " + (_maxExp-_currentExp) + " exp");
        }
    }

    private IEnumerator ShowEBar()
    {
        EBar.gameObject.GetComponent<Image>().enabled = true;

        // update health bar
        EBar.UpdateBar(_maxExp, _currentExp);
        yield return new WaitForSeconds(1.5f);

        EBar.gameObject.GetComponent<Image>().enabled = false;
    }

    private IEnumerator LevelUp()
    {
        LeveledUp = true;

        // increase level
        Level++;

        // update stats
        MaxHealth = Level * 20;
        CurrentHealth = MaxHealth;
        HBar.UpdateBar(MaxHealth, CurrentHealth);
        Strength = Level * 3 + 3;

        // reset exp
        _currentExp = 0;
        _maxExp = Level * 100;

        yield return new WaitForSeconds(.3f);

        // glow up effect
        Sprite.material.SetColor("_FlashColor", new Color(0, 243, 255));
        CallDamageFlash();

        yield return new WaitForSeconds(.5f);

        Sprite.material.SetColor("_FlashColor", Color.white);

        EBar.UpdateBar(_maxExp, _currentExp);

        string text = charName + " grew to level " + Level + "!";
        DialogueController.Instance.BattleDialogue(this, text, false);

        yield return new WaitForSeconds(1.5f);
    }
}

