using UnityEngine;

public class React : MonoBehaviour
{
    [SerializeField] private AudioClip _sound;
    public bool Mute { get; set; }

    private void Start()
    {
        if (!Mute)
            SFXManager.Play(_sound);
    }
}
