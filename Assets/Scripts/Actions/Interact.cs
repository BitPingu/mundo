using System.Collections.Generic;
using UnityEngine;

public class Interact : Action
{
    [SerializeField] private Dialogue _dialogue;

    public delegate void Interacted();
    public Interacted OnInteract;

    private void OnTriggerEnter2D(Collider2D hitInfo)
    {
        if (hitInfo.GetComponent<Player>() && !_detectPlayer)
        {
            _detectPlayer = hitInfo.GetComponent<Player>();

            if (_detectPlayer.StateMachine.CurrentState == _detectPlayer.IdleState)
            {
                _detectPlayer.IsNearInteractable = true;

                Vector2 iconPos = new Vector2(_detectPlayer.transform.position.x, _detectPlayer.transform.position.y+1f);
                _activeIcon = Instantiate(_icon, iconPos, Quaternion.identity, _detectPlayer.transform);
            }

            // set delegate
            _detectPlayer.OnAction += Engage;
        }
    }

    private void OnTriggerExit2D(Collider2D hitInfo)
    {
        if (hitInfo.GetComponent<Player>())
        {
            Destroy(_activeIcon);
            _detectPlayer = hitInfo.GetComponent<Player>();
            _detectPlayer.OnAction -= Engage; // unset delegate
            _detectPlayer.IsNearInteractable = false;
            _detectPlayer = null;
        }
    }

    private void Update()
    {
        if (_detectPlayer && _detectPlayer.StateMachine.CurrentState == _detectPlayer.IdleState)
        {
            if (_activeIcon && !_activeIcon.activeSelf && !DialogueController.Instance.IsDialogueActive)
            {
                _activeIcon.SetActive(true);
            }
        }

        if (_detectPlayer && _detectPlayer.StatusOn)
        {
            _activeIcon.SetActive(false);
        }
    }

    protected override void Engage()
    {
        if (_dialogue)
        {
            _detectPlayer.StateMachine.End(); // disable movement
            _activeIcon.SetActive(false);
            // start dialogue
            DialogueController.Instance.StartDialogue(_dialogue, new List<CharacterBase>{_detectPlayer});
        }

        OnInteract?.Invoke(); // call delegates
    }

    protected override void Finish()
    {
        if (!_detectPlayer)
            return;

        _detectPlayer.StateMachine.Initialize(_detectPlayer.IdleState); // enable movement
    }
}
