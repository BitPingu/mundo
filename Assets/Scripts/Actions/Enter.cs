using System.Collections.Generic;
using UnityEngine;

public class Enter : Action
{
    [SerializeField] private Transform _location;
    [SerializeField] private bool _faceRight;
    [SerializeField] private Dialogue _dialogue;
    [SerializeField] private AudioClip _sound;
    [SerializeField] private float _pitch = 1f;

    private void OnTriggerEnter2D(Collider2D hitInfo)
    {
        if (hitInfo.GetComponent<Player>() && !_detectPlayer)
        {
            _detectPlayer = hitInfo.GetComponent<Player>();

            // set delegate
            _detectPlayer.OnAction += Active;
        }
    }

    private void OnTriggerExit2D(Collider2D hitInfo)
    {
        if (hitInfo.GetComponent<Player>() && _detectPlayer)
        {
            Destroy(_activeIcon);
            _detectPlayer = hitInfo.GetComponent<Player>();
            _detectPlayer.OnAction -= Active; // unset delegate
            _detectPlayer.IsEntering = false;
            _detectPlayer = null;
        }
    }

    private void Update()
    {
        if (_detectPlayer)
        {
            if (_detectPlayer.StateMachine.CurrentState == _detectPlayer.IdleState)
            {
                if (!_activeIcon)
                {
                    Vector2 iconPos = new Vector2(_detectPlayer.transform.position.x, _detectPlayer.transform.position.y+1f);
                    _activeIcon = Instantiate(_icon, iconPos, Quaternion.identity, _detectPlayer.transform);
                }
                else
                {
                    _activeIcon.SetActive(true);
                }

                _detectPlayer.IsEntering = true;
            }
            else
            {
                if (_activeIcon)
                    _activeIcon.SetActive(false);
            }
        }

        if (_detectPlayer && _detectPlayer.StatusOn)
        {
            _activeIcon.SetActive(false);
        }
    }

    protected override void Active()
    {
        if (!_detectPlayer.CannotEnter)
        {
            if (_location == null)
            {
                _detectPlayer.StateMachine.End(); // disable movement
                _activeIcon.SetActive(false);
                // start dialogue
                DialogueController.Instance.StartDialogue(_dialogue, new List<CharacterBase>{_detectPlayer});
            }
            else
            {
                ScreenTransition();
            }
        }
        else if (_detectPlayer.CannotEnter)
        {
            _detectPlayer.OnEnter(); // call delegates
            _activeIcon.SetActive(false);
        }
    }

    private async void ScreenTransition()
    {
        Player player = _detectPlayer;
        player.StateMachine.End(); // disable movement
        _activeIcon.SetActive(false);

        await Transition.Instance.FadeOut();

        SFXManager.Play(_sound, _pitch);

        if (_faceRight)
            player.Sprite.flipX = false;
        else
            player.Sprite.flipX = true;

        // move characters
        player.transform.position = _location.position + new Vector3(_location.GetComponent<BoxCollider2D>().offset.x/2f, _location.GetComponent<BoxCollider2D>().offset.y/2f);
        if (player.Allies.Count > 0 && player.Allies[0])
        {
            player.Allies[0].transform.position = new Vector2(player.transform.position.x+.3f, player.transform.position.y+.1f);
        }

        await Transition.Instance.FadeIn();

        player.StateMachine.Initialize(player.IdleState); // enable movement
    }

    protected override void Finish()
    {
        if (!_detectPlayer)
            return;

        _detectPlayer.StateMachine.Initialize(_detectPlayer.IdleState); // enable movement
    }
}
