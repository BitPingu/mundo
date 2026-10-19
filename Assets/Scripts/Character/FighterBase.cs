using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FighterBase : CharacterBase
{
    // battle state
    public StateBase BattleState { get; set; }
    public FighterBase CurrentOpponent { get; set; }
    public List<FighterBase> Opponents = new List<FighterBase>();
    public List<FighterBase> Allies = new List<FighterBase>();

    // stats
    public int Level = 1;
    public int MaxHealth = 20;
    public int CurrentHealth { get; set; }
    public int Strength = 4;

    // combat arts
    public List<CombatArt> Arts = new List<CombatArt>();

    // conditions
    public bool BattleTurn { get; set; }
    public bool IsAttacking { get; set; }
    public bool FinishTurn { get; set; }

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

    public virtual IEnumerator Engage(FighterBase opponent)
    {
        yield return null;
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

    protected virtual void EndTurn()
    {
        
    }

    protected IEnumerator CallAttack()
    {
        // battle dialogue
        string text = charName + " attacks " + CurrentOpponent.charName + "!";
        DialogueController.Instance.BattleDialogue(this, text, "");

        // attack
        yield return StartCoroutine(Attack("Attack", 1f, .1f, .2f));
        yield return new WaitForSeconds(.5f);
        StartCoroutine(AttackFinish());
    }

    protected IEnumerator CallChargeAttack(CombatArt art)
    {
        // battle dialogue
        string text = charName + " used " + art.artName + " on " + CurrentOpponent.charName + "!";
        DialogueController.Instance.BattleDialogue(this, text, "");

        // attack
        yield return StartCoroutine(Attack("Charge Attack", art.multiplier, art.delay1, art.delay2));
        StartCoroutine(AttackFinish());
    }

    protected IEnumerator Attack(string type, float multiplier, float delay1, float delay2)
    {
        IsAttacking = true;

        // go to opponent
        float _distance = Vector2.Distance(CurrentOpponent.transform.position, transform.position);
        while (_distance > 0.5f)
        {
            _distance = Vector2.Distance(CurrentOpponent.transform.position, transform.position);
            Move(CurrentOpponent.transform.position - transform.position);
            yield return new WaitForFixedUpdate();
        }
        Move(Vector2.zero);

        yield return new WaitForSeconds(.1f);

        Anim.SetTrigger(type);

        yield return new WaitForSeconds(delay1);

        // damage
        CurrentOpponent.Damage(Mathf.RoundToInt(Strength*multiplier));

        yield return new WaitForSeconds(delay2);

        Vector3 attackDir = (CurrentOpponent.transform.position - transform.position).normalized;
        Vector3 attackPos = CurrentOpponent.transform.position - (attackDir*1.3f);

        // return to pos
        _distance = Vector2.Distance(attackPos, transform.position);
        while (_distance > 0.05f)
        {
            _distance = Vector2.Distance(attackPos, transform.position);
            Move(attackPos - transform.position);
            yield return new WaitForFixedUpdate();
        }
        Move(Vector2.zero);

        transform.position = attackPos;

        IsAttacking = false;
    }

    protected virtual IEnumerator AttackFinish()
    {
        if (CurrentOpponent.CurrentHealth == 0)
        {
            CurrentOpponent.Die();
            yield return new WaitForSeconds(2f);

            if (CurrentOpponent.Allies.Count == 0)
            {
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

    protected void PassTurn()
    {
        if (Allies.Count > 0)
        {
            foreach (FighterBase ally in Allies)
            {
                if (ally && ally.CurrentHealth > 0 && !ally.FinishTurn)
                {
                    // ally turn
                    ally.BattleTurn = true;
                    return;
                }
            }
        }

        // reset enemy turn
        foreach (FighterBase opponent in Opponents)
        {
            if (opponent && opponent.CurrentHealth > 0)
            {
                opponent.FinishTurn = false;
            }
        }

        foreach (FighterBase opponent in Opponents)
        {
            if (opponent && !opponent.FinishTurn)
            {
                // enemy turn
                opponent.BattleTurn = true;
                break;
            }
        }
    }

    protected void ContinueBattle()
    {
        // more opponents (continue battle)
        foreach (FighterBase opponent in CurrentOpponent.Allies)
        {
            // target remaining opponents
            if (opponent)
            {
                SetCurrentOpponent(opponent);
                foreach (FighterBase ally in Allies)
                {
                    ally.SetCurrentOpponent(opponent);
                }
                break;
            }
        }
    }

    protected virtual void EndBattle()
    {
        // no more opponents (end battle)
        Opponents.Clear();
        foreach (FighterBase ally in Allies)
            ally.Opponents.Clear();

        // reset conditions
        FinishTurn = false;
        foreach (FighterBase ally in Allies)
            ally.FinishTurn = false;

        // end battle dialogue
        DialogueController.Instance.EndBattleDialogue();

        // reset anim
        Anim.Rebind();
        foreach (FighterBase ally in Allies)
            ally.Anim.Rebind();

        // camera
        CameraController.Instance.follow = true;
    }

    public virtual void Damage(int damageAmount)
    {
        // prevent negative
        int damageTaken = Mathf.Min(damageAmount, CurrentHealth);

        // take damage
        CurrentHealth -= damageTaken;

        // damage flash
        CallDamageFlash();

        // update health bar
        StartCoroutine(HBar.UpdateBar(MaxHealth, CurrentHealth));

        // battle dialogue
        string text = charName + " took " + damageTaken + " damage.";
        DialogueController.Instance.BattleDialogue(this, text, "");
        // Debug.Log("Current health: " + CurrentHealth + "/" + MaxHealth);
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

    public virtual void Die()
    {
        foreach (FighterBase ally in Allies)
            ally.Allies.Remove(this);
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
        StartCoroutine(HBar.UpdateBar(MaxHealth, CurrentHealth));

        // Debug.Log(name + " restored " + healAmount + " health.");
        // Debug.Log("Current health: " + CurrentHealth + "/" + MaxHealth);
    }
}

