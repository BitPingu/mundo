using UnityEngine;

public class Boundary : MonoBehaviour
{
    public Player DetectPlayer { get; set; }

    private void OnTriggerEnter2D(Collider2D hitInfo)
    {
        if (hitInfo.GetComponent<Player>())
        {
            DetectPlayer = null;
        }
    }

    private void OnTriggerExit2D(Collider2D hitInfo)
    {
        if (hitInfo.GetComponent<Player>() && !DetectPlayer)
        {
            DetectPlayer = hitInfo.GetComponent<Player>();
        }
    }
}
