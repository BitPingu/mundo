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
            NearbyPlayer = _detectPlayer;
        }
    }

    private void OnTriggerExit2D(Collider2D hitInfo)
    {
        if (hitInfo.GetComponent<Player>() && _detectPlayer)
        {
            _detectPlayer = hitInfo.GetComponent<Player>();
            _detectPlayer = null;
            NearbyPlayer = _detectPlayer;
        }
    }
}
