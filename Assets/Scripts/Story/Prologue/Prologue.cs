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
    [SerializeField] protected Companion _friend;
    [SerializeField] protected Villager _mom, _chief, _shopkeeper, _traveller;
    [SerializeField] protected Enemy _slime, _slime1, _slime2, _slime3;
    [SerializeField] protected VillagerDialogue[] _vDialogue;
    [SerializeField] protected GameObject _chiefHouse, _chiefHouseIndoor, _chest, _vines, _crystal;
    [SerializeField] protected Boundary _matchBoundary, _slimeBoundary;
    [SerializeField] protected Destination _momTrigger, _friendTrigger, _chiefTrigger, _encounterTrigger, _ambushTrigger;

    public override void BeginChapter()
    {
        // TODO: temp
        // _friend.Join(_player);

        UpdateDialogue(0); // init dialogue
        base.BeginChapter();
    }

    public override void SetupEvent()
    {
        base.SetupEvent();

        if (CurrentEvent.GetComponent<SparringMatch>())
        {
            CurrentEvent.GetComponent<SparringMatch>().PlayerChar = _player;
            CurrentEvent.GetComponent<SparringMatch>().Friend = _friend;
            CurrentEvent.GetComponent<SparringMatch>().Mom = _mom;
            CurrentEvent.GetComponent<SparringMatch>().Chief = _chief;
            CurrentEvent.GetComponent<SparringMatch>().House = _chiefHouse;
            CurrentEvent.GetComponent<SparringMatch>().HouseIndoor = _chiefHouseIndoor;
            CurrentEvent.GetComponent<SparringMatch>().MatchBoundary = _matchBoundary;
            CurrentEvent.GetComponent<SparringMatch>().MomTrigger = _momTrigger;
            CurrentEvent.GetComponent<SparringMatch>().FriendTrigger = _friendTrigger;
            CurrentEvent.GetComponent<SparringMatch>().ChiefTrigger = _chiefTrigger;
        }
        else if (CurrentEvent.GetComponent<FirstQuest>())
        {
            UpdateDialogue(1); // update dialogue
            CurrentEvent.GetComponent<FirstQuest>().PlayerChar = _player;
            CurrentEvent.GetComponent<FirstQuest>().Friend = _friend;
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
            CurrentEvent.GetComponent<FirstQuest>().SlimeBoundary = _slimeBoundary;
            CurrentEvent.GetComponent<FirstQuest>().EncounterTrigger = _encounterTrigger;
            CurrentEvent.GetComponent<FirstQuest>().AmbushTrigger = _ambushTrigger;
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
