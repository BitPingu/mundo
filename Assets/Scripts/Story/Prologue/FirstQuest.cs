using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class FirstQuest : EventBase
{
    public Player PlayerChar { get; set; }
    public Companion CompanionChar { get; set; }
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
    public GameObject Vines1 { get; set; }
    public GameObject Crystal { get; set; }
    public GameObject Campfire { get; set; }
    public Boundary SlimeBoundary { get; set; }
    public Destination EncounterTrigger { get; set; }
    public Destination AmbushTrigger { get; set; }
    public Destination RestTrigger { get; set; }
    public Destination SenseTrigger { get; set; }
    [SerializeField] private GameObject _reactIcon;
    [SerializeField] private Dialogue _companionDialogue, _chiefDialogue, _momDialogue, _shopkeeperDialogue, 
        _slimeDialogue, _companionDialogue2, _outBoundsDialogue, _slimeDialogue2, _companionDialogue3, _chestDialogue, 
        _ambushDialogue, _ambushBattleDialogue, _afterAmbushDialogue, _travellerDialogue, _vinesDialogue, 
        _companionDialogue4, _crystalDialogue, _companionDialogue5, _companionDialogue6, _removeVinesDialogue, 
        _companionDialogue7, _restDialogue, _nostalgiaDialogue, _restoreDialogue, _companionDialogue8, _senseDialogue,
        _companionDialogue9;
    private bool _slimeEncounter, _firstSlimeDefeat, _firstSlimeDefeat2, _findChest, _ambush,
        _ambushTutorial, _vines, _crystal, _crystalEncounter, _crystalSlimeDefeat, _vinesRemoved,
        _rest, _rest2, _rested, _healthRestored, _sense, _sensed;
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
        DialogueController.Instance.OnDialogueFinish += RestStop;
        DialogueController.Instance.OnDialogueFinish += RestDialogue;
        DialogueController.Instance.OnDialogueFinish += RestoreHealth;
        DialogueController.Instance.OnDialogueFinish += Sense;
        // DialogueController.Instance.OnDialogueFinish += FinishEvent;

        // set current dialogues
        CompanionChar.CurrentDialogue = _companionDialogue;
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

        CompanionChar.Join(PlayerChar); // rejoin party

        // available quests
        Vector2 iconPos = new Vector2(Traveller.transform.position.x, Traveller.transform.position.y+1f);
        GameObject _activeIcon = Instantiate(_reactIcon, iconPos, Quaternion.identity, Traveller.transform);
        _activeIcon.GetComponent<React>().Mute = true;

        // disable objects
        Crystal.GetComponentInChildren<Interact>().gameObject.GetComponent<BoxCollider2D>().enabled = false;
        Campfire.SetActive(false);
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
                CompanionChar.StateMachine.End(); // stop movement
                
                CompanionChar.Anim.Rebind();
                CompanionChar.Anim.enabled = false;

                CameraController.Instance.target = SlimeChar.transform;

                // start dialogue
                StartCoroutine(SlimeDialogue());
                _slimeEncounter = true;
            }

            // out of bounds check
            if (_slimeEncounter && SlimeBoundary.DetectPlayer && PlayerChar.StateMachine.CurrentState == PlayerChar.IdleState)
            {
                PlayerChar.StateMachine.End(); // stop movement

                CompanionChar.Face(PlayerChar);
                CompanionChar.Anim.Rebind();
                CompanionChar.Anim.enabled = false;

                // start dialogue
                DialogueController.Instance.StartDialogue(_outBoundsDialogue, new List<CharacterBase>{CompanionChar});
            }
        }

        // chest
        if (!_findChest)
        {
            float chestDistance = Vector2.Distance(Chest.transform.position, PlayerChar.transform.position);
            if (chestDistance < 3f)
            {
                SecondaryDialogueController.Instance.StartDialogue(_chestDialogue, new List<CharacterBase>{CompanionChar}, 1.5f);
                _findChest = true;
            }
        }

        // ambush
        if (AmbushTrigger.Reached)
        {
            if (!_ambush)
            {
                PlayerChar.StateMachine.End(); // stop movement
                CompanionChar.StateMachine.End(); // stop movement

                CompanionChar.Anim.Rebind();
                CompanionChar.Anim.enabled = false;

                SlimeChar1.Face(PlayerChar);
                SlimeChar2.Face(CompanionChar);

                SlimeChar1.StateMachine.End();
                SlimeChar2.StateMachine.End();

                StartCoroutine(MoveSlime(SlimeChar1, new Vector2(PlayerChar.transform.position.x-1.6f, PlayerChar.transform.position.y+.5f)));
                StartCoroutine(MoveSlime(SlimeChar2, new Vector2(CompanionChar.transform.position.x+1.6f, CompanionChar.transform.position.y-.3f)));

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
                SecondaryDialogueController.Instance.StartDialogue(_vinesDialogue, new List<CharacterBase>{CompanionChar}, 1.5f);
                CompanionChar.CurrentDialogue = _companionDialogue4;
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
                CompanionChar.StateMachine.End(); // stop movement

                CompanionChar.Anim.Rebind();
                CompanionChar.Anim.enabled = false;

                CameraController.Instance.target = Crystal.transform;

                StartCoroutine(CrystalDialogue());

                _crystal = true;
            }
        }

        // rest stop
        if (RestTrigger.Reached)
        {
            if (!_rest)
            {
                PlayerChar.StateMachine.End(); // stop movement
                CompanionChar.StateMachine.End(); // stop movement

                CompanionChar.Anim.Rebind();
                CompanionChar.Anim.enabled = false;

                // start dialogue
                DialogueController.Instance.StartDialogue(_restDialogue, new List<CharacterBase>{CompanionChar});
                _rest = true;
            }
        }

        // sense boss
        if (SenseTrigger.Reached)
        {
            if (!_sense)
            {
                PlayerChar.StateMachine.End(); // stop movement
                CompanionChar.StateMachine.End(); // stop movement

                CompanionChar.Anim.Rebind();
                CompanionChar.Anim.enabled = false;

                CameraController.Instance.target = Vines1.transform;

                // start dialogue
                StartCoroutine(SenseDialogue());
                _sense = true;
            }
        }
    }

    private IEnumerator SlimeDialogue()
    {
        yield return new WaitForSeconds(.5f);

        // start dialogue
        DialogueController.Instance.StartDialogue(_slimeDialogue, new List<CharacterBase>{CompanionChar});
    }

    private void SlimeEncounter()
    {
        if (!_slimeEncounter || SlimeBoundary.DetectPlayer || SlimeChar == null)
            return;

        CameraController.Instance.target = PlayerChar.transform;

        PlayerChar.StateMachine.Initialize(PlayerChar.IdleState);
        CompanionChar.StateMachine.Initialize(CompanionChar.IdleState);

        CompanionChar.Anim.enabled = true;
        CompanionChar.CurrentDialogue = _companionDialogue2;
    }

    private void OutOfBounds()
    {
        if (!SlimeBoundary.DetectPlayer)
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
        
        SlimeBoundary.DetectPlayer = null;
    }

    private void SlimeDefeat()
    {
        if ((SlimeChar && SlimeChar.Opponents.Count == 0) || _firstSlimeDefeat)
            return;

        _firstSlimeDefeat = true;

        PlayerChar.StateMachine.End(); // stop movement
        PlayerChar.Face(CompanionChar);

        CompanionChar.StateMachine.End();

        StartCoroutine(MoveCompanion());
    }

    private IEnumerator MoveCompanion()
    {
        // move to player
        CompanionChar.Face(PlayerChar);
        float _distance = Vector2.Distance(PlayerChar.transform.position, CompanionChar.transform.position);
        while (_distance > 0.7f)
        {
            _distance = Vector2.Distance(PlayerChar.transform.position, CompanionChar.transform.position);
            CompanionChar.Move(PlayerChar.transform.position - CompanionChar.transform.position);
            yield return new WaitForFixedUpdate();
        }

        CompanionChar.Move(Vector2.zero);

        StartCoroutine(DelayAnimStop());

        // start dialogue
        DialogueController.Instance.StartDialogue(_slimeDialogue2, new List<CharacterBase>{CompanionChar});
        _firstSlimeDefeat2 = true;
    }

    private IEnumerator DelayAnimStop()
    {
        yield return new WaitForSeconds(.4f);
        if (_firstSlimeDefeat2)
            CompanionChar.Anim.enabled = false;
    }

    private void SlimeDefeat2()
    {
        if (!_firstSlimeDefeat2)
            return;
        
        _firstSlimeDefeat2 = false;

        CompanionChar.CurrentDialogue = _companionDialogue3;

        PlayerChar.StateMachine.Initialize(PlayerChar.IdleState);
        CompanionChar.StateMachine.Initialize(CompanionChar.IdleState);
        CompanionChar.Anim.enabled = true;
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
        Vector2 iconPos = new Vector2(CompanionChar.transform.position.x, CompanionChar.transform.position.y+1f);
        GameObject _activeIcon = Instantiate(_reactIcon, iconPos, Quaternion.identity, CompanionChar.transform);
        yield return new WaitForSeconds(1f);
        Destroy(_activeIcon);

        // start dialogue
        DialogueController.Instance.StartDialogue(_ambushDialogue, new List<CharacterBase>{CompanionChar});
    }

    private void Ambush()
    {
        if (!_ambush || (SlimeChar1 == null && SlimeChar2 == null))
            return;
        
        PlayerChar.StateMachine.Initialize(PlayerChar.IdleState);
        CompanionChar.StateMachine.Initialize(CompanionChar.IdleState);
        CompanionChar.Anim.enabled = true;
        CompanionChar.EventDialogue = _afterAmbushDialogue;

        SlimeChar1.StateMachine.Initialize(SlimeChar1.IdleState);
        SlimeChar2.StateMachine.Initialize(SlimeChar2.IdleState);

        PlayerChar.Opponents.AddRange(new List<FighterBase>{SlimeChar1, SlimeChar2});
        CompanionChar.Opponents.AddRange(new List<FighterBase>{SlimeChar1, SlimeChar2});

        SlimeChar1.Opponents.AddRange(new List<FighterBase>{PlayerChar, CompanionChar});
        SlimeChar1.Allies.Add(SlimeChar2);
        SlimeChar2.Opponents.AddRange(new List<FighterBase>{PlayerChar, CompanionChar});
        SlimeChar2.Allies.Add(SlimeChar1);

        // Slime goes first
        SlimeChar1.BattleTurn = true;
    }

    private IEnumerator AmbushBattleDialogue()
    {
        yield return new WaitForSeconds(1f);

        // dialogue during battle
        SecondaryDialogueController.Instance.StartDialogue(_ambushBattleDialogue, new List<CharacterBase>{CompanionChar}, 3f);
    }

    private IEnumerator CrystalDialogue()
    {
        yield return new WaitForSeconds(.5f);

        // start dialogue
        DialogueController.Instance.StartDialogue(_crystalDialogue, new List<CharacterBase>{CompanionChar});
    }

    private void CrystalEncounter()
    {
        if (!_crystal || _crystalEncounter)
            return;

        _crystalEncounter = true;

        CameraController.Instance.target = PlayerChar.transform;

        PlayerChar.StateMachine.Initialize(PlayerChar.IdleState);
        CompanionChar.StateMachine.Initialize(CompanionChar.IdleState);

        CompanionChar.Anim.enabled = true;
        CompanionChar.CurrentDialogue = _companionDialogue5;
    }

    private void CrystalSlimeDefeat()
    {
        if ((SlimeChar3 && SlimeChar3.Opponents.Count == 0) || _crystalSlimeDefeat)
            return;
        
        _crystalSlimeDefeat = true;

        CompanionChar.CurrentDialogue = _companionDialogue6;

        // enable crystal
        Crystal.GetComponentInChildren<Interact>().gameObject.GetComponent<BoxCollider2D>().enabled = true;
    }

    private void CrystalInteract()
    {
        PlayerChar.StateMachine.End(); // stop movement
        CompanionChar.StateMachine.End();
        
        StartCoroutine(RemoveVines());

        // disable crystal
        Crystal.GetComponentInChildren<Interact>().gameObject.GetComponent<BoxCollider2D>().enabled = false;
        Crystal.GetComponentInChildren<Light2D>().enabled = false;
    }

    private IEnumerator RemoveVines()
    {
        CameraController.Instance.target = Vines.transform;
        yield return new WaitForSeconds(1f);
        Destroy(Vines);
        yield return new WaitForSeconds(1f);
        CameraController.Instance.target = PlayerChar.transform;

        CompanionChar.Face(PlayerChar);
        CompanionChar.Anim.enabled = false;

        yield return new WaitForSeconds(1f);

        // start dialogue
        DialogueController.Instance.StartDialogue(_removeVinesDialogue, new List<CharacterBase>{CompanionChar});
        _vinesRemoved = true;
    }

    private void VinesRemoved()
    {
        if (!_vinesRemoved)
            return;
        
        _vinesRemoved = false;

        PlayerChar.StateMachine.Initialize(PlayerChar.IdleState);
        CompanionChar.StateMachine.Initialize(CompanionChar.IdleState);

        CompanionChar.Anim.enabled = true;
        CompanionChar.CurrentDialogue = _companionDialogue7;
    }

    private async void RestStop()
    {
        if (!_rest || _rest2)
            return;
        
        _rest2 = true;

        await Transition.Instance.FadeOut();

        // move characters
        PlayerChar.transform.position = new Vector2(Campfire.transform.position.x+.8f, Campfire.transform.position.y);
        CompanionChar.transform.position = new Vector2(Campfire.transform.position.x-.8f, Campfire.transform.position.y);

        PlayerChar.Face(CompanionChar);
        CompanionChar.Face(PlayerChar);

        Campfire.SetActive(true);

        // lighting to night
        // Lighting.Instance.SetLighting(255f, .4f);

        await Transition.Instance.FadeIn();

        // start dialogue
        DialogueController.Instance.StartDialogue(_nostalgiaDialogue, new List<CharacterBase>{CompanionChar, PlayerChar});
        _rested = true;
    }

    private async void RestDialogue()
    {
        if (!_rested)
            return;
        
        _rested = false;

        await Transition.Instance.FadeOut();

        // lighting to day
        // Lighting.Instance.SetLighting(50f, 1f);

        // Campfire.GetComponentInChildren<Light2D>().enabled = false;

        PlayerChar.transform.position = new Vector2(Campfire.transform.position.x+.8f, Campfire.transform.position.y-.4f);

        await Transition.Instance.FadeIn(); 

        // campfire restores health
        PlayerChar.Heal(PlayerChar.MaxHealth);
        CompanionChar.Heal(CompanionChar.MaxHealth); 

        // restore dialogue
        DialogueController.Instance.StartDialogue(_restoreDialogue, new List<CharacterBase>{});
        _healthRestored = true;
    }

    private void RestoreHealth()
    {
        if (!_healthRestored)
            return;
        
        _healthRestored = false;

        PlayerChar.StateMachine.Initialize(PlayerChar.IdleState);
        CompanionChar.StateMachine.Initialize(CompanionChar.IdleState);

        CompanionChar.Anim.enabled = true;
        CompanionChar.CurrentDialogue = _companionDialogue8;
    }

    private IEnumerator SenseDialogue()
    {
        yield return new WaitForSeconds(.5f);

        // start dialogue
        DialogueController.Instance.StartDialogue(_senseDialogue, new List<CharacterBase>{CompanionChar});
        _sensed = true;
    }

    private void Sense()
    {
        if (!_sensed)
            return;

        _sensed = false;

        CameraController.Instance.target = PlayerChar.transform;

        PlayerChar.StateMachine.Initialize(PlayerChar.IdleState);
        CompanionChar.StateMachine.Initialize(CompanionChar.IdleState);

        CompanionChar.Anim.enabled = true;
        CompanionChar.CurrentDialogue = _companionDialogue9;
    }

    private void FinishEvent()
    {
        EventIsDone = true; // event done

        DialogueController.Instance.OnDialogueFinish -= FinishEvent;
    }
}
