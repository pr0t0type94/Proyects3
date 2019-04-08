﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    //tumadre
    private CharacterController playerCC;

    [Header("KeyCodes",order =0)]

    public KeyCode UpKeyCode;
    public KeyCode DownKeyCode;
    public KeyCode LeftKeyCode;
    public KeyCode RightKeyCode;
    public KeyCode RunKeyCode;
    public KeyCode PassKeyCode;
    public KeyCode JumpKeyCode;
    public KeyCode DashKeyCode;



    [Header("Speed")]

    public float l_Speed;
    public float m_RunSpeed;
    private float iniSpeed;
    public float JumpSpeed;
    private float verticalSpeed;
    public bool OnGround;
    private Vector3 l_Movement;

    private GameObject m_CurrentPlatform;


    //For DASH
    [Header("Dash")]

    public Vector3 moveDirection;
    public float maxDashTime;
    private float dashSpeed;
    private float currentDashTime;
    public float  dashReloadmaxTime;
    private float currentDashReloadTime;
    public Text dash1;
    public Text dash2;
    public Text dash3;
    private int totalDashes = 3;
    private bool DashingBool = false;
    private bool DashReloadBool = false;
    private bool dashCooldownBool;
    public float dashCooldownTime;
    private float currentDashCooldown;
    //

    //public GameController gameControler;

    [Header("Bools")]

    public bool canMove;
    public bool canAttack;

    CollisionFlags l_CollisionFlags;

    [Header("Attaching")]
    public GameObject m_AttachingPosition;
    public bool m_AttachedObject;
    private Rigidbody m_ObjectAttached;
    public float m_AttachingObjectSpeed;
    private Quaternion m_AttachingObjectStartRotation;
    private bool m_AttachingObject;
    public float impulseForce = 50f;

    //public RestartGame resetController;

    public Vector3 respawnPosition;

    Renderer rend;
    [Header("Camera")]


    public Camera cam;
    public Camera_Controller noBallCC;
    public Camera_Controller_3rdPerson hasBallCC;


    [Header("Ball")]

    private GameObject ball;
    private bool hasBall;
    private Rigidbody ballRb;
    float speedX = 600f;
    float speedY = 400f;
    private bool hasLostBall;
    public float ballLeftTimer;

    [Header("Push")]


    bool startPushEnemyTimer;
    public float pushHitTimer = 2f;
    public float pushSpeed=5f;
    public CharacterController enemyCC;




    // Use this for initialization
    void Start()
    {
        playerCC = gameObject.GetComponent<CharacterController>();
        iniSpeed = l_Speed;
        canMove = true;
        respawnPosition = transform.position;
        
        dashSpeed = l_Speed * 3;
        
        //hasTheBall = false;
        totalDashes = 3;
        

        rend = GetComponent<Renderer>();
        rend.material.SetColor("_Color", Color.blue);

        ball = GameObject.FindGameObjectWithTag("ball");
        hasBall = false;

        noBallCC.enabled = true;
        hasBallCC.enabled = false;

        ballRb = ball.GetComponent<Rigidbody>();
        startPushEnemyTimer = false;

    }

    // Update is called once per frame
    void Update()
    {
       

        /////////////
        ///

        if (hasBall)
        {
            hasBallCC.enabled = true;
            noBallCC.enabled = false;
            if (Input.GetKeyDown(PassKeyCode))
            {
                Vector3 direction = Camera.main.transform.forward;
                ball.transform.parent = null;
                ballRb.isKinematic = false;
                ballRb.AddForce(new Vector3(speedX *transform.forward.normalized.x, speedY, 0.0f));
                hasBall = false;
            }

        }
        else
        {
            hasBallCC.enabled = false;
            noBallCC.enabled = true;
        }

        //transform.LookAt(transform.forward); 
        if (gameObject.name == "Player")
        {
            float hor = Input.GetAxis("Horizontal");
            float ver = Input.GetAxis("Vertical");
            l_Movement = new Vector3(ver, 0, -hor);
            l_Movement = Vector3.ClampMagnitude(l_Movement, 1) * l_Speed * Time.deltaTime;

        }

        if (gameObject.name == "Player2")
        {
            float hor = Input.GetAxis("Horizontal2");
            float ver = Input.GetAxis("Vertical2");
            l_Movement = new Vector3(-ver, 0, hor);
            l_Movement = Vector3.ClampMagnitude(l_Movement, 1) * l_Speed * Time.deltaTime;

        }
        playerCC.Move(l_Movement);


        if (Input.GetKey(JumpKeyCode) && OnGround)
        {
            verticalSpeed = JumpSpeed;
        }

        //RUN
        if (OnGround && Input.GetKey(RunKeyCode))
        {
            l_Speed = m_RunSpeed;

        }
        else
        {
            l_Speed = iniSpeed;
        }

        //DASH
        performDash();
        updateDashText();
        

     
        /////LONGJUMP
        //if (startLongJumpTimer)
        //{
        //    longJumpTimer -= Time.deltaTime;

        //    if (longJumpTimer <= 0)
        //    {
        //        startLongJumpTimer = false;
        //        longJumpTimer = 1f;
        //        canMove = true;
        //    }
        //    else
        //    {
        //        canMove = false;
        //        verticalSpeed = JumpSpeed / 6;
        //        playerCC.Move(transform.forward * longJumpImpulseForce * Time.deltaTime);
        //        playerCC.Move(transform.up * verticalSpeed * Time.deltaTime);

        //    }


        //}


        //MOVEMENT

        l_Movement.Normalize();

        l_Movement *= Time.deltaTime * l_Speed;

        //hasMovement
        if (l_Movement != Vector3.zero)
        {

            Quaternion newRotation = Quaternion.LookRotation(new Vector3(l_Movement.x, 0, l_Movement.z));
            transform.rotation = Quaternion.Slerp(transform.rotation, newRotation, 4 * Time.deltaTime);
        }
        
        //GRAVITY
        if (OnGround && verticalSpeed <= 0)
        {
            verticalSpeed = -playerCC.stepOffset / Time.deltaTime;
        }
        else
        {
            verticalSpeed += Physics.gravity.y * Time.deltaTime;
        }


        l_Movement.y = verticalSpeed * Time.deltaTime;

        //CollisionFlags + controller Move
        /*if (canMove)
        {
            l_CollisionFlags = playerCC.Move(l_Movement);

        }*/

        if ((l_CollisionFlags & CollisionFlags.Below) != 0)
        {
            OnGround = true;
            verticalSpeed = 0.0f;
        }
        else
        {
            OnGround = false;

        }

        if ((l_CollisionFlags & CollisionFlags.Above) != 0 && verticalSpeed > 0.0f)
            verticalSpeed = 0.0f;


        if (startPushEnemyTimer)
        {
            pushHitTimer -= Time.deltaTime;
            if (pushHitTimer > 0)
            {

                pushEnemy();
            }
            else
            {
                startPushEnemyTimer = false;
                pushHitTimer = 1f;
            }
        }
        else
        {

        }

    
    }

    //private void OnTriggerEnter(Collider other)
    //{
    //    PlayerController pc = other.gameObject.GetComponent<PlayerController>();
    //    if (other.gameObject.tag == "Player")
    //    {
    //        startPushEnemyTimer = true;
    //        if (hasBall)
    //        {
    //            ball.transform.parent = null;
 
    //                ball.GetComponent<Rigidbody>().isKinematic = false;
    //                ball.GetComponent<Rigidbody>().AddForce(Vector3.up * 200f);
    //            hasBall = false;
    //        }
    //    }
    //}
    private bool canGrabBall()
    { 
            ballLeftTimer -= Time.deltaTime;
            if (ballLeftTimer >= 0)
            {
                return false;
            }
            else
            {
                ballLeftTimer = .2f;
                return true;
            }
        
    }

    public void OnControllerColliderHit(ControllerColliderHit hit)
    {
        //PlayerController pc = hit.gameObject.GetComponent<PlayerController>();
        //if (hit.gameObject.tag=="Player")
        //{
        //    startPushEnemyTimer = true;
        //    if(hasBall)
        //    {
        //        ball.transform.parent = null;
        //        hasBall = false;
        //        if (!hasBall)
        //        {
        //            ball.GetComponent<Rigidbody>().isKinematic = false;
        //            ball.GetComponent<Rigidbody>().AddForce(Vector3.up * 40f);

        //        }
        //    }
        //}

        //if(hit.gameObject.tag == "Player" && pc.hasBall)
        //{
        //    ball.transform.parent = null;
        //    hasBall = false;
        //    if (!hasBall)
        //    {
        //    ball.GetComponent<Rigidbody>().isKinematic = false;
        //    ball.GetComponent<Rigidbody>().AddForce(Vector3.up * 40f);

        //    }

        //}
        PlayerController pc = hit.gameObject.GetComponent<PlayerController>();
        if (hit.gameObject.tag == "Player")
        {
            startPushEnemyTimer = true;
            if (hasBall)
            {
                ball.transform.parent = null;
                ball.GetComponent<Rigidbody>().isKinematic = false;
                ball.GetComponent<Rigidbody>().AddRelativeForce(Vector3.up * 200f);
                hasBall = false;
            }
        }

        if (hit.gameObject.tag == "ball" && canGrabBall())
        {
            hasBall = true;
            ball.GetComponent<Rigidbody>().isKinematic = true;
            ball.transform.position = m_AttachingPosition.transform.position;
            ball.transform.parent = gameObject.transform;


        }
    }

    

    void performDash()
    {


        if (totalDashes != 0)
        {
            

            if (Input.GetKeyDown(DashKeyCode) && dashCooldownBool == false)
            {
                currentDashTime = 0.0f;
                currentDashCooldown = 0.0f;

                totalDashes -= 1;
                DashingBool = true;

                dashCooldownBool = true;

            }
        }

            if (dashCooldownBool)
            {

                currentDashCooldown += Time.deltaTime;
                if (currentDashCooldown >= dashCooldownTime)
                {
                    dashCooldownBool = false;
                }
            }


            if (DashingBool)
            {

                currentDashTime += Time.deltaTime;

                rend.material.SetColor("_Color", Color.green);
                l_Speed = dashSpeed;

                if (currentDashTime >= maxDashTime)
                {

                    
                    DashingBool = false;


                }

            }
      

        if (DashingBool == false)
        {
            rend.material.SetColor("_Color", Color.blue);
            l_Speed = iniSpeed;
        }



        //DASH RELOAD

        if (totalDashes != 3)
        {
            
            currentDashReloadTime += Time.deltaTime;


            if (currentDashReloadTime >= dashReloadmaxTime)
            {
                totalDashes += 1;

                currentDashReloadTime = 0.0f;

            }
        }
        

    }



    private void updateDashText()
    {
        if (totalDashes == 3)
        {
            dash3.enabled = true;
            dash2.enabled = true;
            dash1.enabled = true;
        }

        else if (totalDashes == 2)
        {
            dash3.enabled = false;
            dash2.enabled = true;
            dash1.enabled = true;
        }
        else if (totalDashes == 1)
        {
            dash3.enabled = false;
            dash2.enabled = false;
            dash1.enabled = true;
        }
        else if (totalDashes == 0)
        {
            dash3.enabled = false;
            dash2.enabled = false;
            dash1.enabled = false;
        }

    }


    public void pushEnemy()
    {

        enemyCC.Move(transform.forward * pushSpeed * Time.deltaTime);


    }

  





} 


