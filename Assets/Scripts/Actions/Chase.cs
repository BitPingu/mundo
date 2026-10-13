using UnityEngine;

public class Chase : MonoBehaviour
{
    public Player DetectPlayer { get; set; }

    private void OnTriggerEnter2D(Collider2D hitInfo)
    {
        if (hitInfo.GetComponent<Player>() && !DetectPlayer
            && hitInfo.GetComponent<Player>().StateMachine.CurrentState == hitInfo.GetComponent<Player>().IdleState)
        {
            DetectPlayer = hitInfo.GetComponent<Player>();
        }
    }

    private void OnTriggerExit2D(Collider2D hitInfo)
    {
        if (hitInfo.GetComponent<Player>())
        {
            DetectPlayer = hitInfo.GetComponent<Player>();
            DetectPlayer = null;
        }
    }
}
