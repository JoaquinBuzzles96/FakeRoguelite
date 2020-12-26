﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum State { None, Melee1, Melee2, Melee3, Cooldown };

public class PlayerController : MonoBehaviour
{
    [SerializeField]
    private float runSpeed = 4;
    [SerializeField]
    private float walkSpeed = 1; // no se utiliza
    [SerializeField]
    private float currentSpeed;
    [SerializeField]
    private float gravity = -9.8f;

    [SerializeField]
    private Transform playerTransform;

    Dictionary<KeyCode, Vector3> directions;
    Dictionary<KeyCode, Quaternion> rotations;

    [SerializeField]
    private float turnSmoothTime = 0.02f;
    float turnSmoothVelocity;

    [SerializeField]
    private float speedSmoothTime = 0.05f;
    float speedSmoothVelocity;
    [SerializeField]
    private float movementVelocityY = 0;

    [SerializeField]
    private float jumpHeight = 1.0f;

    [SerializeField]
    CharacterController controller;
    [SerializeField]
    Transform cameraT;
    [SerializeField]
    Animator anim;

    //MELEE ATTACK
    //public Weapon myWeapon;
    [SerializeField]
    private float attackTimer = 0;
    [SerializeField]
    private float attackDuration = 0; 
    [SerializeField]
    private float limitAttackDuration = 1.0f;
    [SerializeField]
    private float attackCoolDown = 0.05f;
    [SerializeField]
    private State playerState = State.None;

    [SerializeField]
    private KeyCode attackButton = KeyCode.E;

    void Start()
    {
        directions = new Dictionary<KeyCode, Vector3>() {
        {KeyCode.W, Vector3.forward},
        {KeyCode.A, Vector3.left},
        {KeyCode.D, Vector3.right},
        {KeyCode.S, Vector3.back}
         };

        rotations = new Dictionary<KeyCode, Quaternion>() {
        {KeyCode.W, Quaternion.LookRotation(new Vector3(0f, 0f, 1f))},
        {KeyCode.A, Quaternion.LookRotation(new Vector3(-1f, 0f, 0f))},
        {KeyCode.D, Quaternion.LookRotation(new Vector3(1f, 0f, 0f))},
        {KeyCode.S, Quaternion.LookRotation(new Vector3(0f, 0f, -1f))}
         };

        if (controller == null)
        {
            controller = GetComponent<CharacterController>();
        }

        if (cameraT == null)
        {
            cameraT = Camera.main.transform;
        }

        if (anim == null)
        {
            anim = GetComponent<Animator>();
        }

        AnimationClip[] clips = anim.runtimeAnimatorController.animationClips;
        
        foreach (AnimationClip clip in clips)
        {
            //Debug.Log($"- {clip.name} : {clip.length}");
            switch (clip.name)
            {
                case "Melee360":
                    attackDuration = clip.length / 2.5f; // TODO: hacer que este vlaor lo coja del animator
                    break;
            }
        }
        //Debug.Log($"attack duration = {attackDuration}");

    }

    void Update()
    {
        if (playerState != State.Melee1)
        {
            Movement();
        }
        if (playerState != State.Cooldown)
        {
            MeleeAtack();
        }
        

    }

    public void Movement()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Jump();
        }

        Vector2 input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        Vector2 inputDir = input.normalized;

        if (inputDir != Vector2.zero)
        {
            float targetRotation = Mathf.Atan2(inputDir.x, inputDir.y) * Mathf.Rad2Deg + cameraT.eulerAngles.y;
            transform.eulerAngles = Vector3.up * Mathf.SmoothDampAngle(transform.eulerAngles.y, targetRotation, ref turnSmoothVelocity, GetModifiedSmoothTime(turnSmoothTime));
        }

        //bool running = Input.GetKey(KeyCode.LeftShift);
        bool running = true; // de momento siempre vamos corriendo
        float targetSpeed = ((running) ? runSpeed : walkSpeed) * inputDir.magnitude;
        currentSpeed = Mathf.SmoothDamp(currentSpeed, targetSpeed, ref speedSmoothVelocity, GetModifiedSmoothTime(speedSmoothTime));

        movementVelocityY += Time.deltaTime * gravity;
        Vector3 velocity = transform.forward * currentSpeed + Vector3.up * movementVelocityY;

        controller.Move(velocity * Time.deltaTime);
        currentSpeed = new Vector2(controller.velocity.x, controller.velocity.z).magnitude;

        if (controller.isGrounded)
        {
            movementVelocityY = 0;
        }

        float animationSpeedPercent = ((running) ? currentSpeed / runSpeed : currentSpeed / walkSpeed * .5f);
        anim.SetFloat("SpeedPercent", animationSpeedPercent, speedSmoothTime, Time.deltaTime);
        
    }

    public void Jump()
    {
        Debug.Log("Jump!!");
    }

    void MeleeAtack() 
    {
        if (Input.GetKeyDown(attackButton))
        {
            if (playerState != State.Melee1)
            {
                StartCoroutine(Attack1()); //Melee1
            }
            else if (playerState == State.Melee1)
            {
                //StartCoroutine(Attack2()); //Melee2
            }
            else if (playerState == State.Melee2)
            {
                //StartCoroutine(Attack()); //Melee3
            }
            
        }
    }

    IEnumerator Attack1()
    {
        playerState = State.Melee1;
        anim.SetBool("Attack", true);
        yield return new WaitForSeconds(attackDuration); 
        anim.SetBool("Attack", false);
        playerState = State.Cooldown;
        yield return new WaitForSeconds(attackCoolDown);
        playerState = State.None;
    }

    /*
    IEnumerator Attack2() // esto deberia cortar el melee1
    {
        playerState = State.Melee2;
        anim.SetBool("Attack2", true);
        anim.SetBool("Attack", false);
        yield return new WaitForSeconds(attackDuration);
        anim.SetBool("Attack2", false);
        playerState = State.Cooldown;
        yield return new WaitForSeconds(attackCoolDown);
        playerState = State.None;
    }*/


    float GetModifiedSmoothTime(float smoothTime)
    {
        if (controller.isGrounded)
        {
            return smoothTime;
        }

        return smoothTime;
    }
}
