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
    public GameObject HouseIndoor { get; set; }
    public GameObject Chest { get; set; }
    public GameObject Vines { get; set; }
    public Destination EncounterTrigger { get; set; }
    public Destination AmbushTrigger { get; set; }
    [SerializeField] private GameObject _reactIcon;
    [SerializeField] private Dialogue _friendDialogue, _chiefDialogue, _momDialogue, _shopkeeperDialogue, 
        _slimeDialogue, _friendDialogue2, _outBoundsDialogue, _slimeDialogue2, _afterBattleDialogue, 
        _friendDialogue3, _chestDialogue, _ambushDialogue, _afterAmbushDialogue, _travellerDialogue,
        _vinesDialogue;
    private bool _encounter, _outBounds, _firstSlimeDefeat, _ambushSlimeDefeat, _findChest, _ambush, _vines;
    private int _inPos;

    private void Start()
    {
        // set dialogue delegates
        DialogueController.Instance.OnDialogueFinish += SlimeEncounter;
        DialogueController.Instance.OnDialogueFinish += OutOfBounds;
        DialogueController.Instance.OnBattleDialogueFinish += SlimeDefeat;
        DialogueController.Instance.OnDialogueFinish += SlimeDefeat2;
        DialogueController.Instance.OnDialogueFinish += Ambush;
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

        // available quests
        Vector2 iconPos = new Vector2(Traveller.transform.position.x, Traveller.transform.position.y+1f);
        GameObject _activeIcon = Instantiate(_reactIcon, iconPos, Quaternion.identity, Traveller.transform);
        _activeIcon.GetComponent<React>().Mute = true;
    }

    private void Update()
    {
        // first enemy slime encounter
        if (EncounterTrigger.Reached && !_firstSlimeDefeat)
        {
            float slimeDistance = Vector2.Distance(SlimeChar.SpawnPoint, PlayerChar.transform.position);
            if (!_encounter && !DialogueController.Instance.IsDialogueActive)
            {
                PlayerChar.StateMachine.End(); // stop movement
                Friend.StateMachine.End(); // stop movement
                
                Friend.Anim.Rebind();
                Friend.Anim.enabled = false;

                // start dialogue
                DialogueController.Instance.StartDialogue(_slimeDialogue, new List<CharacterBase>{Friend});

                CameraController.Instance.target = SlimeChar.transform;

                _encounter = true;
            }
            if (_encounter && slimeDistance > 7f && PlayerChar.StateMachine.CurrentState == PlayerChar.IdleState)
            {
                PlayerChar.StateMachine.End(); // stop movement

                Friend.Face(PlayerChar);
                Friend.Anim.Rebind();
                Friend.Anim.enabled = false;

                // start dialogue
                DialogueController.Instance.StartDialogue(_outBoundsDialogue, new List<CharacterBase>{Friend});
                _outBounds = true;
            }
        }

        // chest
        if (!_findChest)
        {
            float chestDistance = Vector2.Distance(Chest.transform.position, PlayerChar.transform.position);
            if (chestDistance < 3f)
            {
                SecondaryDialogueController.Instance.StartDialogue(_chestDialogue, new List<CharacterBase>{Friend});
                _findChest = true;
            }
        }

        // ambush
        if (AmbushTrigger.Reached && !_ambushSlimeDefeat)
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

        // vines
        if (!_vines)
        {
            float vinesDistance = Vector2.Distance(Vines.transform.position, PlayerChar.transform.position);
            if (vinesDistance < 3f)
            {
                SecondaryDialogueController.Instance.StartDialogue(_vinesDialogue, new List<CharacterBase>{Friend});
                _vines = true;
            }
        }
    }

    private void SlimeEncounter()
    {
        if (!_encounter || _outBounds || SlimeChar == null)
            return;

        CameraController.Instance.target = PlayerChar.transform;

        PlayerChar.StateMachine.Initialize(PlayerChar.IdleState);
        Friend.StateMachine.Initialize(Friend.IdleState);

        Friend.Anim.enabled = true;
        Friend.CurrentDialogue = _friendDialogue2;
    }

    private void OutOfBounds()
    {
        if (!_outBounds)
            return;

        _outBounds = false;

        Friend.Anim.enabled = true;
        StartCoroutine(GoBack(PlayerChar));
    }

    private IEnumerator GoBack(Player player)
    {
        player.Face(SlimeChar);

        Vector3 returnPos = SlimeChar.SpawnPoint;

        // go back
        float _distance = Vector2.Distance(returnPos, player.transform.position);
        while (_distance > 2f)
        {
            _distance = Vector2.Distance(returnPos, player.transform.position);
            player.Move(returnPos - player.transform.position);
            yield return new WaitForFixedUpdate();
        }

        player.StateMachine.Initialize(player.IdleState); // enable movement
    }

    private void SlimeDefeat()
    {
        if ((SlimeChar && SlimeChar.Opponents.Count == 0) || _firstSlimeDefeat)
            return;

        _firstSlimeDefeat = true;

        PlayerChar.StateMachine.End(); // stop movement
        PlayerChar.Face(Friend);

        Friend.StateMachine.End();
        Friend.SpeakAfterBattle = false;

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
        _ambushSlimeDefeat = true;
    }

    private IEnumerator DelayAnimStop()
    {
        yield return new WaitForSeconds(.4f);
        if (_ambushSlimeDefeat)
            Friend.Anim.enabled = false;
    }

    private void SlimeDefeat2()
    {
        if (!_ambushSlimeDefeat)
            return;
        
        _ambushSlimeDefeat = false;

        Friend.SpeakAfterBattle = true;
        Friend.CurrentDialogue = _friendDialogue3;
        Friend.CurAfterBattleDialogue = _afterBattleDialogue;

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
        Friend.CurAfterBattleDialogue = _afterAmbushDialogue;

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

    private void FinishEvent()
    {
        EventIsDone = true; // event done

        DialogueController.Instance.OnDialogueFinish -= FinishEvent;
    }
}
