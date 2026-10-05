using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Player : PartyBase
{
    // conditions
    public bool CanEnter { get; set; }
    public bool IsEntering { get; set; }
    public bool IsNearEnemy { get; set; }
    public bool IsNearChest { get; set; }

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

    public IEnumerator Engage(Enemy enemy)
    {
        string text = charName + " engages enemy " + enemy.charName + ".";
        DialogueController.Instance.BattleDialogue(this, text, "");

        // add enemies to battle
        Opponents.Add(enemy);
        foreach (Enemy enemyAlly in enemy.Allies)
        {
            Opponents.Add(enemyAlly);
        }

        Vector3 attackDir = (enemy.transform.position - transform.position).normalized;
        float distanceFromEnemy = 1.5f;
        Vector3 attackPos = enemy.transform.position - (attackDir*distanceFromEnemy);

        IsAttacking = true;

        // companion engage
        foreach (Companion ally in Allies)
        {
            if (!ally.IsSparring)
                StartCoroutine(ally.Engage(attackPos, enemy.transform.position, distanceFromEnemy));
        }

        // go to enemy
        float distance = Vector2.Distance(enemy.transform.position, transform.position);
        while (distance > 0.7f)
        {
            distance = Vector2.Distance(enemy.transform.position, transform.position);
            Move(enemy.transform.position - transform.position);
            yield return new WaitForFixedUpdate();
        }

        Anim.SetTrigger("Attack");
        yield return new WaitForSeconds(.1f);
        enemy.CallDamageFlash();

        // trigger battle with enemies
        enemy.Opponents.Add(this);
        foreach (PartyBase ally in Allies)
        {
            enemy.Opponents.Add(ally);
        }
        foreach (Enemy enemyAlly in enemy.Allies)
        {
            enemyAlly.Opponents.Add(this);
            foreach (PartyBase ally in Allies)
            {
                enemyAlly.Opponents.Add(ally);
            }
        }

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

    public IEnumerator Run()
    {
        string text = "Got away safely!";
        DialogueController.Instance.BattleDialogue(this, text, "");

        // end battle
        yield return new WaitForSeconds(1.5f);

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

