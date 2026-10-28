using UnityEngine;

public class Dummy : FighterBase
{
    public override void Die()
    {
        StartCoroutine(Fade());

        string text = charName + " was defeated!";
        DialogueController.Instance.BattleDialogue(this, text, "");
    }
}
