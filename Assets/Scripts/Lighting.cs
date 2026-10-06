using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class Lighting : MonoBehaviour
{
    public static Lighting Instance { get; private set; } // Singleton instance

    [SerializeField] private Gradient _lightColor;
    [SerializeField] private Light2D _light;

    [SerializeField] private float _time; // default is 500
    [SerializeField] private float _dayLum, _nightLum; // default is 1f, .4f

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject); // Make sure only one instance
    }

    public void SetLighting(float time, float intensity)
    {
        _light.color = _lightColor.Evaluate(time * (1f/_time));
        _light.intensity = intensity;
    }
}
