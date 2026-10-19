using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Open : Action
{
    [SerializeField] private Dialogue _dialogue;
    private bool _opened;

    private void OnTriggerEnter2D(Collider2D hitInfo)
    {
        if (hitInfo.GetComponent<Player>() && !_detectPlayer && !_opened)
        {
            _detectPlayer = hitInfo.GetComponent<Player>();

            if (_detectPlayer.StateMachine.CurrentState == _detectPlayer.IdleState)
            {
                _detectPlayer.IsNearInteractable = true;

                Vector2 iconPos = new Vector2(transform.position.x, transform.position.y+.05f);
                _activeIcon = Instantiate(_icon, iconPos, Quaternion.identity, transform);
            }

            // set delegate
            _detectPlayer.OnAction += Active;
        }
    }

    private void OnTriggerExit2D(Collider2D hitInfo)
    {
        if (hitInfo.GetComponent<Player>())
        {
            Destroy(_activeIcon);
            _detectPlayer = hitInfo.GetComponent<Player>();
            _detectPlayer.OnAction -= Active; // unset delegate
            _detectPlayer.IsNearInteractable = false;
            _detectPlayer = null;
        }
    }

    protected override void Active()
    {
        StartCoroutine(OpenChest());
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

    protected override void Finish()
    {
        if (!_detectPlayer)
            return;

        _detectPlayer.StateMachine.Initialize(_detectPlayer.IdleState); // enable movement

        OnTriggerExit2D(_detectPlayer.GetComponent<Collider2D>());
    }
}
