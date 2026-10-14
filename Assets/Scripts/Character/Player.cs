using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Player : PartyBase
{
    // conditions
    public bool CannotEnter { get; set; }
    public bool IsEntering { get; set; }
    public bool IsNearEnemy { get; set; }
    public bool IsNearInteractable { get; set; }

    public delegate void Enter();
    public Enter OnEnter;

    public bool StatusOn { get; set; }
    [SerializeField] private GameObject _inventoryBox;

    protected override void Start()
    {
        // call base class
        base.Start();
    }

    protected override void Update()
    {
        // call base class
        base.Update();

        // if (Input.Q && StateMachine.CurrentState == IdleState && !DialogueController.Instance.IsDialogueActive)
        // {
        //     StateMachine.End(); // disable movement
        //     StatusOn = true;
        //     CheckStatus();
        // }
        // else if (Input.Q && StatusOn)
        // {
        //     StateMachine.Initialize(IdleState); // enable movement
        //     StatusOn = false;
        //     CheckStatus();
        // }
    }

    public override void Idle()
    {
        // call base class
        base.Idle();

        // allow player movement
        Move(new Vector2(Input.HorizontalInput, Input.VerticalInput));
    }

    public override IEnumerator Engage(FighterBase opponent)
    {
        opponent.GetComponent<Enemy>().CannotMove = true; // halt enemy movement

        string text = charName + " engages enemy " + opponent.charName + ".";
        DialogueController.Instance.BattleDialogue(this, text, "");

        // add opponents to battle
        Opponents.Add(opponent);
        foreach (FighterBase opponentAlly in opponent.Allies)
        {
            Opponents.Add(opponentAlly);
        }

        // set current opponent
        SetCurrentOpponent(opponent);

        Vector3 attackDir = (opponent.transform.position - transform.position).normalized;
        float distanceFromEnemy = 1.5f;
        Vector3 attackPos = opponent.transform.position - (attackDir*distanceFromEnemy);

        // companion engage
        foreach (Companion ally in Allies)
        {
            if (!ally.IsSparring)
                yield return StartCoroutine(ally.Engage(attackPos, opponent.transform.position, distanceFromEnemy));
        }

        IsAttacking = true;

        // go to opponent
        float distance = Vector2.Distance(opponent.transform.position, transform.position);
        while (distance > 0.7f)
        {
            distance = Vector2.Distance(opponent.transform.position, transform.position);
            Move(opponent.transform.position - transform.position);
            yield return new WaitForFixedUpdate();
        }

        Anim.SetTrigger("Attack");
        yield return new WaitForSeconds(.1f);
        opponent.CallDamageFlash();

        // trigger battle with opponents
        opponent.Opponents.Add(this);
        foreach (PartyBase ally in Allies)
        {
            opponent.Opponents.Add(ally);
        }
        foreach (Enemy enemyAlly in opponent.Allies)
        {
            enemyAlly.Opponents.Add(this);
            foreach (PartyBase ally in Allies)
            {
                enemyAlly.Opponents.Add(ally);
            }
        }

        // set enemy current opponent
        opponent.SetCurrentOpponent(this);

        // return to pos
        distance = Vector2.Distance(attackPos, transform.position);
        while (distance > 0.1f)
        {
            distance = Vector2.Distance(attackPos, transform.position);
            Move(attackPos - transform.position);
            yield return new WaitForFixedUpdate();
        }

        IsAttacking = false;

        // Player goes first
        BattleTurn = true;
    }

    public override void Battle()
    {
        // call base class
        base.Battle();

        // player only battle options go here

        if (BattleTurn)
        {
            // run (if wild)
            // if (Opponent.GetComponent<Enemy>())
            // {
            //     BattleHUD.transform.Find("RunImage").gameObject.SetActive(true);
            //     if (Input.Q)
            //     {
            //         StartCoroutine(Run());
            //         EndTurn();
            //     }
            // }
        }
    }

    public IEnumerator Run()
    {
        string text = "Got away safely!";
        DialogueController.Instance.BattleDialogue(this, text, "");

        // end battle
        yield return new WaitForSeconds(1f);

        // TODO: add random chance of working
        foreach (Enemy opponent in Opponents)
        {
            opponent.Opponents.Clear();
        }

        Opponents.Clear();
        foreach (Companion ally in Allies)
        {
            ally.Opponents.Clear();
        }

        // end battle dialogue
        DialogueController.Instance.EndBattleDialogue();
    }

    public void CheckStatus()
    {
        if (StatusOn)
        {
            // show bars
            HBar.gameObject.GetComponent<Image>().enabled = true;
            EBar.gameObject.GetComponent<Image>().enabled = true;
            if (Allies[0])
            {
                Allies[0].HBar.gameObject.GetComponent<Image>().enabled = true;
                Allies[0].GetComponent<Companion>().EBar.gameObject.GetComponent<Image>().enabled = true;
            }

            // inventory
            _inventoryBox.SetActive(true);

            if (Input.E)
            {
                // TODO: need to make this more dynamic
                ItemBase item = InventoryController.Instance.GetItem("Potion");
                if (item is Consumable)
                {
                    Debug.Log(name + " used a " + item.name + ".");
                    Heal(((Consumable)item).amount);
                }
                else
                {
                    Debug.Log("There are no consumable items.");
                }
            }
        }
        else if (!StatusOn && StateMachine.CurrentState == IdleState)
        {
            // hide bars
            HBar.gameObject.GetComponent<Image>().enabled = false;
            EBar.gameObject.GetComponent<Image>().enabled = false;
            if (Allies[0])
            {
                Allies[0].HBar.gameObject.GetComponent<Image>().enabled = false;
                Allies[0].GetComponent<Companion>().EBar.gameObject.GetComponent<Image>().enabled = false;
            }

            // inventory
            _inventoryBox.SetActive(false);
        }
    }
}

