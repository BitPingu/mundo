using System.Collections;
using UnityEngine;

public class Enemy : FighterBase
{
    public float SightRadius;
    public Vector3 SpawnPoint;
    [SerializeField] private float _spawnRange = 3f;
    private Vector2 _curDir, _curPoint;
    private float _moveCounter, _waitCounter, _waitTime;
    [SerializeField] private float _minWaitTime = 3f, _maxWaitTime = 5f;

    protected override void Start()
    {
        // call base class
        base.Start();

        // set spawn
        transform.position = SpawnPoint;

        // set wait time
        _waitCounter = _minWaitTime;
    }

    public void SetRandomDir()
    {
        // choose random point from spawn
        _curPoint = (Vector2)SpawnPoint + Random.insideUnitCircle * _spawnRange;

        // direction to point
        _curDir.x = _curPoint.x - transform.position.x;
        _curDir.y = _curPoint.y - transform.position.y;

        // choose random wait time
        _waitTime = Random.Range(_minWaitTime, _maxWaitTime);
    }

    public void InterruptIdle()
    {
        // stop
        _curDir = Vector2.zero;
    }

    public override void Idle()
    {
        // call base class
        base.Idle();

        Patrol();
    }

    private void Patrol()
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
            InterruptIdle();

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

    public void Chase(CharacterBase opponent)
    {
        // chase opponent
        float distance = Vector2.Distance(opponent.transform.position, transform.position);
        if (distance > SightRadius)
        {
            Move(opponent.transform.position - transform.position);
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

        // attack
        if (BattleTurn)
        {
            // choose target
            // current strat: prioritize opponents <= 50% health
            if (Opponents.Count > 1)
            {
                bool priority = false;
                foreach (FighterBase opponent in Opponents)
                {
                    if (opponent.CurrentHealth <= opponent.MaxHealth / 2)
                    {
                        CurrentOpponent = opponent;
                        priority = true;
                        break;
                    }
                }
                if (!priority && Random.value < .5f)
                {
                    CurrentOpponent = Opponents[1];
                }                
            }

            CallAttack();
            BattleTurn = false;
        }
    }

    public override void Damage(int damageAmount)
    {
        // call base class
        base.Damage(damageAmount);

        if (CurrentHealth <= 0)
        {
            StartCoroutine(Die());      
        }
    }

    public override IEnumerator Die()
    {
        yield return new WaitForSeconds(1.5f);

        string text = "Enemy " + charName + " was defeated!";
        DialogueController.Instance.BattleDialogue(this, text, false);

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

        // Die
        foreach (FighterBase ally in Allies)
            ally.Allies.Remove(this);
        Destroy(gameObject);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position, SightRadius);
    }
}

