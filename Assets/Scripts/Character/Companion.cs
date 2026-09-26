using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Companion : PartyBase
{
    public Player Leader { get; set; }

    [SerializeField] private float _followDistance = 1f;
    [SerializeField] private Dialogue _levelDialogue;
    public Dialogue CurAfterBattleDialogue { get; set; }
    public bool SpeakAfterBattle { get; set; }

    protected override void Start()
    {
        // call base class
        base.Start();

        // set dialogue delegates
        DialogueController.Instance.OnBattleDialogueFinish += AfterBattle;
    }

    public override void Idle()
    {
        // call base class
        base.Idle();

        if (Leader && !IsAttacking)
        {
            Follow();
        }
    }

    public void Join(Player player)
    {
        player.Party.Add(this);
        player.Allies.Add(this);
        Party.Add(player);
        Allies.Add(player);
        Leader = player;
        Debug.Log(name + " joins the party!");
    }

    public void Leave(Player player)
    {
        player.Party.Remove(this);
        player.Allies.Remove(this);
        Party.Remove(player);
        Allies.Remove(player);
        Leader = null;
        Debug.Log(name + " left the party!");
    }


    private void Follow()
    {
        float distance = Vector2.Distance(Leader.transform.position, transform.position);
        if (distance > _followDistance)
            Move(Leader.transform.position - transform.position);
        // Stop moving when within range
        if (distance < _followDistance-.2f)
            Move(Vector2.zero);
    }

    public IEnumerator Engage(Enemy enemy, Vector3 playerAttackPos, Vector3 enemyPos, float distanceFromEnemy)
    {
        // get angle of player attack position in radians
        Vector2 offsetFromCenter = playerAttackPos - enemyPos;
        float currentAngle = Mathf.Atan2(offsetFromCenter.y, offsetFromCenter.x);

        // add offset in radians
        float angleOffsetDegrees = 45f;
        float newAngle = currentAngle + (angleOffsetDegrees * Mathf.Deg2Rad);

        // new position on circumference
        float targetX = enemyPos.x + (distanceFromEnemy * Mathf.Cos(newAngle));
        float targetY = enemyPos.y + (distanceFromEnemy * Mathf.Sin(newAngle));
        Vector3 attackPos = new Vector3(targetX, targetY);

        IsAttacking = true;
        
        // move to point
        float distance = Vector2.Distance(attackPos, transform.position);
        while (distance > 0.1f)
        {
            distance = Vector2.Distance(attackPos, transform.position);
            Move(attackPos - transform.position);
            yield return new WaitForFixedUpdate();
        }

        IsAttacking = false;

        foreach (Enemy opponent in Leader.Opponents)
        {
            Opponents.Add(opponent);
        }
    }

    private void AfterBattle()
    {
        if (SpeakAfterBattle)
            StartCoroutine(AfterBattleDialogue());
    }

    private IEnumerator AfterBattleDialogue()
    {
        Dialogue dialogue = CurAfterBattleDialogue;
        if (LeveledUp)
        {
            dialogue = _levelDialogue;
            LeveledUp = false;
        }
        yield return new WaitForSeconds(1f);
        SecondaryDialogueController.Instance.StartDialogue(dialogue, new List<CharacterBase>{this});
    }
}

