using System.Collections.Generic;
using UnityEngine;

public class Talk : Action
{
    private CharacterBase _character, _nearbyCharacter;
    private bool _talking;

    protected override void Start()
    {
        // call base class
        base.Start();

        if (GetComponentInParent<CharacterBase>())
        {
            _character = GetComponentInParent<CharacterBase>();
        }
    }

    private void OnTriggerEnter2D(Collider2D hitInfo)
    {
        if (hitInfo.GetComponent<Player>() && !_detectPlayer)
        {
            _detectPlayer = hitInfo.GetComponent<Player>();

            // set delegate
            _detectPlayer.OnAction += Engage;
        }

        // nearby character
        if (hitInfo.GetComponent<CharacterBase>() && hitInfo.GetComponent<CharacterBase>().GetComponentInChildren<Talk>())
        {
            _nearbyCharacter = hitInfo.GetComponent<CharacterBase>();
        }
    }

    private void OnTriggerExit2D(Collider2D hitInfo)
    {
        if (hitInfo.GetComponent<Player>() && _detectPlayer)
        {
            Destroy(_activeIcon);
            _detectPlayer = hitInfo.GetComponent<Player>();
            _detectPlayer.OnAction -= Engage; // unset delegate
            _detectPlayer = null;
        }
    }

    private void Update()
    {
        if (_detectPlayer)
        {
            // check if close enough to player
            // cancel when other actions are available
            if (_detectPlayer.StateMachine.CurrentState == _detectPlayer.IdleState
                && Vector2.Distance(_detectPlayer.transform.position, _character.transform.position) < .7f 
                    && !_detectPlayer.IsEntering && !_detectPlayer.IsNearEnemy && !_detectPlayer.IsNearInteractable)
            {
                if (!_activeIcon)
                {
                    Vector2 iconPos = new Vector2(transform.position.x, transform.position.y);
                    _activeIcon = Instantiate(_icon, iconPos, Quaternion.identity, transform);
                }
                else
                {
                    _activeIcon.SetActive(true);
                }

                if (_nearbyCharacter)
                {
                    // check if other character is closer to player
                    if (Vector2.Distance(_detectPlayer.transform.position, _nearbyCharacter.transform.position) < Vector2.Distance(_detectPlayer.transform.position, _character.transform.position))
                    {
                        _nearbyCharacter.IsNearPlayer = true;
                        _character.IsNearPlayer = false;
                        _activeIcon.SetActive(false);
                    }
                    else
                    {
                        _character.IsNearPlayer = true;
                        _nearbyCharacter.IsNearPlayer = false;
                    }
                }
                else
                {
                    _character.IsNearPlayer = true;
                }
            }
            else
            {
                if (_activeIcon)
                    _activeIcon.SetActive(false);
                _character.IsNearPlayer = false;
            }
        }

        if (_detectPlayer && _detectPlayer.StatusOn)
        {
            _activeIcon.SetActive(false);
        }
    }

    protected override void Engage()
    {
        if (!_character.IsNearPlayer)
            return;
        
        _activeIcon.SetActive(false);

        if (_character.GetComponent<Companion>())
        {
            _character.Anim.Rebind(); // TODO: workaround when using back sprites
        }

        if (_character && _character.Anim)
            _character.Anim.enabled = false;

        _detectPlayer.Face(_character);
        _character.Face(_detectPlayer);

        _detectPlayer.StateMachine.End(); // disable movement
        _character.StateMachine.End(); // disable movement

        // get characters in dialogue
        List<CharacterBase> characters = new List<CharacterBase>{_character, _detectPlayer};
        if (_detectPlayer.Allies.Count > 0)
        {
            foreach (PartyBase ally in _detectPlayer.Allies)
            {
                if (!characters.Contains(ally))
                    characters.Add(ally);
            }
        }

        // start dialogue
        DialogueController.Instance.StartDialogue(_character.CurrentDialogue, characters);
        _talking = true;
    }

    protected override void Finish()
    {
        if (!_talking)
            return;

        if (_character && _character.Anim)
            _character.Anim.enabled = true;

        _detectPlayer.StateMachine.Initialize(_detectPlayer.IdleState); // enable movement
        _character.StateMachine.Initialize(_character.IdleState); // enable movement
        _talking = false;
    }
}
