using UnityEngine;

public class Battle : MonoBehaviour
{
    private Player _detectPlayer;
    private Enemy _detectEnemy;
    private GameObject _activeIcon;
    [SerializeField] private GameObject _icon;

    private void OnTriggerEnter2D(Collider2D hitInfo)
    {
        if (hitInfo.GetComponent<Player>() && !_detectPlayer)
        {
            _detectPlayer = hitInfo.GetComponent<Player>();

            if (_detectPlayer.StateMachine.CurrentState == _detectPlayer.IdleState)
            {
                _detectPlayer.IsNearEnemy = true;

                Vector2 iconPos = new Vector2(transform.position.x, transform.position.y+.02f);
                _activeIcon = Instantiate(_icon, iconPos, Quaternion.identity, transform);

                if (_detectEnemy)
                {
                    _detectEnemy.GetComponentInChildren<Battle>().OnTriggerEnter2D(_detectPlayer.GetComponent<Collider2D>());
                }
            }
        }

        // nearby enemy
        if (hitInfo.GetComponent<Enemy>())
        {
            _detectEnemy = hitInfo.GetComponent<Enemy>();
        }
    }

    private void OnTriggerExit2D(Collider2D hitInfo)
    {
        if (hitInfo.GetComponent<Player>() && !_detectEnemy)
        {
            ExitBattle(hitInfo);
        }
        else if (hitInfo.GetComponent<Player>() && _detectEnemy)
        {
            ExitBattle(hitInfo);
            _detectEnemy.GetComponentInChildren<Battle>().ExitBattle(hitInfo);
        }
    }

    private void ExitBattle(Collider2D hitInfo)
    {
        Destroy(_activeIcon);

        _detectPlayer = hitInfo.GetComponent<Player>();
        _detectPlayer.IsNearEnemy = false;
        _detectPlayer = null;
    }

    private void Update()
    {
        if (_detectPlayer && _detectPlayer.StateMachine.CurrentState == _detectPlayer.IdleState)
        {
            if (_detectPlayer.Input.E)
            {
                Enemy enemy = GetComponentInParent<Enemy>();
                enemy.InterruptIdle(); // halt enemy movement
                if (_detectEnemy)
                {
                    // enemy allies
                    enemy.Allies.Add(_detectEnemy);
                    _detectEnemy.Allies.Add(enemy);
                    // check if ally is closer to player
                    if (Vector2.Distance(_detectPlayer.transform.position, enemy.transform.position) > Vector2.Distance(_detectPlayer.transform.position, _detectEnemy.transform.position))
                    {
                        enemy = _detectEnemy;
                    }
                }
                StartCoroutine(_detectPlayer.Engage(enemy)); // engage the enemy
                OnTriggerExit2D(_detectPlayer.GetComponent<Collider2D>());
            }
        }
    }
}
