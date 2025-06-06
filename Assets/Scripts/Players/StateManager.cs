using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StateManager : MonoBehaviour
{

    public int health = 100;

    public float horizontal;
    public float vertical;
    public bool attack1;
    public bool attack2;
    public bool attack3;
    public bool crouch;
    public bool block;
    public bool crouchblock;
    public bool canAttack;
    public bool gettingHit;
    public bool currentlyAttacking;
    public bool dash;
    public bool dontMove;
    public bool onGround;
    public bool lookRight;
    public bool gettingBlock;
    public GameObject lightHitSparkPrefab;
    public GameObject heavyHitSparkPrefab;
    public Slider healthSlider;
    SpriteRenderer sRenderer;
    Animator anim;
    [HideInInspector]
    public HandleDamageColliders handleDC;
    [HideInInspector]
    public HandleAnimations handleAnim;
    [HideInInspector]
    public HandleMovement handleMovement;
    public StateManager stateManager;
    public GameObject[] movementColliders;
    AudioManager audioManager;
    void Awake()
    {
        GameObject audioObj = GameObject.FindGameObjectWithTag("Audio");
        if (audioObj != null)
        {
            audioManager = audioObj.GetComponent<AudioManager>();
        }
        else
        {
            // No es un error fatal en escenas como selección de personajes
            Debug.Log("AudioManager no encontrado. Probablemente estás en una escena que no lo necesita.");
        }
        handleAnim = GetComponentInChildren<HandleAnimations>();
        if (handleAnim == null)
        {
            Debug.LogWarning("HandleAnimations no encontrado en " + gameObject.name + ". Puede que este personaje esté en modo preview.");
        }

    }

    void Start()
    {
         anim = GetComponent<Animator>();
        handleDC = GetComponent<HandleDamageColliders>();
        handleMovement = GetComponent<HandleMovement>();
        sRenderer = GetComponentInChildren<SpriteRenderer>();

        if (handleAnim == null)
        {
            Debug.LogError("handleAnim is null in StateManager on " + gameObject.name);
        }
        else if (handleAnim.anim == null)
        {
            Debug.LogError("handleAnim.anim is null in StateManager on " + gameObject.name);
        }
    }

    void FixedUpdate()
    {
        sRenderer.flipX = lookRight;
        onGround = isOnGround();
        dontMove = gettingBlock || gettingHit || attack1 || attack2;
        if (healthSlider != null)
        {
            healthSlider.value = health * 0.01f;
        }

        if (health <= 0)
        {
            LevelManager lm = LevelManager.getInstance();
            if (lm != null && lm.countdown)
            {
                lm.EndTurnFunction();

                if (handleAnim != null && handleAnim.anim != null)
                {
                    handleAnim.anim.Play("Dead");
                }
                else
                {
                    Debug.LogError("Null reference: handleAnim or handleAnim.anim is null on " + gameObject.name);
                }
            }
        }
    }

    bool isOnGround()
    {
        bool retVal = false;

        LayerMask layer = ~(1 << gameObject.layer | 1 << 3);
        retVal = Physics2D.Raycast(transform.position, -Vector2.up, 0.1f, layer);

        return retVal;
    }

    public void ResetStateInputs()
    {
        horizontal = 0;
        vertical = 0;
        attack1 = false;
        attack2 = false;
        attack3 = false;
        gettingHit = false;
        currentlyAttacking = false;
        dontMove = true;
        dash = false;
        block = false;
        crouchblock = false;
        gettingBlock = false;
    }

    public void CloseMovementCollider(int index)
    {
        movementColliders[index].SetActive(false);
    }

    public void OpenMovementCollider(int index)
    {
        movementColliders[index].SetActive(true);
    }

    public void TakeDamage(int damage, HandleDamageColliders.DamageType damageType, StateManager attacker)
    {
       

        if (!gettingHit && !block)
        {
            GameObject sparkToSpawn = null;
            bool grounded = onGround;

            switch (damageType)
            {
                case HandleDamageColliders.DamageType.light:
                    StartCoroutine(HitPauseEffect(0.05f, 0.15f, 0.3f));
                    sparkToSpawn = lightHitSparkPrefab;
                    Vector2 pushDir = ((!lookRight) ? Vector2.right : Vector2.left);
                    float pushForce = grounded ? 2f : 4f;
                    handleMovement.PushCharacter(pushDir, pushForce);
                    StartCoroutine(CloseImmortality(0.3f));
                    audioManager.PlaySFXWithVolume(audioManager.lighthit, 0.2f);
                    if (CameraShake.instance != null)
                        StartCoroutine(CameraShake.instance.Shake(0.2f, 0.2f));
                    break;

                case HandleDamageColliders.DamageType.heavy:
                    StartCoroutine(HitPauseEffect(0.05f, 0.15f, 0.3f));
                    sparkToSpawn = heavyHitSparkPrefab;
                    Vector2 launchDir = ((!lookRight) ? Vector2.right * 0.01f : Vector2.left * 0.01f) + Vector2.up;
                    float launchForce = grounded ? 4.3f : 4.3f;
                    handleMovement.AddVelocityOnCharacter(launchDir, launchForce);
                    StartCoroutine(CloseImmortality(0.5f));
                    audioManager.PlaySFXWithVolume(audioManager.heavyhit, 0.3f);
                    if (CameraShake.instance != null)
                        StartCoroutine(CameraShake.instance.Shake(0.4f, 0.6f));
                    break;
            }

            if (sparkToSpawn != null)
            {
                Vector3 sparkPos = transform.position + Vector3.up * 0.4f;
                GameObject spark = Instantiate(sparkToSpawn, sparkPos, Quaternion.identity);
                spark.transform.SetParent(transform);
            }

            health -= damage;
            gettingHit = true;
        }
        else if (!gettingHit && block)
        {
            GameObject sparkToSpawn = null;
            bool grounded = onGround;

            switch (damageType)
            {
                case HandleDamageColliders.DamageType.light:
                    StartCoroutine(HitPauseEffect(0.05f, 0.15f, 0.3f));
                    sparkToSpawn = lightHitSparkPrefab;
                    Vector2 pushDir = ((!lookRight) ? Vector2.right : Vector2.left);
                    float pushForce = grounded ? 1f : 1f;
                    handleMovement.PushCharacter(pushDir, pushForce);

                    // 👇 Knockback para el atacante
                    if (attacker != null)
                    {
                        Vector2 attackerKnockback = (attacker.lookRight ? Vector2.left : Vector2.right);
                        attacker.handleMovement.PushCharacter(attackerKnockback, 2f);
                    }

                    StartCoroutine(CloseImmortality(0.3f));
                    audioManager.PlaySFXWithVolume(audioManager.block, 0.05f);
                    gettingBlock = true;

                    if (CameraShake.instance != null)
                        StartCoroutine(CameraShake.instance.Shake(0.1f, 0.1f));
                    break;

                case HandleDamageColliders.DamageType.heavy:
                    StartCoroutine(HitPauseEffect(0.05f, 0.15f, 0.3f));
                    sparkToSpawn = heavyHitSparkPrefab;
                    Vector2 launchDir = ((!lookRight) ? Vector2.right * 0.01f : Vector2.left * 0.01f) + Vector2.up;
                    float launchForce = grounded ? 4.3f : 4.3f;
                    handleMovement.AddVelocityOnCharacter(launchDir, launchForce);

                    // 👇 Knockback para el atacante
                    if (attacker != null)
                    {
                        Vector2 attackerKnockback = (attacker.lookRight ? Vector2.left : Vector2.right);
                        attacker.handleMovement.PushCharacter(attackerKnockback, 3f);
                    }

                    StartCoroutine(CloseImmortality(0.5f));
                    audioManager.PlaySFXWithVolume(audioManager.block, 0.05f);
                    gettingBlock = true;

                    if (CameraShake.instance != null)
                        StartCoroutine(CameraShake.instance.Shake(0.2f, 0.2f));
                    break;
            }


            if (sparkToSpawn != null)
            {
                Vector3 sparkPos = transform.position + Vector3.up * 0.4f;
                GameObject spark = Instantiate(sparkToSpawn, sparkPos, Quaternion.identity);
                spark.transform.SetParent(transform);
            }

            StartCoroutine(CloseBlocking(0.3f));
        }
    }

    public void TakeDamage(int damage, HandleDamageColliders.DamageType damageType)
    {
        TakeDamage(damage, damageType, null); // Llama a la versión principal sin atacante
    }
    IEnumerator CloseBlocking(float timer)
    {
        yield return new WaitForSeconds(timer);
        gettingBlock = false;
    }
    IEnumerator CloseImmortality(float timer)
    {
        yield return new WaitForSeconds(timer);
        gettingHit = false;
    }
    IEnumerator HitPauseEffect(float freezeTime, float slowmoTime, float slowmoScale)
    {
        // ⏸️ Freeze frame
        Time.timeScale = 0f;
        yield return new WaitForSecondsRealtime(freezeTime);

        // 🐢 Slow motion
        Time.timeScale = slowmoScale;
        yield return new WaitForSecondsRealtime(slowmoTime);

        // 🔁 Volver a la normalidad
        Time.timeScale = 1f;
    }
}