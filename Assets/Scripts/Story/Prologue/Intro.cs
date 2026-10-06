using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Intro : EventBase
{
    public Player PlayerChar { get; set; }
    public Companion CompanionChar { get; set; }
    public Villager Mom { get; set; }
    public Villager Chief { get; set; }
    public GameObject House { get; set; }
    public GameObject HouseIndoor { get; set; }
    public Boundary MatchBoundary { get; set; }
    public Destination MomTrigger { get; set; }
    public Destination CompanionTrigger { get; set; }
    public Destination ChiefTrigger { get; set; }
    [SerializeField] private GameObject _reactIcon;
    [SerializeField] private Dialogue _wakeDialogue, _momDialogue, _meetDialogue, _companionDialogue, 
        _outBoundsDialogue, _reachDialogue, _chiefDialogue, _damageDialogue, _chiefDialogue2, 
        _restoreDialogue, _chiefDialogue3, _afterMatchDialogue;
    private bool _momEncounter, _companionEncounter, _entered, _chiefEncounter, _moveToMatch, 
        _matchReady, _chiefDialogueActive, _companionHurt, _chiefDialogue2Active, _moveToChief, 
        _healthRestored, _moveBack, _moveBack2, _origPos, _chiefDialogue3Active, _introEnd;
    private int _inPos;

    private void Start()
    {
        // set dialogue delegates
        DialogueController.Instance.OnDialogueFinish += MomEncounter;
        DialogueController.Instance.OnDialogueFinish += CompanionEncounter;
        DialogueController.Instance.OnDialogueFinish += OutOfBounds;
        PlayerChar.OnEnter += EnterBuilding;
        DialogueController.Instance.OnDialogueFinish += EnterBuilding2;
        DialogueController.Instance.OnDialogueFinish += ChiefEncounter;
        DialogueController.Instance.OnDialogueFinish += StartMatch;
        DialogueController.Instance.OnBattleDialogueFinish += FinishMatch;
        DialogueController.Instance.OnDialogueFinish += RestoreHealth;
        DialogueController.Instance.OnDialogueFinish += RestoreHealth2;
        DialogueController.Instance.OnDialogueFinish += ChiefReturns;
        DialogueController.Instance.OnDialogueFinish += FinishEvent;

        // set positions
        PlayerChar.transform.position = new Vector3(-13.79f,-41.41f);
        CompanionChar.transform.position = new Vector3(3f,-.3f);
        Mom.transform.position = new Vector3(.73f,-43.48f,0);
    }

    private void Update()
    {
        if (MomTrigger.Reached && !Mom.CurrentDialogue)
        {
            // meet mom
            if (!_momEncounter && PlayerChar.StateMachine.CurrentState == PlayerChar.IdleState)
            {
                PlayerChar.StateMachine.End(); // stop movement
                PlayerChar.Face(Mom);
                Mom.StateMachine.End();
                StartCoroutine(MoveMom());
                _momEncounter = true;
            }
        }

        if (CompanionTrigger.Reached && !CompanionChar.CurrentDialogue && !CompanionChar.Leader)
        {
            // meet companion
            if (!_companionEncounter && PlayerChar.StateMachine.CurrentState == PlayerChar.IdleState)
            {
                PlayerChar.StateMachine.End(); // stop movement
                PlayerChar.Face(CompanionChar);

                StartCoroutine(DelayAnimStop());

                // join party
                CompanionChar.Join(PlayerChar);

                // start dialogue
                DialogueController.Instance.StartDialogue(_meetDialogue, new List<CharacterBase>{CompanionChar});
                _companionEncounter = true;
            }
        }

        // out of bounds check
        if (CompanionChar.Leader && MatchBoundary.DetectPlayer && PlayerChar.StateMachine.CurrentState == PlayerChar.IdleState)
        {
            PlayerChar.StateMachine.End(); // stop movement

            CompanionChar.Face(PlayerChar);
            CompanionChar.Anim.Rebind();
            CompanionChar.Anim.enabled = false;

            // start dialogue
            DialogueController.Instance.StartDialogue(_outBoundsDialogue, new List<CharacterBase>{CompanionChar});
        }

        // sparring match
        if (ChiefTrigger.Reached)
        {
            // meet the chief
            if (!_chiefEncounter && !DialogueController.Instance.IsDialogueActive)
            {
                PlayerChar.StateMachine.End(); // stop movement
                CompanionChar.StateMachine.End(); // stop movement
                
                CompanionChar.Anim.Rebind();
                CompanionChar.Anim.enabled = false;

                // start dialogue
                DialogueController.Instance.StartDialogue(_reachDialogue, new List<CharacterBase>{CompanionChar});
                _chiefEncounter = true;
                _moveToMatch = true;
            }

            // setup match
            if (_chiefEncounter && _inPos == 2 && !_matchReady)
            {
                _inPos = 0;

                CompanionChar.Anim.Rebind();
                CompanionChar.Anim.enabled = false;

                // set spawn
                PlayerChar.transform.position = new Vector2(Chief.transform.position.x+.65f, Chief.transform.position.y-1f);
                CompanionChar.transform.position = new Vector2(Chief.transform.position.x-.65f, Chief.transform.position.y-1f);
                
                PlayerChar.Face(Chief);
                CompanionChar.Face(Chief);

                // start chief dialogue
                DialogueController.Instance.StartDialogue(_chiefDialogue, new List<CharacterBase>{Chief, CompanionChar});
                _matchReady = true;
                _chiefDialogueActive = true;
            }

            // sparring dialogue
            if (!_companionHurt && CompanionChar.CurrentHealth <= CompanionChar.MaxHealth/2f)
            {
                StartCoroutine(SparringDialogue());
                _companionHurt = true;
            }

            // after match
            if (_moveToChief && _inPos == 2 && !_healthRestored)
            {
                _inPos = 0;

                CompanionChar.Anim.Rebind();
                CompanionChar.Anim.enabled = false;

                // set spawn
                PlayerChar.transform.position = new Vector2(Chief.transform.position.x+.2f, Chief.transform.position.y-.2f);
                CompanionChar.transform.position = new Vector2(Chief.transform.position.x-.2f, Chief.transform.position.y-.2f);

                PlayerChar.Face(Chief);
                CompanionChar.Face(Chief);

                // chief restores health
                PlayerChar.Heal(PlayerChar.MaxHealth);
                CompanionChar.Heal(CompanionChar.MaxHealth);

                // restore dialogue
                DialogueController.Instance.StartDialogue(_restoreDialogue, new List<CharacterBase>{});
                _healthRestored = true;
                _moveBack = true;
            }

            // return to original position
            if (_moveBack2 && _inPos == 2 && !_origPos)
            {
                _inPos = 0;

                CompanionChar.Anim.Rebind();
                CompanionChar.Anim.enabled = false;

                // set spawn
                PlayerChar.transform.position = new Vector2(Chief.transform.position.x+.65f, Chief.transform.position.y-1f);
                CompanionChar.transform.position = new Vector2(Chief.transform.position.x-.65f, Chief.transform.position.y-1f);

                PlayerChar.Face(Chief);
                CompanionChar.Face(Chief);

                // start quest dialogue
                DialogueController.Instance.StartDialogue(_chiefDialogue3, new List<CharacterBase>{Chief});
                StartCoroutine(DelayNextDialogue2());
                _origPos = true;
            }
        }
    }

    private IEnumerator MoveMom()
    {
        // react
        Vector2 iconPos = new Vector2(Mom.transform.position.x, Mom.transform.position.y+1f);
        GameObject _activeIcon = Instantiate(_reactIcon, iconPos, Quaternion.identity, Mom.transform);
        yield return new WaitForSeconds(1f);
        Destroy(_activeIcon);

        // move to player
        float _distance = Vector2.Distance(PlayerChar.transform.position, Mom.transform.position);
        while (_distance > 0.7f)
        {
            _distance = Vector2.Distance(PlayerChar.transform.position, Mom.transform.position);
            Mom.Move(PlayerChar.transform.position - Mom.transform.position);
            yield return new WaitForFixedUpdate();
        }

        Mom.Move(Vector2.zero);

        // dialogue
        DialogueController.Instance.StartDialogue(_wakeDialogue, new List<CharacterBase>{Mom});
    }

    private void MomEncounter()
    {
        if (!_momEncounter)
            return;
        
        _momEncounter = false;

        Mom.CurrentDialogue = _momDialogue;

        PlayerChar.StateMachine.Initialize(PlayerChar.IdleState); // enable movement
        Mom.StateMachine.Initialize(Mom.IdleState);
    }

    private IEnumerator DelayAnimStop()
    {
        yield return new WaitForSeconds(.4f);
        CompanionChar.Anim.enabled = false;
    }

    private void CompanionEncounter()
    {
        if (!_companionEncounter)
            return;

        _companionEncounter = false;

        CompanionChar.CurrentDialogue = _companionDialogue;

        PlayerChar.CannotEnter = true; // disable enter action
        PlayerChar.StateMachine.Initialize(PlayerChar.IdleState); // enable movement

        CompanionChar.Anim.enabled = true;
    }

    private void OutOfBounds()
    {
        if (!MatchBoundary.DetectPlayer)
            return;

        CompanionChar.Anim.enabled = true;
        StartCoroutine(GoBack());
    }

    private IEnumerator GoBack()
    {
        PlayerChar.Face(CompanionChar);

        // go back
        Vector2 vec = CompanionChar.transform.position - PlayerChar.transform.position;
        
        PlayerChar.Move(vec);

        yield return new WaitForSeconds(.5f);

        PlayerChar.StateMachine.Initialize(PlayerChar.IdleState); // enable movement
        
        MatchBoundary.DetectPlayer = null;
    }

    private void EnterBuilding()
    {
        // enter building check
        PlayerChar.StateMachine.End(); // stop movement

        CompanionChar.Face(PlayerChar);
        CompanionChar.Anim.Rebind();
        CompanionChar.Anim.enabled = false;

        // start dialogue
        DialogueController.Instance.StartDialogue(_outBoundsDialogue, new List<CharacterBase>{CompanionChar});
        _entered = true;
    }

    private void EnterBuilding2()
    {
        if (!_entered)
            return;

        _entered = false;
        CompanionChar.Anim.enabled = true;
        PlayerChar.StateMachine.Initialize(PlayerChar.IdleState); // enable movement
    }

    private void ChiefEncounter()
    {
        if (!_moveToMatch)
            return;
        
        _moveToMatch = false;

        CompanionChar.Anim.enabled = true;

        // before battle
        StartCoroutine(GoToChief(PlayerChar, new Vector2(Chief.transform.position.x+.65f, Chief.transform.position.y-1f)));
        StartCoroutine(GoToChief(CompanionChar, new Vector2(Chief.transform.position.x-.65f, Chief.transform.position.y-1f)));
    }

    private IEnumerator GoToChief(CharacterBase character, Vector3 destination)
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

    private void StartMatch()
    {
        if (!_chiefDialogueActive)
            return;

        _chiefDialogueActive = false;

        // start the match
        PlayerChar.StateMachine.Initialize(PlayerChar.IdleState);
        CompanionChar.StateMachine.Initialize(CompanionChar.IdleState);
        CompanionChar.Anim.enabled = true;

        CompanionChar.Leave(PlayerChar);
        CompanionChar.IsSparring = true;

        PlayerChar.Opponents.Add(CompanionChar);
        CompanionChar.Opponents.Add(PlayerChar);

        // Player goes first
        PlayerChar.BattleTurn = true;
    }

    private IEnumerator SparringDialogue()
    {
        yield return new WaitForSeconds(1f);

        // dialogue during battle
        SecondaryDialogueController.Instance.StartDialogue(_damageDialogue, new List<CharacterBase>{CompanionChar}, 1.5f);
    }

    private void FinishMatch()
    {
        if (!CompanionChar.IsSparring)
            return;

        PlayerChar.Opponents.Clear();
        CompanionChar.Opponents.Clear();

        CompanionChar.IsSparring = false;

        PlayerChar.StateMachine.End(); // stop movement (and combat)

        StartCoroutine(DelayNextDialogue());
    }

    private IEnumerator DelayNextDialogue()
    {
        yield return new WaitForSeconds(.4f);

        // start dialogue
        DialogueController.Instance.StartDialogue(_chiefDialogue2, new List<CharacterBase>{Chief, CompanionChar});
        _chiefDialogue2Active = true;
    }

    private void RestoreHealth()
    {
        if (!_chiefDialogue2Active)
            return;

        _chiefDialogue2Active = false;

        CompanionChar.Anim.enabled = true;

        // go to chief
        StartCoroutine(GoToChief(PlayerChar, new Vector2(Chief.transform.position.x+.2f, Chief.transform.position.y-.2f)));
        StartCoroutine(GoToChief(CompanionChar, new Vector2(Chief.transform.position.x-.2f, Chief.transform.position.y-.2f)));
        _moveToChief = true;
    }

    private void RestoreHealth2()
    {
        if (!_moveBack)
            return;
        
        _moveBack = false;

        CompanionChar.Anim.enabled = true;

        // return
        StartCoroutine(GoToChief(PlayerChar, new Vector2(Chief.transform.position.x+.65f, Chief.transform.position.y-1f)));
        StartCoroutine(GoToChief(CompanionChar, new Vector2(Chief.transform.position.x-.65f, Chief.transform.position.y-1f)));
        _moveBack2 = true;
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

        CompanionChar.Anim.enabled = true;
        StartCoroutine(MoveCompanion());
    }

    private IEnumerator MoveCompanion()
    {
        // move to player
        float _distance = Vector2.Distance(PlayerChar.transform.position, CompanionChar.transform.position);
        while (_distance > 0.7f)
        {
            _distance = Vector2.Distance(PlayerChar.transform.position, CompanionChar.transform.position);
            CompanionChar.Move(PlayerChar.transform.position - CompanionChar.transform.position);
            yield return new WaitForFixedUpdate();
        }

        CompanionChar.Move(Vector2.zero);
        CompanionChar.Anim.enabled = false;

        PlayerChar.Face(CompanionChar);

        // start dialogue
        DialogueController.Instance.StartDialogue(_afterMatchDialogue, new List<CharacterBase>{CompanionChar});
        _introEnd = true;
    }

    private void FinishEvent()
    {
        if (!_introEnd)
            return;

        _introEnd = false;

        PlayerChar.CannotEnter = false; // enable enter action
        PlayerChar.StateMachine.Initialize(PlayerChar.IdleState);
        CompanionChar.StateMachine.Initialize(CompanionChar.IdleState);
        CompanionChar.Anim.enabled = true;

        EventIsDone = true; // event done

        DialogueController.Instance.OnDialogueFinish -= MomEncounter;
        DialogueController.Instance.OnDialogueFinish -= CompanionEncounter;
        DialogueController.Instance.OnDialogueFinish -= OutOfBounds;
        PlayerChar.OnEnter -= EnterBuilding;
        DialogueController.Instance.OnDialogueFinish -= EnterBuilding2;
        DialogueController.Instance.OnDialogueFinish -= ChiefEncounter;
        DialogueController.Instance.OnDialogueFinish -= StartMatch;
        DialogueController.Instance.OnBattleDialogueFinish -= FinishMatch;
        DialogueController.Instance.OnDialogueFinish -= RestoreHealth;
        DialogueController.Instance.OnDialogueFinish -= RestoreHealth2;
        DialogueController.Instance.OnDialogueFinish -= ChiefReturns;
        DialogueController.Instance.OnDialogueFinish -= FinishEvent;
    }
}
