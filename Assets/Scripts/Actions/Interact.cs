using System.Collections.Generic;
using UnityEngine;

public class Interact : MonoBehaviour
{
    private Player _detectPlayer;
    private GameObject _activeIcon;
    [SerializeField] private GameObject _icon;
    [SerializeField] private Dialogue _dialogue;

    public delegate void Interacted();
    public Interacted OnInteract;

    private void Start()
    {
        // set dialogue delegates
        DialogueController.Instance.OnDialogueFinish += FinishInteract;
    }

    private void OnTriggerEnter2D(Collider2D hitInfo)
    {
        if (hitInfo.GetComponent<Player>() && !_detectPlayer)
        {
            _detectPlayer = hitInfo.GetComponent<Player>();

            Vector2 iconPos = new Vector2(_detectPlayer.transform.position.x, _detectPlayer.transform.position.y+1f);
            _activeIcon = Instantiate(_icon, iconPos, Quaternion.identity, _detectPlayer.transform);
        }
    }

    private void OnTriggerExit2D(Collider2D hitInfo)
    {
        if (hitInfo.GetComponent<Player>())
        {
            Destroy(_activeIcon);

            _detectPlayer = null;
        }
    }

    private void Update()
    {
        if (_detectPlayer && _detectPlayer.StateMachine.CurrentState == _detectPlayer.IdleState)
        {
            if (_detectPlayer.Input.E)
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

    private void FinishInteract()
    {
        if (!_detectPlayer)
            return;

        _detectPlayer.StateMachine.Initialize(_detectPlayer.IdleState); // enable movement
    }
}
