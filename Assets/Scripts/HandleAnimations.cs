﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandleAnimations : MonoBehaviour
{
    public Animator anim;
    StateManager states;
    HandleMovement move;
    public float attackRate = .3f;
    public AttacksBase[] attacks = new AttacksBase[2];
    AudioManager audioManager;

    void Awake()
    {
        anim = GetComponentInChildren<Animator>();
        if (anim == null)
        {
            Debug.LogError("Animator is null in HandleAnimations on " + gameObject.name);
        }
    }
    private void awake()
    {
        audioManager = GameObject.FindGameObjectWithTag("Audio").GetComponent<AudioManager>();
    }
    void Start()
    {
        states = GetComponent<StateManager>();
        anim = GetComponentInChildren<Animator>();
    }

    void FixedUpdate()
    {
        

        anim.SetBool("TakesHit", states.gettingHit);
        anim.SetBool("OnAir", !states.onGround);
        anim.SetBool("Crouch", states.crouch);
       
        float movement = Mathf.Abs(states.horizontal);
        anim.SetFloat("Movement", movement);

        if (states.vertical < 0)
        {
            states.crouch = true;
        }
        else
        { 
            states.crouch = false;
        }
        if (states.gettingBlock)
        {
            anim.SetBool("Blocking", true);
        }
        else
        {
            anim.SetBool("Blocking", false);
        }
            HandleAttacks();
    }

    void HandleAttacks()
    {
        if (states.canAttack)
        {
            if (states.attack1)
            {
                attacks[0].attack = true;
                attacks[0].attackTimer = 0;
                attacks[0].timesPressed++;
            }

            if (attacks[0].attack)
            {
                attacks[0].attackTimer += Time.deltaTime;

                if (attacks[0].attackTimer > attackRate || attacks[0].timesPressed >= 3)
                {
                    attacks[0].attackTimer = 0;
                    attacks[0].attack = false;
                    attacks[0].timesPressed = 0;
                }
            }

            if (states.attack2)
            {
                attacks[1].attack = true;
                attacks[1].attackTimer = 0;
                attacks[1].timesPressed++;
            }

            if (attacks[1].attack)
            {
                attacks[1].attackTimer += Time.deltaTime;

                if (attacks[1].attackTimer > attackRate || attacks[1].timesPressed >= 3)
                {
                    attacks[1].attackTimer = 0;
                    attacks[1].attack = false;
                    attacks[1].timesPressed = 0;
                }
            }
        }

        // Animaciones
        anim.SetBool("Attack1", attacks[0].attack);
        anim.SetBool("Attack2", attacks[1].attack);

        // 👇 Esta es la línea clave que debes agregar:
        states.currentlyAttacking = attacks[0].attack || attacks[1].attack;
    }
    public void DashAnim()
    {
        //if (move._dashingDir)
        {

        }
        
    }

    public void BackDashAnim()
    {
        anim.SetBool("Bdash", true);
    }
    public void FrontDashAnim()
    {
        anim.SetBool("Fdash", true);
    }
    public void finishdash()
    {
        anim.SetBool("Fdash", false);
        anim.SetBool("Bdash", false);
    }
    public void JumpAnim()
    { 
        anim.SetBool("Attack1", false);
        anim.SetBool("Jump", true);
        StartCoroutine(CloseBoolInAnim("Jump"));
    }

    IEnumerator CloseBoolInAnim(string name)
    {
        yield return new WaitForSeconds(0.5f);
        anim.SetBool(name, false);
    }
}

[System.Serializable]
public class AttacksBase
{
    public bool attack;
    public float attackTimer;
    public int timesPressed;
}
