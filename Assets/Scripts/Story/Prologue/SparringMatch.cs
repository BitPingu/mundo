using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SparringMatch : EventBase
{
    public Player PlayerChar { get; set; }
    public Companion Friend { get; set; }
    public Villager Chief { get; set; }
    public GameObject House { get; set; }
    public GameObject HouseIndoor { get; set; }
    public Boundary MatchBoundary { get; set; }
    public Destination ChiefTrigger { get; set; }
    [SerializeField] private Dialogue _curFriendDialogue, _targetDialogue, _outBoundsDialogue, _chiefDialogue, 
        _damageDialogue, _chiefDialogue2, _chiefDialogue3, _friendDialogue;
    private bool _entered, _chiefEncounter, _chiefDialogueActive, _friendHurt, _chiefDialogue2Active, _chiefDialogue3Active, 
        _friendDialogueActive;
    private int _inPos;

    private void Start()
    {
        // set dialogue delegates
        DialogueController.Instance.OnDialogueFinish += OutOfBounds;
        PlayerChar.OnEnter += EnterBuilding;
        DialogueController.Instance.OnDialogueFinish += ExitBuilding;
        DialogueController.Instance.OnDialogueFinish += ChiefEncounter;
        DialogueController.Instance.OnDialogueFinish += StartMatch;
        DialogueController.Instance.OnBattleDialogueFinish += FinishMatch;
        DialogueController.Instance.OnDialogueFinish += GiveQuest;
        DialogueController.Instance.OnDialogueFinish += ChiefReturns;
        DialogueController.Instance.OnDialogueFinish += FinishEvent;

        // set current dialogue
        Friend.CurrentDialogue = _curFriendDialogue;

        // disable enter action
        PlayerChar.CanEnter = false;
    }

    private void Update()
    {
        // out of bounds check
        if (MatchBoundary.DetectPlayer && PlayerChar.StateMachine.CurrentState == PlayerChar.IdleState)
        {
            PlayerChar.StateMachine.End(); // stop movement

            Friend.Face(PlayerChar);
            Friend.Anim.Rebind();
            Friend.Anim.enabled = false;

            // start dialogue
            DialogueController.Instance.StartDialogue(_outBoundsDialogue, new List<CharacterBase>{Friend});
        }

        // meet the chief
        if (ChiefTrigger.Reached)
        {
            // approach chief
            if (!_chiefEncounter && !DialogueController.Instance.IsDialogueActive)
            {
                PlayerChar.StateMachine.End(); // stop movement
                Friend.StateMachine.End(); // stop movement
                
                Friend.Anim.Rebind();
                Friend.Anim.enabled = false;

                // start dialogue
                DialogueController.Instance.StartDialogue(_targetDialogue, new List<CharacterBase>{Friend});
                _chiefEncounter = true;
            }

            // setup match
            if (_chiefEncounter && _inPos == 2)
            {
                Debug.Log("setup!");
                _inPos = -1;

                Friend.Anim.Rebind();
                Friend.Anim.enabled = false;

                // set spawn
                PlayerChar.transform.position = new Vector2(Chief.transform.position.x+.8f, Chief.transform.position.y-1f);
                Friend.transform.position = new Vector2(Chief.transform.position.x-.8f, Chief.transform.position.y-1f);

                // end states
                if (PlayerChar.StateMachine.CurrentState != null)
                    PlayerChar.StateMachine.End(); // stop movement
                if (Friend.StateMachine.CurrentState != null)
                    Friend.StateMachine.End(); // stop movement
                
                PlayerChar.Face(Chief);
                Friend.Face(Chief);

                // start chief dialogue
                DialogueController.Instance.StartDialogue(_chiefDialogue, new List<CharacterBase>{Chief, Friend});
                _chiefDialogueActive = true;
            }

            // sparring dialogue
            if (!_friendHurt && Friend.CurrentHealth <= Friend.MaxHealth/2f)
            {
                StartCoroutine(SparringDialogue());
                _friendHurt = true;
            }
        }
    }

    private void OutOfBounds()
    {
        if (!MatchBoundary.DetectPlayer)
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
        
        MatchBoundary.DetectPlayer = null;
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

    private void ChiefEncounter()
    {
        if (!_chiefEncounter || MatchBoundary.DetectPlayer || _inPos == -1)
            return;
        
        Debug.Log("encounter");

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
        Debug.Log("add");

        _inPos++;
    }

    private void StartMatch()
    {
        if (!_chiefDialogueActive)
            return;

        _chiefDialogueActive = false;

        // start the match
        PlayerChar.StateMachine.Initialize(PlayerChar.IdleState);
        Friend.StateMachine.Initialize(Friend.IdleState);
        Friend.Anim.enabled = true;

        Friend.Leave(PlayerChar);
        Friend.IsSparring = true;

        PlayerChar.Opponents.Add(Friend);
        Friend.Opponents.Add(PlayerChar);

        // Player goes first
        PlayerChar.BattleTurn = true;
    }

    private IEnumerator SparringDialogue()
    {
        yield return new WaitForSeconds(1f);

        // dialogue during battle
        SecondaryDialogueController.Instance.StartDialogue(_damageDialogue, new List<CharacterBase>{Friend}, 1.5f);
    }

    private void FinishMatch()
    {
        if (!Friend.IsSparring)
            return;

        PlayerChar.Opponents.Clear();
        Friend.Opponents.Clear();

        Friend.IsSparring = false;

        PlayerChar.StateMachine.End(); // stop movement (and combat)

        // rest TODO: move this part to the inn tutorial?
        PlayerChar.Heal(PlayerChar.MaxHealth);
        Friend.Heal(Friend.MaxHealth);

        StartCoroutine(DelayNextDialogue());
    }

    private IEnumerator DelayNextDialogue()
    {
        yield return new WaitForSeconds(.4f);

        // start dialogue
        DialogueController.Instance.StartDialogue(_chiefDialogue2, new List<CharacterBase>{Chief, Friend});
        _chiefDialogue2Active = true;
    }

    private void GiveQuest()
    {
        if (!_chiefDialogue2Active)
            return;

        _chiefDialogue2Active = false;

        // start dialogue
        DialogueController.Instance.StartDialogue(_chiefDialogue3, new List<CharacterBase>{Chief, Friend});
        StartCoroutine(DelayNextDialogue2());
    }

    private IEnumerator DelayNextDialogue2()
    {
        yield return new WaitForSeconds(.4f);
        _chiefDialogue3Active = true;
    }

    private void ChiefReturns()
    {
        if (!_chiefDialogue3Active)
            return;

        _chiefDialogue3Active = false;

        Chief.StateMachine.End();
        StartCoroutine(MoveChief());
    }

    private IEnumerator MoveChief()
    {
        // move to house
        float _distance = Vector2.Distance(House.transform.position, Chief.transform.position);

        while (_distance > 0.1f)
        {
            _distance = Vector2.Distance(House.transform.position, Chief.transform.position);
            Chief.Move(House.transform.position - Chief.transform.position);
            PlayerChar.Face(Chief);
            yield return new WaitForFixedUpdate();
        }

        Chief.Move(Vector2.zero);
        Chief.transform.position = new Vector2(18.49f, -41.73f);
        Chief.transform.SetParent(HouseIndoor.transform);
        Chief.StateMachine.Initialize(Chief.IdleState);

        Friend.Anim.enabled = true;
        StartCoroutine(MoveFriend());
    }

    private IEnumerator MoveFriend()
    {
        // move to player
        float _distance = Vector2.Distance(PlayerChar.transform.position, Friend.transform.position);
        while (_distance > 0.7f)
        {
            _distance = Vector2.Distance(PlayerChar.transform.position, Friend.transform.position);
            Friend.Move(PlayerChar.transform.position - Friend.transform.position);
            yield return new WaitForFixedUpdate();
        }

        Friend.Move(Vector2.zero);
        Friend.Anim.enabled = false;

        PlayerChar.Face(Friend);

        // start dialogue
        DialogueController.Instance.StartDialogue(_friendDialogue, new List<CharacterBase>{Friend});
        _friendDialogueActive = true;
    }

    private void FinishEvent()
    {
        if (!_friendDialogueActive)
            return;

        _friendDialogueActive = false;

        Friend.Join(PlayerChar); // rejoin party

        EventIsDone = true; // event done

        DialogueController.Instance.OnDialogueFinish -= OutOfBounds;
        PlayerChar.OnEnter -= EnterBuilding;
        DialogueController.Instance.OnDialogueFinish -= ExitBuilding;
        DialogueController.Instance.OnDialogueFinish -= ChiefEncounter;
        DialogueController.Instance.OnDialogueFinish -= StartMatch;
        DialogueController.Instance.OnBattleDialogueFinish -= FinishMatch;
        DialogueController.Instance.OnDialogueFinish -= GiveQuest;
        DialogueController.Instance.OnDialogueFinish -= ChiefReturns;
        DialogueController.Instance.OnDialogueFinish -= FinishEvent;
    }
}
