using UnityEngine;

public class Chase : MonoBehaviour
{
    public Player NearbyPlayer { get; set; }

    private void Start()
    {
        // set delegate
        GetComponentInParent<Enemy>().OnEngagePlayer += OnTriggerExit2D;
    }

    private void OnTriggerEnter2D(Collider2D hitInfo)
    {
        if (hitInfo.GetComponent<Player>() && !NearbyPlayer
            && hitInfo.GetComponent<Player>().StateMachine.CurrentState == hitInfo.GetComponent<Player>().IdleState)
        {
            NearbyPlayer = hitInfo.GetComponent<Player>();
        }
    }

    private void OnTriggerExit2D(Collider2D hitInfo)
    {
        if (hitInfo.GetComponent<Player>())
        {
            NearbyPlayer = hitInfo.GetComponent<Player>();
            NearbyPlayer = null;
        }
    }
}
