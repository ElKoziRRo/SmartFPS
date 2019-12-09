using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{

    [Header("Speeds")]
    [Space]
    [Range(0, 10)] public float walkSpeed = 2;
    [Range(0, 20)] public float runSpeed = 4;
    [Range(0, 10)] public int crouchSpeed = 2;
    [Range(0, 10)] public float jumpSpeed = 6.0f;
    [Space(5)]

    [Header("Player Attributes")]
    [Space]
    public bool canRun = true;
    public bool canJump = true;
    public bool canCrouch = true;
    public bool canSprintSlide = true;
    public bool inputCooldown = false;
    [Space]
    public float normalHeight = 2.0f;
    public float normalCamHeight;
    public Vector3 normalCenter = new Vector3(0, 0, 0);
    public float crouchHeight = 1.4f;
    public float crouchCamHeight;
    public Vector3 CrouchCenter = new Vector3(0, -0.3f, 0);
    [Space]
    public float slideLimit = 45.0f;
    public float slideSpeed = 8.0f;
    public float sprintSlideTime = 2.0f;
    public float sprintSlideCooldowm = 5.0f;
    public MouseLook mouseLook;
    //Physics Tuner
    private float bounceTuner = 1.75f;
    private float gravity = 24f;
    [Space]

    [Header("Fall Damage")]
    [Space]
    public float fallDistance;
    public float fallingDamageThreshold = 8f;
    public float fallDamageMultiplier = 5.0f;
    private Vector3 currentPosition;
    private Vector3 lastPosition;
    private float highestPoint;
    [Space]

    [Header("Stats")]
    [Space]
    public float currentSpeed;
    public bool isWalking = false;
    public bool isRunning = false;
    public bool isCrouching = false;
    public bool isGrounded = false;
    public bool isFalling = false;
    public bool sliding = false;
    public bool wasSliding = false;
    public bool sprintSliding = false;

    private Vector3 moveDirection = Vector3.zero;
    float inputModifyFactor;
    [Space]
    public float velMagnitude;
    public float horizontalAxisInput;
    public float verticalAxisInput;
    [Space]

    public GameObject mainCam;
    public CameraRigController crController;
    public Transform cameraRig;

    //Private Variables
    private SoundStake soundProfile;

    private float rayDistance;
    private Vector3 contactPoint;
    private int jumpTimer;

    private float distanceToObstacle;
    private Transform playerTransform;
    private CharacterController controller;

    // Use this for initialization
    void Start()
    {
        soundProfile = GameObject.Find("GameManager").GetComponent<SoundManager>().soundProfile;
        controller = GetComponent<CharacterController>();
        playerTransform = transform;
        rayDistance = controller.height / 2 + 1.1f;
        slideLimit = controller.slopeLimit;
        mouseLook.Init(playerTransform, mainCam.transform);
    }

    // Update is called once per frame
    void Update()
    {
        InputFunctionality();
        ControllerHeight();
        Movement();
        mouseLook.LookRotation(playerTransform, mainCam.transform);
        CameraRigAssistant();
    }

    void LateUpdate()
    {
        lastPosition = currentPosition;
    }

    void CameraRigAssistant()
    {

        if (horizontalAxisInput != 0 || verticalAxisInput != 0)
        {
            if (isRunning) crController._PlayerState = CameraRigController.PlayerState.Running;
            else crController._PlayerState = CameraRigController.PlayerState.Walking;
        }
        else crController._PlayerState = CameraRigController.PlayerState.Idle;

    }

    void InputFunctionality()
    {
        if (!inputCooldown)
        {
            horizontalAxisInput = Input.GetAxis("Horizontal");
            verticalAxisInput = Input.GetAxis("Vertical");
            inputModifyFactor = (horizontalAxisInput != 0.0f && verticalAxisInput != 0.0f) ? 0.7f : 1.0f;
            if (canRun && !isCrouching)
            {
                if (Input.GetKey(KeyCode.LeftShift) && Input.GetKey(KeyCode.W) && !Input.GetButton("Fire2"))
                {
                    if (GetComponent<SurvivalController>() != null && !GetComponent<SurvivalController>().runCooldown)
                    {
                        isRunning = true;
                        isWalking = false;
                    }
                }
                else
                    isRunning = false;
            }

            if (isRunning && canSprintSlide && !isCrouching)
            {
                if (Input.GetKeyDown(KeyCode.C))
                {
                    StartCoroutine(SprintSlide());
                }
            }

            isWalking = false;

            if (!isRunning && horizontalAxisInput != 0 || verticalAxisInput != 0)
                isWalking = true;

            if (Input.GetKeyDown(KeyCode.C))
            {
                CheckDistance();

                if (!isCrouching)
                {
                    isCrouching = true;
                }
                else
                {
                    if (distanceToObstacle >= crouchHeight)
                        isCrouching = false;
                }
            }
        }
    }

    void ControllerHeight()
    {
        if (!isCrouching)
        {
            controller.height = normalHeight;
            controller.center = normalCenter;
            if (mainCam.transform.localPosition.y > normalCamHeight)
                mainCam.transform.localPosition = new Vector3(mainCam.transform.localPosition.x, normalCamHeight, mainCam.transform.localPosition.z);
            else if (mainCam.transform.localPosition.y < normalCamHeight)
                mainCam.transform.localPosition = new Vector3(mainCam.transform.localPosition.x, mainCam.transform.localPosition.y + Time.deltaTime * crouchSpeed, mainCam.transform.localPosition.z);
        }
        else
        {
            controller.height = crouchHeight;
            controller.center = CrouchCenter;
            if (mainCam.transform.localPosition.y != crouchCamHeight)
            {
                if (mainCam.transform.localPosition.y > crouchCamHeight)
                    mainCam.transform.localPosition = new Vector3(mainCam.transform.localPosition.x, mainCam.transform.localPosition.y - Time.deltaTime * crouchSpeed, mainCam.transform.localPosition.z);
                if (mainCam.transform.localPosition.y < crouchCamHeight)
                    mainCam.transform.localPosition = new Vector3(mainCam.transform.localPosition.x, mainCam.transform.localPosition.y + Time.deltaTime * crouchSpeed, mainCam.transform.localPosition.z);
            }
        }
    }

    void Movement()
    {
        velMagnitude = controller.velocity.magnitude;
        RaycastHit hit;
        if (isGrounded)
        {
            //Play Footstep Sounds
            if (GetComponent<FootstepController>().triggerMode == FootstepController._trigger.Smart && !sliding)
            {
                GetComponent<FootstepController>().PlayFootstepSound();
            }

            //Detecting Slopes
            if (Physics.Raycast(playerTransform.position, -Vector3.up, out hit, rayDistance))
            {
                float hitangle = Vector3.Angle(hit.normal, Vector3.up);
                if (!sprintSliding)
                {
                    if (hitangle > slideLimit - 0.2f)
                    {
                        sliding = true;
                    }
                    else
                    {
                        sliding = false;
                    }
                }
            }

            if (isFalling)
            {
                isFalling = false;
                fallDistance = highestPoint - currentPosition.y;
                if (fallDistance > fallingDamageThreshold)
                {
                    ApplyFallingDamage(fallDistance);
                }

                if (fallDistance < fallingDamageThreshold && fallDistance > 0.1f)
                {
                    if (!isCrouching)
                        if (GetComponent<FootstepController>().triggerMode == FootstepController._trigger.Smart)
                            GetComponent<FootstepController>().JumpLandSound();

                    StartCoroutine(FallCamera(new Vector3(7, Random.Range(-1.0f, 1.0f), 0), 0.15f));
                }
            }

            if (sliding)
            {
                //Preventing infinite sprint slide
                if (sprintSliding)
                {
                    isCrouching = true;
                    moveDirection = transform.forward;
                    moveDirection *= slideSpeed / 1.1f;

                    float slideTimer = Time.deltaTime;
                    if (slideTimer > sprintSlideTime)
                        sliding = false;
                }
                else
                {
                    // StartCoroutine(InputCooldownFunctionality(utSec, utSec * 2));
                    isCrouching = true;
                    wasSliding = true;
                    Vector3 hitNormal = hit.normal;
                    moveDirection = new Vector3(hitNormal.x, -hitNormal.y, hitNormal.z);
                    Vector3.OrthoNormalize(ref hitNormal, ref moveDirection);
                    moveDirection *= slideSpeed;
                }
            }
            else
            {
                //Crouch if sprint slider remain state to crouch.else change to stand state
                if (sprintSliding) { isCrouching = true; sprintSliding = false; }
                else if (wasSliding) { isCrouching = false; wasSliding = false; }

                //Fixing current Speed
                if (!isCrouching)
                {
                    if (isRunning)
                        currentSpeed = runSpeed;
                    else
                        currentSpeed = walkSpeed;
                }
                else
                {
                    currentSpeed = crouchSpeed;
                }

                if (!inputCooldown)
                {
                    moveDirection = new Vector3(horizontalAxisInput * inputModifyFactor, -bounceTuner, verticalAxisInput * inputModifyFactor);
                    moveDirection = playerTransform.TransformDirection(moveDirection);
                    moveDirection *= currentSpeed;
                }

                if (!Input.GetButton("Jump"))
                {
                    jumpTimer++;
                }
                else if (jumpTimer >= bounceTuner)
                {
                    jumpTimer = 0;

                    if (!isCrouching)
                    {
                        GetComponent<FootstepController>().footstepAudioSource.clip = soundProfile.jumpStartSound;
                        GetComponent<FootstepController>().footstepAudioSource.volume = GetComponent<FootstepController>().jumpVolume;
                        GetComponent<FootstepController>().footstepAudioSource.Play();
                        moveDirection.y = jumpSpeed;
                    }
                    else
                    {
                        CheckDistance();
                        if (distanceToObstacle > 1.6f)
                            isCrouching = false;
                    }
                }
            }
        }
        else
        {
            currentPosition = playerTransform.position;

            if (currentPosition.y > lastPosition.y)
            {
                highestPoint = playerTransform.position.y;
                isFalling = true;
            }

            if (!isFalling)
            {
                highestPoint = playerTransform.position.y;
                isFalling = true;
            }
        }
        moveDirection.y -= gravity * Time.deltaTime;
        isGrounded = (controller.Move(moveDirection * Time.fixedDeltaTime) & CollisionFlags.Below) != 0;
    }

    void CheckDistance()
    {
        Vector3 pos = playerTransform.position + controller.center - new Vector3(0, controller.height / 2, 0);
        RaycastHit hit;
        if (Physics.SphereCast(pos, controller.radius, playerTransform.up, out hit, 10))
        {
            distanceToObstacle = (hit.distance) - controller.radius;
            Debug.DrawLine(pos, hit.point, Color.red, 2.0f);
        }
        else
        {
            distanceToObstacle = 6;
        }
    }

    void ApplyFallingDamage(float fallDistance)
    {
        GetComponent<HealthController>().TakeDamage(fallDistance * fallDamageMultiplier, transform.position);
        //Fall Camera Animation should be here
    }

    IEnumerator FallCamera(Vector3 d, float ta)
    {
        Quaternion s = cameraRig.localRotation;
        Quaternion e = cameraRig.localRotation * Quaternion.Euler(d);

        float r = 1.0f / ta;
        float t = 0.0f;
        while (t < 1.0f)
        {
            t += Time.deltaTime * r;
            cameraRig.localRotation = Quaternion.Slerp(s, e, t);
            yield return null;
        }
    }

    IEnumerator SprintSlide()
    {
        isRunning = false;
        isWalking = false;
        //Play Sliding Sound
        GetComponent<FootstepController>().PlaySlideSound();
        //Apply sliding
        sprintSliding = true;
        sliding = true;
        canSprintSlide = false;
        yield return new WaitForSecondsRealtime(sprintSlideTime);

        sliding = false;
        yield return new WaitForSecondsRealtime(sprintSlideCooldowm);

        canSprintSlide = true;
    }
}
