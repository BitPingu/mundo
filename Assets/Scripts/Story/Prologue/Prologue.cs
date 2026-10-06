using UnityEngine;

[System.Serializable]
public class VillagerDialogue
{
    public Villager villager;
    public Dialogue[] dialogue;
}

public class Prologue : ChapterBase
{
    // characters
    [SerializeField] protected Player _player;
    [SerializeField] protected Companion _companion;
    [SerializeField] protected Villager _mom, _chief, _shopkeeper, _traveller;
    [SerializeField] protected Enemy _slime, _slime1, _slime2, _slime3;
    [SerializeField] protected VillagerDialogue[] _vDialogue;
    [SerializeField] protected GameObject _chiefHouse, _chiefHouseIndoor, _chest, _vines, _crystal, _campfire;
    [SerializeField] protected Boundary _matchBoundary, _slimeBoundary;
    [SerializeField] protected Destination _momTrigger, _companionTrigger, _chiefTrigger, _encounterTrigger, _ambushTrigger, _restTrigger;

    public override void BeginChapter()
    {
        UpdateDialogue(0); // init dialogue
        base.BeginChapter();
    }

    public override void SetupEvent()
    {
        base.SetupEvent();

        if (CurrentEvent.GetComponent<Intro>())
        {
            CurrentEvent.GetComponent<Intro>().PlayerChar = _player;
            CurrentEvent.GetComponent<Intro>().CompanionChar = _companion;
            CurrentEvent.GetComponent<Intro>().Mom = _mom;
            CurrentEvent.GetComponent<Intro>().Chief = _chief;
            CurrentEvent.GetComponent<Intro>().House = _chiefHouse;
            CurrentEvent.GetComponent<Intro>().HouseIndoor = _chiefHouseIndoor;
            CurrentEvent.GetComponent<Intro>().MatchBoundary = _matchBoundary;
            CurrentEvent.GetComponent<Intro>().MomTrigger = _momTrigger;
            CurrentEvent.GetComponent<Intro>().CompanionTrigger = _companionTrigger;
            CurrentEvent.GetComponent<Intro>().ChiefTrigger = _chiefTrigger;
        }
        else if (CurrentEvent.GetComponent<FirstQuest>())
        {
            UpdateDialogue(1); // update dialogue
            CurrentEvent.GetComponent<FirstQuest>().PlayerChar = _player;
            CurrentEvent.GetComponent<FirstQuest>().CompanionChar = _companion;
            CurrentEvent.GetComponent<FirstQuest>().Mom = _mom;
            CurrentEvent.GetComponent<FirstQuest>().Chief = _chief;
            CurrentEvent.GetComponent<FirstQuest>().Shopkeeper = _shopkeeper;
            CurrentEvent.GetComponent<FirstQuest>().Traveller = _traveller;
            CurrentEvent.GetComponent<FirstQuest>().SlimeChar = _slime;
            CurrentEvent.GetComponent<FirstQuest>().SlimeChar1 = _slime1;
            CurrentEvent.GetComponent<FirstQuest>().SlimeChar2 = _slime2;
            CurrentEvent.GetComponent<FirstQuest>().SlimeChar3 = _slime3;
            CurrentEvent.GetComponent<FirstQuest>().HouseIndoor = _chiefHouseIndoor;
            CurrentEvent.GetComponent<FirstQuest>().Chest = _chest;
            CurrentEvent.GetComponent<FirstQuest>().Vines = _vines;
            CurrentEvent.GetComponent<FirstQuest>().Crystal = _crystal;
            CurrentEvent.GetComponent<FirstQuest>().Campfire = _campfire;
            CurrentEvent.GetComponent<FirstQuest>().SlimeBoundary = _slimeBoundary;
            CurrentEvent.GetComponent<FirstQuest>().EncounterTrigger = _encounterTrigger;
            CurrentEvent.GetComponent<FirstQuest>().AmbushTrigger = _ambushTrigger;
            CurrentEvent.GetComponent<FirstQuest>().RestTrigger = _restTrigger;
        }
    }

    private void UpdateDialogue(int index)
    {
        // villagers
        foreach (VillagerDialogue vd in _vDialogue)
        {
            if (index < vd.dialogue.Length)
                vd.villager.CurrentDialogue = vd.dialogue[index];
        }
    }
}
