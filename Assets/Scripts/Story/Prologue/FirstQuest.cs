using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FirstQuest : EventBase
{
    public Player PlayerChar { get; set; }
    public Companion Friend { get; set; }
    public Villager Mom { get; set; }
    public Villager Chief { get; set; }
    public Villager Shopkeeper { get; set; }
    public Villager Traveller { get; set; }
    public Enemy SlimeChar { get; set; }
    public Enemy SlimeChar1 { get; set; }
    public Enemy SlimeChar2 { get; set; }
    public Enemy SlimeChar3 { get; set; }
    public GameObject HouseIndoor { get; set; }
    public GameObject Chest { get; set; }
    public GameObject Vines { get; set; }
    public GameObject Crystal { get; set; }
    public Boundary SlimeBoundary { get; set; }
    public Destination EncounterTrigger { get; set; }
    public Destination AmbushTrigger { get; set; }
    [SerializeField] private GameObject _reactIcon;
    [SerializeField] private Dialogue _friendDialogue, _chiefDialogue, _momDialogue, _shopkeeperDialogue, 
        _slimeDialogue, _friendDialogue2, _outBoundsDialogue, _slimeDialogue2, _friendDialogue3, _chestDialogue, 
        _ambushDialogue, _ambushBattleDialogue, _afterAmbushDialogue, _travellerDialogue, _vinesDialogue, 
        _friendDialogue4, _crystalDialogue, _friendDialogue5, _friendDialogue6, _removeVinesDialogue, 
        _friendDialogue7;
    private bool _slimeEncounter, _firstSlimeDefeat, _firstSlimeDefeat2, _findChest, _ambush,
        _ambushTutorial, _vines, _crystal, _crystalEncounter, _crystalSlimeDefeat, _vinesRemoved;
    private int _inPos;

    private void Start()
    {
        // set dialogue delegates
        DialogueController.Instance.OnDialogueFinish += SlimeEncounter;
        DialogueController.Instance.OnDialogueFinish += OutOfBounds;
        DialogueController.Instance.OnBattleDialogueFinish += SlimeDefeat;
        DialogueController.Instance.OnDialogueFinish += SlimeDefeat2;
        DialogueController.Instance.OnDialogueFinish += Ambush;
        DialogueController.Instance.OnDialogueFinish += CrystalEncounter;
        DialogueController.Instance.OnBattleDialogueFinish += CrystalSlimeDefeat;
        Crystal.GetComponentInChildren<Interact>().OnInteract += CrystalInteract;
        DialogueController.Instance.OnDialogueFinish += VinesRemoved;
        // DialogueController.Instance.OnDialogueFinish += FinishEvent;

        // set current dialogues
        Friend.CurrentDialogue = _friendDialogue;
        Mom.CurrentDialogue = _momDialogue;
        Chief.CurrentDialogue = _chiefDialogue;
        Shopkeeper.CurrentDialogue = _shopkeeperDialogue;
        Traveller.CurrentDialogue = _travellerDialogue;

        // reset chief position
        Chief.transform.position = new Vector2(18.49f, -41.73f);
        Chief.transform.SetParent(HouseIndoor.transform);

        // reset mom position
        Mom.transform.position = new Vector3(.73f,-43.48f,0);
        Mom.Sprite.flipX = false;

        Friend.Join(PlayerChar); // rejoin party

        // available quests
        Vector2 iconPos = new Vector2(Traveller.transform.position.x, Traveller.transform.position.y+1f);
        GameObject _activeIcon = Instantiate(_reactIcon, iconPos, Quaternion.identity, Traveller.transform);
        _activeIcon.GetComponent<React>().Mute = true;

        // disable crystal
        Crystal.GetComponentInChildren<Interact>().gameObject.GetComponent<BoxCollider2D>().enabled = false;
    }

    private void Update()
    {
        // first enemy slime encounter
        if (EncounterTrigger.Reached && !_firstSlimeDefeat)
        {
            // approach slime
            if (!_slimeEncounter && !DialogueController.Instance.IsDialogueActive)
            {
                PlayerChar.StateMachine.End(); // stop movement
                Friend.StateMachine.End(); // stop movement
                
                Friend.Anim.Rebind();
                Friend.Anim.enabled = false;

                CameraController.Instance.target = SlimeChar.transform;

                // start dialogue
                StartCoroutine(SlimeDialogue());
                _slimeEncounter = true;
            }

            // out of bounds check
            if (_slimeEncounter && SlimeBoundary.DetectPlayer && PlayerChar.StateMachine.CurrentState == PlayerChar.IdleState)
            {
                PlayerChar.StateMachine.End(); // stop movement

                Friend.Face(PlayerChar);
                Friend.Anim.Rebind();
                Friend.Anim.enabled = false;

                // start dialogue
                DialogueController.Instance.StartDialogue(_outBoundsDialogue, new List<CharacterBase>{Friend});
            }
        }

        // chest
        if (!_findChest)
        {
            float chestDistance = Vector2.Distance(Chest.transform.position, PlayerChar.transform.position);
            if (chestDistance < 3f)
            {
                SecondaryDialogueController.Instance.StartDialogue(_chestDialogue, new List<CharacterBase>{Friend}, 1.5f);
                _findChest = true;
            }
        }

        // ambush
        if (AmbushTrigger.Reached)
        {
            if (!_ambush)
            {
                PlayerChar.StateMachine.End(); // stop movement
                Friend.StateMachine.End(); // stop movement

                Friend.Anim.Rebind();
                Friend.Anim.enabled = false;

                SlimeChar1.Face(PlayerChar);
                SlimeChar2.Face(Friend);

                SlimeChar1.StateMachine.End();
                SlimeChar2.StateMachine.End();

                StartCoroutine(MoveSlime(SlimeChar1, new Vector2(PlayerChar.transform.position.x-1.6f, PlayerChar.transform.position.y+.5f)));
                StartCoroutine(MoveSlime(SlimeChar2, new Vector2(Friend.transform.position.x+1.6f, Friend.transform.position.y-.3f)));

                _ambush = true;
            }
            if (_inPos == 2)
            {
                _inPos = -1;

                StartCoroutine(AmbushReact());
            }
        }

        // ambush battle dialogue
        if (PlayerChar.BattleTurn && _ambush && !_ambushTutorial)
        {
            StartCoroutine(AmbushBattleDialogue());
            _ambushTutorial = true;
        }

        // vines
        if (!_vines && !_crystalEncounter)
        {
            float vinesDistance = Vector2.Distance(Vines.transform.position, PlayerChar.transform.position);
            if (vinesDistance < 3f)
            {
                SecondaryDialogueController.Instance.StartDialogue(_vinesDialogue, new List<CharacterBase>{Friend}, 1.5f);
                Friend.CurrentDialogue = _friendDialogue4;
                _vines = true;
            }
        }

        // crystal
        if (!_crystal)
        {
            float crystalDistance = Vector2.Distance(Crystal.transform.position, PlayerChar.transform.position);
            if (crystalDistance < 3f)
            {
                PlayerChar.StateMachine.End(); // stop movement
                Friend.StateMachine.End(); // stop movement

                Friend.Anim.Rebind();
                Friend.Anim.enabled = false;

                CameraController.Instance.target = Crystal.transform;

                StartCoroutine(CrystalDialogue());

                _crystal = true;
            }
        }
    }

    private IEnumerator SlimeDialogue()
    {
        yield return new WaitForSeconds(.5f);

        // start dialogue
        DialogueController.Instance.StartDialogue(_slimeDialogue, new List<CharacterBase>{Friend});
    }

    private void SlimeEncounter()
    {
        if (!_slimeEncounter || SlimeBoundary.DetectPlayer || SlimeChar == null)
            return;

        CameraController.Instance.target = PlayerChar.transform;

        PlayerChar.StateMachine.Initialize(PlayerChar.IdleState);
        Friend.StateMachine.Initialize(Friend.IdleState);

        Friend.Anim.enabled = true;
        Friend.CurrentDialogue = _friendDialogue2;
    }

    private void OutOfBounds()
    {
        if (!SlimeBoundary.DetectPlayer)
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
        
        SlimeBoundary.DetectPlayer = null;
    }

    private void SlimeDefeat()
    {
        if ((SlimeChar && SlimeChar.Opponents.Count == 0) || _firstSlimeDefeat)
            return;

        _firstSlimeDefeat = true;

        PlayerChar.StateMachine.End(); // stop movement
        PlayerChar.Face(Friend);

        Friend.StateMachine.End();

        StartCoroutine(MoveFriend());
    }

    private IEnumerator MoveFriend()
    {
        // move to player
        Friend.Face(PlayerChar);
        float _distance = Vector2.Distance(PlayerChar.transform.position, Friend.transform.position);
        while (_distance > 0.7f)
        {
            _distance = Vector2.Distance(PlayerChar.transform.position, Friend.transform.position);
            Friend.Move(PlayerChar.transform.position - Friend.transform.position);
            yield return new WaitForFixedUpdate();
        }

        Friend.Move(Vector2.zero);

        StartCoroutine(DelayAnimStop());

        // start dialogue
        DialogueController.Instance.StartDialogue(_slimeDialogue2, new List<CharacterBase>{Friend});
        _firstSlimeDefeat2 = true;
    }

    private IEnumerator DelayAnimStop()
    {
        yield return new WaitForSeconds(.4f);
        if (_firstSlimeDefeat2)
            Friend.Anim.enabled = false;
    }

    private void SlimeDefeat2()
    {
        if (!_firstSlimeDefeat2)
            return;
        
        _firstSlimeDefeat2 = false;

        Friend.CurrentDialogue = _friendDialogue3;

        PlayerChar.StateMachine.Initialize(PlayerChar.IdleState);
        Friend.StateMachine.Initialize(Friend.IdleState);
        Friend.Anim.enabled = true;
    }

    private IEnumerator MoveSlime(CharacterBase character, Vector3 destination)
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

        _inPos++;
    }

    private IEnumerator AmbushReact()
    {
        // react
        Vector2 iconPos = new Vector2(Friend.transform.position.x, Friend.transform.position.y+1f);
        GameObject _activeIcon = Instantiate(_reactIcon, iconPos, Quaternion.identity, Friend.transform);
        yield return new WaitForSeconds(1f);
        Destroy(_activeIcon);

        // start dialogue
        DialogueController.Instance.StartDialogue(_ambushDialogue, new List<CharacterBase>{Friend});
    }

    private void Ambush()
    {
        if (!_ambush || (SlimeChar1 == null && SlimeChar2 == null))
            return;
        
        PlayerChar.StateMachine.Initialize(PlayerChar.IdleState);
        Friend.StateMachine.Initialize(Friend.IdleState);
        Friend.Anim.enabled = true;
        Friend.EventDialogue = _afterAmbushDialogue;

        SlimeChar1.StateMachine.Initialize(SlimeChar1.IdleState);
        SlimeChar2.StateMachine.Initialize(SlimeChar2.IdleState);

        PlayerChar.Opponents.AddRange(new List<FighterBase>{SlimeChar1, SlimeChar2});
        Friend.Opponents.AddRange(new List<FighterBase>{SlimeChar1, SlimeChar2});

        SlimeChar1.Opponents.AddRange(new List<FighterBase>{PlayerChar, Friend});
        SlimeChar1.Allies.Add(SlimeChar2);
        SlimeChar2.Opponents.AddRange(new List<FighterBase>{PlayerChar, Friend});
        SlimeChar2.Allies.Add(SlimeChar1);

        // Slime goes first
        SlimeChar1.BattleTurn = true;
    }

    private IEnumerator AmbushBattleDialogue()
    {
        yield return new WaitForSeconds(1f);

        // dialogue during battle
        SecondaryDialogueController.Instance.StartDialogue(_ambushBattleDialogue, new List<CharacterBase>{Friend}, 3f);
    }

    private IEnumerator CrystalDialogue()
    {
        yield return new WaitForSeconds(.5f);

        // start dialogue
        DialogueController.Instance.StartDialogue(_crystalDialogue, new List<CharacterBase>{Friend});
    }

    private void CrystalEncounter()
    {
        if (!_crystal || _crystalEncounter)
            return;

        _crystalEncounter = true;

        CameraController.Instance.target = PlayerChar.transform;

        PlayerChar.StateMachine.Initialize(PlayerChar.IdleState);
        Friend.StateMachine.Initialize(Friend.IdleState);

        Friend.Anim.enabled = true;
        Friend.CurrentDialogue = _friendDialogue5;
    }

    private void CrystalSlimeDefeat()
    {
        if ((SlimeChar3 && SlimeChar3.Opponents.Count == 0) || _crystalSlimeDefeat)
            return;
        
        _crystalSlimeDefeat = true;

        Friend.CurrentDialogue = _friendDialogue6;

        // enable crystal
        Crystal.GetComponentInChildren<Interact>().gameObject.GetComponent<BoxCollider2D>().enabled = true;
    }

    private void CrystalInteract()
    {
        PlayerChar.StateMachine.End(); // stop movement
        Friend.StateMachine.End();
        
        StartCoroutine(RemoveVines());

        // disable crystal
        Crystal.GetComponentInChildren<Interact>().gameObject.GetComponent<BoxCollider2D>().enabled = false;
    }

    private IEnumerator RemoveVines()
    {
        CameraController.Instance.target = Vines.transform;
        yield return new WaitForSeconds(1f);
        Destroy(Vines);
        yield return new WaitForSeconds(1f);
        CameraController.Instance.target = PlayerChar.transform;

        Friend.Face(PlayerChar);
        Friend.Anim.enabled = false;

        yield return new WaitForSeconds(1f);

        // start dialogue
        DialogueController.Instance.StartDialogue(_removeVinesDialogue, new List<CharacterBase>{Friend});
        _vinesRemoved = true;
    }

    private void VinesRemoved()
    {
        if (!_vinesRemoved)
            return;
        
        _vinesRemoved = false;

        PlayerChar.StateMachine.Initialize(PlayerChar.IdleState);
        Friend.StateMachine.Initialize(Friend.IdleState);

        Friend.Anim.enabled = true;
        Friend.CurrentDialogue = _friendDialogue7;
    }

    private void FinishEvent()
    {
        EventIsDone = true; // event done

        DialogueController.Instance.OnDialogueFinish -= FinishEvent;
    }
}
