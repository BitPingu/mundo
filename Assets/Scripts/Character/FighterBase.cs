using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FighterBase : CharacterBase
{
    // battle state
    public StateBase BattleState { get; set; }
    protected FighterBase CurrentOpponent { get; set; }
    public List<FighterBase> Opponents = new List<FighterBase>();
    public List<FighterBase> Allies = new List<FighterBase>();
    public int CurrentTurn { get; set; }

    // stats
    public int Level = 1;
    public int MaxHealth = 20;
    public int CurrentHealth { get; set; }
    public int Strength = 4;

    // public List<Ability> Abilities;

    // conditions
    public bool BattleTurn { get; set; }
    public bool IsAttacking { get; set; }
    public bool WinBattle { get; set; }

    // UI
    public Bar HBar;
    public SpriteRenderer target;

    [SerializeField] private AudioClip _hitSound;

    protected override void Start()
    {
        // call base class
        base.Start();

        // set parameters
        CurrentHealth = MaxHealth;

        // states
        BattleState = new BattleState(this, StateMachine);
    }

    public void SetCurrentOpponent(FighterBase opponent)
    {
        CurrentOpponent = opponent;
    }

    public virtual void Battle()
    {
        // face opponent
        if (CurrentOpponent)
            Face(CurrentOpponent);

        // idle when not attacking
        if (!IsAttacking)
            Move(Vector2.zero);
    }

    protected virtual void CallAttack()
    {
        // battle dialogue
        string text = charName + " attacks " + CurrentOpponent.charName + "!";
        DialogueController.Instance.BattleDialogue(this, text, false);

        StartCoroutine(Attack());
    }

    protected virtual IEnumerator Attack()
    {
        Vector3 attackDir = (CurrentOpponent.transform.position - transform.position).normalized;
        Vector3 attackPos = CurrentOpponent.transform.position - (attackDir*1.5f);

        IsAttacking = true;

        // go to opponent
        float _distance = Vector2.Distance(CurrentOpponent.transform.position, transform.position);
        while (_distance > 0.7f)
        {
            _distance = Vector2.Distance(CurrentOpponent.transform.position, transform.position);
            Move(CurrentOpponent.transform.position - transform.position);
            yield return new WaitForFixedUpdate();
        }

        Anim.SetTrigger("Attack");
        yield return new WaitForSeconds(.1f);

        // damage
        CurrentOpponent.Damage(Strength);

        // return to pos
        _distance = Vector2.Distance(attackPos, transform.position);
        while (_distance > 0.1f)
        {
            _distance = Vector2.Distance(attackPos, transform.position);
            Move(attackPos - transform.position);
            yield return new WaitForFixedUpdate();
        }

        Move(Vector2.zero);

        IsAttacking = false;

        StartCoroutine(AttackFinish());
    }

    private IEnumerator AttackFinish()
    {
        yield return new WaitForSeconds(1f);

        if (GetComponent<PartyBase>() && CurrentOpponent.CurrentHealth == 0)
        {
            yield return new WaitForSeconds(2f);

            // exp
            List<FighterBase> party = CurrentOpponent.Opponents;
            foreach (PartyBase ally in party)
            {
                ally.GainExperience(40);
                yield return new WaitForSeconds(1.5f);
                if (ally.LeveledUp)
                    yield return new WaitForSeconds(1.5f);
            }

            if (CurrentOpponent.Allies.Count == 0)
            {
                // no more opponents (end battle)
                Opponents.Clear();
                foreach (FighterBase ally in Allies)
                    ally.Opponents.Clear();
                
                // end battle dialogue
                DialogueController.Instance.EndBattleDialogue();
                yield break;
            }
            else
            {
                // more opponents (continue battle)
                foreach (FighterBase opponent in CurrentOpponent.Allies)
                {
                    // target remaining opponents
                    if (opponent)
                    {
                        CurrentOpponent = opponent;
                        foreach (FighterBase ally in Allies)
                        {
                            ally.SetCurrentOpponent(opponent);
                        }
                        break;
                    }
                }
            }
        }

        if (Allies.Count > 0 && CurrentTurn < Allies.Count && !WinBattle)
        {
            // ally turn
            foreach (FighterBase ally in Allies)
            {
                if (ally && ally.CurrentHealth > 0)
                {
                    ally.CurrentTurn = CurrentTurn+1;
                    ally.BattleTurn = true;
                    break;
                }
            }
        }
        else
        {
            // enemy turn
            foreach (FighterBase opponent in Opponents)
            {
                if (opponent && opponent.CurrentHealth > 0)
                {
                    opponent.CurrentTurn = 0;
                    opponent.BattleTurn = true;
                    break;
                }
            }
        }
    }

    public virtual void Damage(int damageAmount)
    {
        // take damage
        CurrentHealth -= damageAmount;

        // damage flash
        CallDamageFlash();

        if (CurrentHealth <= 0)
        {
            damageAmount -= -CurrentHealth;
            CurrentHealth = 0;
        }

        // update health bar
        HBar.UpdateBar(MaxHealth, CurrentHealth);

        // battle dialogue
        string text = charName + " took " + damageAmount + " damage.";
        DialogueController.Instance.BattleDialogue(this, text, false);
        Debug.Log("Current health: " + CurrentHealth + "/" + MaxHealth);
    }

    public void CallDamageFlash()
    {
        // damage sound
        SFXManager.Play(_hitSound);
        StartCoroutine(DamageFlash());
    }

    private IEnumerator DamageFlash()
    {
        float flashTime = .25f;

        float currentFlashAmount;
        float elapsedTime = 0f;
        while (elapsedTime < flashTime)
        {
            elapsedTime += Time.deltaTime;
            // lerp flash amount
            currentFlashAmount = Mathf.Lerp(1f, 0f, elapsedTime / flashTime);
            // set flash amount
            Sprite.material.SetFloat("_FlashAmount", currentFlashAmount);
            yield return null;
        }
    }

    public virtual IEnumerator Die()
    {
        yield return null;
    }

    public void Heal(int healAmount)
    {
        // restore health
        CurrentHealth += healAmount;

        if (CurrentHealth > MaxHealth)
        {
            healAmount -= CurrentHealth - MaxHealth;
            CurrentHealth = MaxHealth;
        }

        // update health bar
        HBar.UpdateBar(MaxHealth, CurrentHealth);

        Debug.Log(name + " restored " + healAmount + " health.");
        Debug.Log("Current health: " + CurrentHealth + "/" + MaxHealth);
    }
}

