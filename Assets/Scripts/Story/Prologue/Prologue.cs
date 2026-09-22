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
    [SerializeField] protected Villager _mom, _chief, _shopkeeper;
    [SerializeField] protected Enemy _slime, _slime1, _slime2;
    [SerializeField] protected VillagerDialogue[] _vDialogue;
    [SerializeField] protected GameObject _chiefHouse, _chiefHouseIndoor, _chest;

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

        if (CurrentEvent.GetComponent<TalkToMom>())
        {
            _player.CanEnter = true; // enable enter action
            CurrentEvent.GetComponent<TalkToMom>().PlayerChar = _player;
            CurrentEvent.GetComponent<TalkToMom>().Mom = _mom;
        }
        else if (CurrentEvent.GetComponent<TalkToFriend>())
        {
            CurrentEvent.GetComponent<TalkToFriend>().PlayerChar = _player;
            CurrentEvent.GetComponent<TalkToFriend>().Mom = _mom;
            CurrentEvent.GetComponent<TalkToFriend>().Friend = _friend;
        }
        else if (CurrentEvent.GetComponent<HeadToMatch>())
        {
            _player.CanEnter = false; // disable enter action
            CurrentEvent.GetComponent<HeadToMatch>().PlayerChar = _player;
            CurrentEvent.GetComponent<HeadToMatch>().Friend = _friend;
            CurrentEvent.GetComponent<HeadToMatch>().Chief = _chief;
        }
        else if (CurrentEvent.GetComponent<SparringMatch>())
        {
            CurrentEvent.GetComponent<SparringMatch>().PlayerChar = _player;
            CurrentEvent.GetComponent<SparringMatch>().Friend = _friend;
            CurrentEvent.GetComponent<SparringMatch>().Chief = _chief;
            CurrentEvent.GetComponent<SparringMatch>().House = _chiefHouse;
            CurrentEvent.GetComponent<SparringMatch>().HouseIndoor = _chiefHouseIndoor;
        }
        else if (CurrentEvent.GetComponent<FirstQuest>())
        {
            UpdateDialogue(1); // update dialogue
            _player.CanEnter = true; // enable enter action
            CurrentEvent.GetComponent<FirstQuest>().PlayerChar = _player;
            CurrentEvent.GetComponent<FirstQuest>().Friend = _friend;
            CurrentEvent.GetComponent<FirstQuest>().Mom = _mom;
            CurrentEvent.GetComponent<FirstQuest>().Chief = _chief;
            CurrentEvent.GetComponent<FirstQuest>().Shopkeeper = _shopkeeper;
            CurrentEvent.GetComponent<FirstQuest>().SlimeChar = _slime;
            CurrentEvent.GetComponent<FirstQuest>().SlimeChar1 = _slime1;
            CurrentEvent.GetComponent<FirstQuest>().SlimeChar2 = _slime2;
            CurrentEvent.GetComponent<FirstQuest>().HouseIndoor = _chiefHouseIndoor;
            CurrentEvent.GetComponent<FirstQuest>().Chest = _chest;
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
