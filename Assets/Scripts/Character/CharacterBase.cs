using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(SpriteRenderer))]
public class CharacterBase : MonoBehaviour
{
    // components
    public Rigidbody2D RB { get; set; }
    public Animator Anim { get; set; }
    public SpriteRenderer Sprite { get; set; }

    // states
    public StateMachine StateMachine { get; set; }
    public StateBase IdleState { get; set; }

    // conditions
    public bool IsNearPlayer { get; set; }

    // dialogue
    public Dialogue CurrentDialogue { get; set; }
    public string charName;
    [SerializeField] private List<Portrait> _portraits;
    public Dictionary<string, Sprite> portraits = new Dictionary<string, Sprite>();
    public AudioClip voiceSound;
    public float voicePitch = 1f;

    [SerializeField] private float _moveSpeed = 3f;

    protected virtual void Awake()
    {
        // set components
        RB = GetComponent<Rigidbody2D>();
        Anim = GetComponent<Animator>();
        Sprite = GetComponent<SpriteRenderer>();

        // set portraits
        foreach (Portrait portrait in _portraits)
        {
            portraits.Add(portrait.reaction, portrait.sprite);
        }
    }

    protected virtual void Start()
    {
        // state machine
        StateMachine = new StateMachine();

        // states
        IdleState = new IdleState(this, StateMachine);

        // start in idle state
        StateMachine.Initialize(IdleState);
    }

    protected virtual void Update()
    {
        // run current state
        StateMachine.CurrentState?.FrameUpdate();
    }

    public void ChangeState(StateBase newState)
    {
        StateMachine.ChangeState(newState);
    }

    public virtual void Idle() { }

    public virtual void Move(Vector2 inputVector)
    {
        // normalize
        inputVector = inputVector.normalized;

        // face direction (when idle)
        if (StateMachine.CurrentState == IdleState)
        {
            if (inputVector.x > 0)
                Sprite.flipX = false;
            else if (inputVector.x < 0)
                Sprite.flipX = true;
        }

        // move
        RB.linearVelocity = inputVector * _moveSpeed;

        // animate
        if (Anim.runtimeAnimatorController)
            Anim.SetFloat("Movement", RB.linearVelocity.magnitude / _moveSpeed);
    }

    public void Face(CharacterBase character)
    {
        if (character.transform.position.x > transform.position.x)
            Sprite.flipX = false;
        else if (character.transform.position.x < transform.position.x)
            Sprite.flipX = true;
    }
}

[System.Serializable]
public class Portrait
{
    public Sprite sprite;
    public string reaction;
}

