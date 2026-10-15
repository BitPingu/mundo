using UnityEngine;

public class Chase : Action
{
    public Player NearbyPlayer { get; set; }

    protected override void Start()
    {
        // set delegate
        GetComponentInParent<Enemy>().OnEngagePlayer += OnTriggerExit2D;
    }

    private void OnTriggerEnter2D(Collider2D hitInfo)
    {
        if (hitInfo.GetComponent<Player>() && !_detectPlayer)
        {
            _detectPlayer = hitInfo.GetComponent<Player>();
        }
    }

    private void OnTriggerExit2D(Collider2D hitInfo)
    {
        if (hitInfo.GetComponent<Player>() && _detectPlayer)
        {
            _detectPlayer = hitInfo.GetComponent<Player>();
            _detectPlayer = null;
        }
    }

    private void Update()
    {
        if (_detectPlayer)
        {
            if (_detectPlayer.StateMachine.CurrentState == _detectPlayer.IdleState)
            {
                NearbyPlayer = _detectPlayer;
            }
            else
            {
                NearbyPlayer = null;
            }
        }
    }
}
