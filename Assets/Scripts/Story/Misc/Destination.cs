using UnityEngine;

public class Destination : MonoBehaviour
{
    public bool Reached { get; set; }

    private void OnTriggerEnter2D(Collider2D hitInfo)
    {
        if (hitInfo.GetComponent<Player>())
        {
            Reached = true;
        }
    }
}
