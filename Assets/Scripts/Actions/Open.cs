using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Open : MonoBehaviour
{
    private Player _detectPlayer;
    private GameObject _activeIcon;
    [SerializeField] private GameObject _icon;
    [SerializeField] private Dialogue _dialogue;
    private bool _opened;

    private void Start()
    {
        // set dialogue delegates
        DialogueController.Instance.OnDialogueFinish += FinishOpen;
    }

    private void OnTriggerEnter2D(Collider2D hitInfo)
    {
        if (hitInfo.GetComponent<Player>() && !_detectPlayer && !_opened)
        {
            _detectPlayer = hitInfo.GetComponent<Player>();
            _detectPlayer.IsNearInteractable = true;

            Vector2 iconPos = new Vector2(transform.position.x, transform.position.y+.05f);
            _activeIcon = Instantiate(_icon, iconPos, Quaternion.identity, transform);
        }
    }

    private void OnTriggerExit2D(Collider2D hitInfo)
    {
        if (hitInfo.GetComponent<Player>())
        {
            Destroy(_activeIcon);

            _detectPlayer = hitInfo.GetComponent<Player>();
            _detectPlayer.IsNearInteractable = false;
            _detectPlayer = null;
        }
    }

    private void Update()
    {
        if (_detectPlayer && _detectPlayer.StateMachine.CurrentState == _detectPlayer.IdleState)
        {
            if (_detectPlayer.Input.E)
            {
                StartCoroutine(OpenChest());
            }
        }
    }

    public IEnumerator OpenChest()
    {
        _activeIcon.SetActive(false);
        GetComponentInParent<Animator>().SetTrigger("Open");
        _opened = true;

        _detectPlayer.StateMachine.End(); // disable movement

        yield return new WaitForSeconds(.5f);

        // start dialogue
        DialogueController.Instance.StartDialogue(_dialogue, new List<CharacterBase>());
    }

    private void FinishOpen()
    {
        if (!_detectPlayer)
            return;

        _detectPlayer.StateMachine.Initialize(_detectPlayer.IdleState); // enable movement

        OnTriggerExit2D(_detectPlayer.GetComponent<Collider2D>());
    }
}
