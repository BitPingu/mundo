using UnityEngine;

public class Battle : Action
{
    private Enemy _enemy, _nearbyEnemy;

    protected override void Start()
    {
        // call base class
        base.Start();

        if (GetComponentInParent<Enemy>())
        {
            _enemy = GetComponentInParent<Enemy>();
        }

        // set delegate
        _enemy.OnEngagePlayer += EngagePlayer;
    }

    private void OnTriggerEnter2D(Collider2D hitInfo)
    {
        if (hitInfo.GetComponent<Player>() && !_detectPlayer)
        {
            _detectPlayer = hitInfo.GetComponent<Player>();

            if (_detectPlayer.StateMachine.CurrentState == _detectPlayer.IdleState)
            {
                _detectPlayer.IsNearEnemy = true;
                _enemy.IsNearPlayer = true;

                Vector2 iconPos = new Vector2(transform.position.x, transform.position.y);
                _activeIcon = Instantiate(_icon, iconPos, Quaternion.identity, transform);

                if (_nearbyEnemy)
                {
                    // trigger for nearby enemies
                    _nearbyEnemy.GetComponentInChildren<Battle>().OnTriggerEnter2D(_detectPlayer.GetComponent<Collider2D>());
                }
            }

            // set delegate
            _detectPlayer.OnAction += EngageEnemy;
        }

        // nearby enemy
        if (hitInfo.GetComponent<Enemy>())
        {
            _nearbyEnemy = hitInfo.GetComponent<Enemy>();
        }
    }

    private void OnTriggerExit2D(Collider2D hitInfo)
    {
        if (hitInfo.GetComponent<Player>() && _detectPlayer)
        {
            _detectPlayer = hitInfo.GetComponent<Player>();
            _detectPlayer.OnAction -= EngageEnemy; // unset delegate
            ExitBattle(hitInfo);
            if (_nearbyEnemy)
                _nearbyEnemy.GetComponentInChildren<Battle>().ExitBattle(hitInfo);
        }
    }

    private void Update()
    {
        if (_detectPlayer && _nearbyEnemy)
        {
            // check if ally is closer to player
            if (Vector2.Distance(_detectPlayer.transform.position, _nearbyEnemy.transform.position) < Vector2.Distance(_detectPlayer.transform.position, _enemy.transform.position))
            {
                _nearbyEnemy.IsNearPlayer = true;
                _enemy.IsNearPlayer = false;
            }
            else
            {
                _enemy.IsNearPlayer = true;
                _nearbyEnemy.IsNearPlayer = false;
            }
        }
    }

    private void ExitBattle(Collider2D hitInfo)
    {
        Destroy(_activeIcon);
        _detectPlayer = hitInfo.GetComponent<Player>();
        _detectPlayer.IsNearEnemy = false;
        _enemy.IsNearPlayer = false;
        _detectPlayer = null;
    }

    private void EngageEnemy()
    {
        if (!_enemy.IsNearPlayer)
            return;

        if (_nearbyEnemy)
        {
            // enemy allies
            _enemy.Allies.Add(_nearbyEnemy);
            _nearbyEnemy.Allies.Add(_enemy);
        }
    
        StartCoroutine(_detectPlayer.Engage(_enemy)); // player engages enemy
        OnTriggerExit2D(_detectPlayer.GetComponent<Collider2D>());

        // camera
        CameraController.Instance.follow = false;
    }

    private void EngagePlayer(Collider2D hitInfo)
    {
        _detectPlayer = hitInfo.GetComponent<Player>();
        StartCoroutine(_enemy.Engage(_detectPlayer)); // enemy engages player
        OnTriggerExit2D(_detectPlayer.GetComponent<Collider2D>());

        // camera
        CameraController.Instance.follow = false;
    }
}
