using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandleMovement : MonoBehaviour
{
    Rigidbody2D rb;
    StateManager states;
    HandleAnimations anim;

    public float acceleration = 30;
    public float ariAcceleration = 15;
    public float maxSpeed = 20;
    public float jumpSpeed = 8;
    public float jumpDuration = 150;
    float actualSpeed;
    bool justJumped;
    bool canVariableJump;
    float jmpTimer;
    [SerializeField] private LayerMask collisionMask;

    [Header("Dashing")]
    [SerializeField] private float _dashingTime = 0.5f;
    [SerializeField] private int maxDashes = 2;
    private int _dashesLeft;
    private TrailRenderer _trailRenderer;
    public Vector2 _dashingDir;
    public bool _isDashing;
    private Animator _animator;
    [SerializeField] private float groundDashVelocity = 14f;
    [SerializeField] private float airDashVelocity = 30f;
    AudioManager audioManager;
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        states = GetComponent<StateManager>();
        anim = GetComponent<HandleAnimations>();
        rb.freezeRotation = true;
        _trailRenderer = GetComponent<TrailRenderer>();
        _dashesLeft = maxDashes;
    }
    private void Awake()
    {
        audioManager = GameObject.FindGameObjectWithTag("Audio").GetComponent<AudioManager>();
    }
    void FixedUpdate()
    {
        Dash();
        if (!states.dontMove)
        {
            HorizontalMovement();
            Jump();
        }
        if (states.currentlyAttacking)
        {
            rb.velocity = Vector2.zero; // frenar en seco
            return;
        }
        if (states.onGround)
        {
            _dashesLeft = maxDashes;
        }


        if (states.gettingBlock || states.gettingHit)
        {
            states.dontMove = true;
            return;
        }
        else
        {
            states.dontMove = false;
        }
    }

    void Dash()
    {
        if (states.dash && !_isDashing && _dashesLeft > 0)
        {
            _isDashing = true;
            _trailRenderer.emitting = true;
            _dashesLeft--;

            Vector2 inputDir = new Vector2(states.horizontal, states.vertical).normalized;
            _dashingDir = inputDir == Vector2.zero ? Vector2.right * transform.localScale.x : inputDir;

            float dashSpeed = states.onGround ? groundDashVelocity : airDashVelocity;
            rb.velocity = _dashingDir * dashSpeed;

            anim.DashAnim();
            audioManager.PlaySFXWithVolume(audioManager.dash, 0.01f);
            if (states.block)
            {
                anim.BackDashAnim();
            }
            else
            {
                anim.FrontDashAnim();
            }
            StartCoroutine(StopDashing());
        }
        

            states.dash = false;
    }

    private IEnumerator StopDashing()
    {
        yield return new WaitForSeconds(_dashingTime);
        _trailRenderer.emitting = false;
        _isDashing = false;
        anim.finishdash(); 
        if (!states.onGround)
        {
            rb.velocity = new Vector2(0, rb.velocity.y);
        }
    }

    public void PushCharacter(Vector2 direction, float force)
    {
        Vector2 impulse = direction.normalized * force;
        rb.AddForce(new Vector2(impulse.x, 0f), ForceMode2D.Impulse);

        if (states.dontMove)
        {
            float maxX = 2f;
            float maxY = 10f;
            rb.velocity = new Vector2(Mathf.Clamp(rb.velocity.x, -maxX, maxX), Mathf.Clamp(rb.velocity.y, -maxY, maxY));
        }
    }

    IEnumerator ApplyGradualPush(Vector2 force, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            rb.AddForce(force * Time.deltaTime / duration, ForceMode2D.Impulse);
            elapsed += Time.deltaTime;
            yield return null;
        }
    }

    void HorizontalMovement()
    {
        actualSpeed = maxSpeed;

        bool movingBackward = (states.horizontal < 0 && states.lookRight) || (states.horizontal > 0 && !states.lookRight);
        states.block = movingBackward;

        if (states.onGround && !states.currentlyAttacking && !_isDashing)
        {
            rb.AddForce(new Vector2((states.horizontal * actualSpeed) - rb.velocity.x * acceleration, 0));
        }

        if (states.horizontal == 0 && states.onGround && !_isDashing && !states.gettingHit)
        {
            rb.velocity = new Vector2(0, rb.velocity.y);
        }

        if (states.block)  // Corregido: antes tenías `if (block = true)` lo cual era una asignación, no una comparación
        {

            states.dontMove = false;

            if (states.gettingBlock)
            {
                states.dontMove = true;
            }
        }
    }

    void Jump()
    {
        if (states.vertical > 0)
        {
            if (!justJumped)
            {
                justJumped = true;

                if (states.onGround)
                {
                    anim.JumpAnim();
                    audioManager.PlaySFXWithVolume(audioManager.jump, 0.1f);
                    float jumpX = states.horizontal * 1f;
                    rb.velocity = new Vector2(jumpX, jumpSpeed);

                    jmpTimer = 0;
                    canVariableJump = true;
                }
            }
            else if (canVariableJump)
            {
                jmpTimer += Time.deltaTime;
                if (jmpTimer < jumpDuration / 1000f)
                {
                    rb.velocity = new Vector2(rb.velocity.x, jumpSpeed);
                }
            }
        }
        else
        {
            justJumped = false;
        }
    }

    public void AddVelocityOnCharacter(Vector2 direction, float power)
    {
        rb.velocity = Vector2.zero;
        rb.AddForce(direction.normalized * power, ForceMode2D.Impulse);
    }

    IEnumerator AddVelocity(float timer, Vector3 direction)
    {
        float t = 0;
        while (t < timer)
        {
            t += Time.deltaTime;
            rb.AddForce(direction * 5);
            yield return null;
        }
    }
}
