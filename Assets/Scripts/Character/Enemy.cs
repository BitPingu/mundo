using System.Collections;
using UnityEngine;

public class Enemy : FighterBase
{
    public Vector3 SpawnPoint;
    [SerializeField] private float _spawnRange = 3f;
    private Vector2 _curDir, _curPoint;
    private float _moveCounter, _waitCounter, _waitTime, _artCooldownCounter;
    [SerializeField] private float _minWaitTime = 3f, _maxWaitTime = 5f;
    [SerializeField] private float _artCooldownTime;
    public bool CannotMove { get; set; }
    public bool IsNearPlayer { get; set; }

    protected override void Start()
    {
        // call base class
        base.Start();

        // set spawn
        transform.position = SpawnPoint;

        // set wait time
        _waitCounter = _minWaitTime;

        // set art cooldown time
        _artCooldownCounter = _artCooldownTime;
    }

    private void SetRandomDir()
    {
        // choose random point from spawn
        _curPoint = (Vector2)SpawnPoint + Random.insideUnitCircle * _spawnRange;

        // direction to point
        _curDir.x = _curPoint.x - transform.position.x;
        _curDir.y = _curPoint.y - transform.position.y;

        // choose random wait time
        _waitTime = Random.Range(_minWaitTime, _maxWaitTime);
    }

    protected void Patrol()
    {
        if (_moveCounter > 0)
        {
            // update move counter
            _moveCounter -= Time.deltaTime;

            // stop
            float distance = Vector2.Distance(_curPoint, transform.position);
            if (_moveCounter < 0.1f || distance < .1f)
                _waitCounter = _waitTime;
        }
        else
        {
            // stop
            _curDir = Vector2.zero;

            // update wait counter
            _waitCounter -= Time.deltaTime;

            if (_waitCounter < 0)
            {
                SetRandomDir();

                // reset move time
                _moveCounter = 1f;
            }
        }

        // allow enemy movement
        Move(_curDir);
    }

    protected void Chase(FighterBase opponent)
    {
        // chase opponent
        float distance = Vector2.Distance(opponent.transform.position, transform.position);
        if (distance > 0.7f)
        {
            Move(opponent.transform.position - transform.position);
        }
        else
        {
            // stop
            IsNearPlayer = true;
        }
    }

    public override IEnumerator Engage(FighterBase opponent)
    {
        CannotMove = true; // halt enemy movement

        string text = charName + " engages " + opponent.charName + "!";
        DialogueController.Instance.BattleDialogue(this, text, "");

        // add opponents to battle
        Opponents.Add(opponent);
        foreach (FighterBase opponentAlly in opponent.Allies)
        {
            Opponents.Add(opponentAlly);
        }

        IsAttacking = true;

        Anim.SetTrigger("Attack");
        yield return new WaitForSeconds(.1f);
        opponent.CallDamageFlash();

        Vector3 attackDir = (opponent.transform.position - transform.position).normalized;
        float distanceFromEnemy = 1.5f;
        Vector3 attackPos = opponent.transform.position - (attackDir*distanceFromEnemy);

        // trigger battle with opponents
        opponent.Opponents.Add(this);
        foreach (Companion enemyAlly in opponent.Allies)
        {
            StartCoroutine(enemyAlly.Engage(attackPos, opponent.transform.position, distanceFromEnemy));
        }

        // set player current opponent
        opponent.SetCurrentOpponent(this);

        // return to pos
        float distance = Vector2.Distance(attackPos, transform.position);
        while (distance > 0.1f)
        {
            distance = Vector2.Distance(attackPos, transform.position);
            Move(attackPos - transform.position);
            yield return new WaitForFixedUpdate();
        }

        IsAttacking = false;

        // Enemy goes first
        BattleTurn = true;
    }

    public override void Battle()
    {
        // call base class
        base.Battle();

        // attack
        if (BattleTurn)
        {
            TargetOpponent(); // choose target
            StartCoroutine(CallAttack());
            EndTurn();
        }
    }

    private void TargetOpponent()
    {
        // current strat: prioritize opponents <= 50% health
        if (Opponents.Count > 1)
        {
            foreach (FighterBase opponent in Opponents)
            {
                if (opponent.CurrentHealth <= opponent.MaxHealth / 2)
                {
                    CurrentOpponent = opponent;
                    return;
                }
            }
            if (Random.value < .5f)
            {
                CurrentOpponent = Opponents[0];
            }
            else
            {
                CurrentOpponent = Opponents[1];
            }
        }
        else
        {
            CurrentOpponent = Opponents[0];
        }
    }

    protected override void EndTurn()
    {
        BattleTurn = false;
        if (_artCooldownCounter > 0)
            _artCooldownCounter--;
        Debug.Log("counter: " + _artCooldownCounter);
    }

    protected override void EndBattle()
    {
        // call base class
        base.EndBattle();
        CannotMove = false;
    }

    public override void Die()
    {
        // call base class
        base.Die();

        StartCoroutine(Fade());

        string text = "Enemy " + charName + " was defeated!";
        DialogueController.Instance.BattleDialogue(this, text, "");
    }

    private IEnumerator Fade()
    {
        // set color
        Sprite.material.SetFloat("_FlashAmount", 1f);
        float flashTime = 1.5f;

        float currentFlashAmount;
        float elapsedTime = 0f;
        while (elapsedTime < flashTime)
        {
            elapsedTime += Time.deltaTime;
            // lerp flash amount
            currentFlashAmount = Mathf.Lerp(0f, 1f, (elapsedTime / flashTime));
            // set flash amount
            Sprite.material.SetFloat("_Alpha", currentFlashAmount);
            yield return null;
        }

        Destroy(gameObject);
    }

    // void OnDrawGizmosSelected()
    // {
    //     Gizmos.DrawWireSphere(transform.position, SightRadius);
    // }
}

