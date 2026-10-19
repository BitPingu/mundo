using UnityEngine;

public class Action : MonoBehaviour
{
    protected Player _detectPlayer;
    protected GameObject _activeIcon;
    [SerializeField] protected GameObject _icon;

    protected virtual void Start()
    {
        // set dialogue delegates
        DialogueController.Instance.OnDialogueFinish += Finish;
    }

    protected virtual void Active()
    {
        
    }

    protected virtual void Finish()
    {
        
    }
}
