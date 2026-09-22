using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeadToMatch : EventBase
{
    private Player _detectPlayer;
    public Player PlayerChar { get; set; }
    public Companion Friend { get; set; }
    public Villager Chief { get; set; }
    [SerializeField] private Dialogue _curFriendDialogue, _targetDialogue, _outBoundsDialogue;
    private bool _entered, _reached;
    private int _inPos;

    private void Start()
    {
        // set dialogue delegates
        DialogueController.Instance.OnDialogueFinish += OutOfBounds;
        PlayerChar.OnEnter += EnterBuilding;
        DialogueController.Instance.OnDialogueFinish += ExitBuilding;
        DialogueController.Instance.OnDialogueFinish += FinishEvent;

        // set current dialogue
        Friend.CurrentDialogue = _curFriendDialogue;

        // boundary spawn
        transform.position = new Vector3(9.45f,-3.07f);
    }

    private void OnTriggerEnter2D(Collider2D hitInfo)
    {
        if (hitInfo.GetComponent<Player>())
        {
            _detectPlayer = null;
        }
    }

    private void OnTriggerExit2D(Collider2D hitInfo)
    {
        if (hitInfo.GetComponent<Player>() && !_detectPlayer)
        {
            _detectPlayer = hitInfo.GetComponent<Player>();
        }
    }

    private void Update()
    {
        // setup before next event
        if (_inPos == 2)
        {
            _inPos = -1;

            Friend.Anim.Rebind();
            Friend.Anim.enabled = false;

            EventIsDone = true; // event done

            DialogueController.Instance.OnDialogueFinish -= OutOfBounds;
            PlayerChar.OnEnter -= EnterBuilding;
            DialogueController.Instance.OnDialogueFinish -= ExitBuilding;
            DialogueController.Instance.OnDialogueFinish -= FinishEvent;
        }

        // target reached
        float chiefDistance = Vector2.Distance(Chief.transform.position, PlayerChar.transform.position);
        if (!_reached && chiefDistance < 1.5f)
        {
            PlayerChar.StateMachine.End(); // stop movement
            Friend.StateMachine.End(); // stop movement
            
            Friend.Anim.Rebind();
            Friend.Anim.enabled = false;

            // start dialogue
            DialogueController.Instance.StartDialogue(_targetDialogue, new List<CharacterBase>{Friend});

            _reached = true;
        }

        // out of bounds check
        if (_detectPlayer && _detectPlayer.StateMachine.CurrentState == _detectPlayer.IdleState)
        {
            PlayerChar.StateMachine.End(); // stop movement

            Friend.Face(PlayerChar);
            Friend.Anim.Rebind();
            Friend.Anim.enabled = false;

            // start dialogue
            DialogueController.Instance.StartDialogue(_outBoundsDialogue, new List<CharacterBase>{Friend});
        }
    }

    private void OutOfBounds()
    {
        if (!_detectPlayer)
            return;

        Friend.Anim.enabled = true;
        StartCoroutine(GoBack());
    }

    private IEnumerator GoBack()
    {
        PlayerChar.Face(Friend);

        // go back
        Vector2 vec = Friend.transform.position - PlayerChar.transform.position;
        
        PlayerChar.Move(vec);

        yield return new WaitForSeconds(.5f);

        PlayerChar.StateMachine.Initialize(PlayerChar.IdleState); // enable movement
        
        _detectPlayer = null;
    }

    private void EnterBuilding()
    {
        // enter building check
        PlayerChar.StateMachine.End(); // stop movement

        Friend.Face(PlayerChar);
        Friend.Anim.Rebind();
        Friend.Anim.enabled = false;

        // start dialogue
        DialogueController.Instance.StartDialogue(_outBoundsDialogue, new List<CharacterBase>{Friend});

        _entered = true;
    }

    private void ExitBuilding()
    {
        if (!_entered)
            return;

        _entered = false;
        Friend.Anim.enabled = true;
        PlayerChar.StateMachine.Initialize(PlayerChar.IdleState); // enable movement
    }

    private void FinishEvent()
    {
        if (!_reached)
            return;

        Friend.Anim.enabled = true;

        // before battle
        StartCoroutine(GoToBattle(PlayerChar, new Vector2(Chief.transform.position.x+.8f, Chief.transform.position.y-1f)));
        StartCoroutine(GoToBattle(Friend, new Vector2(Chief.transform.position.x-.8f, Chief.transform.position.y-1f)));
    }

    private IEnumerator GoToBattle(CharacterBase character, Vector3 destination)
    {
        // move to battle position
        float distance = Vector2.Distance(destination, character.transform.position);
        Vector2 vec = destination - character.transform.position;
        while (distance > 0.1f)
        {
            distance = Vector2.Distance(destination, character.transform.position);
            character.Move(vec);
            yield return new WaitForFixedUpdate();
        }

        character.Move(Vector2.zero);
        character.Face(Chief);

        _inPos++;
    }
}
