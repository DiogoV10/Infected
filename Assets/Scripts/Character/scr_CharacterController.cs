using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using Unity.VisualScripting.Dependencies.NCalc;
using UnityEngine;
using static scr_Models;

public class scr_CharacterController : MonoBehaviour
{
    List<Rigidbody> ragdollRigids;

    private CharacterController characterController;
    private DefaultInput defaultInput;
    private Animator anim;

    private RaycastHit leftWallHit;
    private RaycastHit rightWallHit;

    private Vector2 input_Movement;
    private Vector2 input_View;
    private Vector2 input_AniMove;

    private Vector3 newCameraRotation;
    private Vector3 newCharacterRotation;
    private Vector3 newMovementSpeed;
    private Vector3 newMovementSpeedVelocity;
    private Vector3 jumpingForce;
    private Vector3 jumpingForceVelocity;
    private Vector2 currentVelocity;

    private bool has_Animator;
    private bool grounded;
    private bool wasGrounded; // Check if the character was grounded in the previous frame
    private bool crouched;
    private bool sprinting;
    private bool hardLand;
    private bool landed;
    private bool sliding;
    private bool wallLeft;
    private bool wallRight;
    private bool wallRunning;
    private bool exitingWall;

    private int xVelHash;
    private int yVelHash;
    private int zVelHash;
    private int jumpHash;
    private int groundHash;
    private int fallingHash;
    private int crouchHash;
    private int slideHash;
    private int wallRunHash;
    private int wallRunRightHash;

    private float viewClampYMin;
    private float fallTimer;
    private float slideTimer;
    private float slideTimerCooldown;
    private float exitWallTimer;


    [Header("References")]
    public Transform cameraRoot;
    public Transform camera;
    public Transform feetTransform;
    public Transform orientation;

    [Header("Settings")]
    public PlayerSettingsModel playerSettings;

    [Header("Other Settings")]
    [SerializeField] public float standViewClampYMin = -70;
    [SerializeField] public float crouchViewClampYMin = 5;
    [SerializeField] public float hardLandViewClampYMin = 70;
    [SerializeField] public float viewClampYMax = 80;
    [SerializeField] public float viewClampSmoothing = 10;
    [SerializeField] private float animBlendSpeed = 0.1f;
    public LayerMask playerMask;
    public LayerMask whatIsWall;
    public LayerMask whatIsGround;
    //[SerializeField] private float rotationThreshold = 45f;
    //[SerializeField] private float rotationSpeed = 5;

    [Header("Gravity")]
    public float gravity;
    public float gravityMin;
    private float playerGravity;

    [Header("Stance")]
    public PlayerStance playerStance;
    public float playerStanceSmoothing;
    public CharacterStance playerStandStance;
    public CharacterStance playerCrouchStance;
    public CharacterStance playerSlideStance;
    private float stanceCheckErrorMargin = 0.05f;
    private Vector3 stanceCapsuleCenterVelocity;
    private float stanceCapsuleHeightVelocity;

    private void Awake()
    {
        HideCursor();

        defaultInput = new DefaultInput();

        defaultInput.Character.Movement.performed += e => input_Movement = e.ReadValue<Vector2>();
        defaultInput.Character.View.performed += e => input_View = e.ReadValue<Vector2>();
        defaultInput.Character.Jump.performed += e =>
        {
            if (wallRunning)
            {
                WallJump();
            }
            else
            {
                Jump();
            }
        };
        defaultInput.Character.Crouch.performed += e => Crouch();
        defaultInput.Character.Sprint.performed += e => ToggleSprint();
        defaultInput.Character.SprintReleased.performed += e => StopSprint();
        defaultInput.Character.Slide.performed += e => Slide();
        defaultInput.AniMove.Movement.performed += e => input_AniMove = e.ReadValue<Vector2>();

        defaultInput.Enable();

        newCameraRotation = cameraRoot.localRotation.eulerAngles;
        newCharacterRotation = transform.localRotation.eulerAngles;

        characterController = GetComponent<CharacterController>();
    }

    private void HideCursor()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    // Start is called before the first frame update
    private void Start()
    {
        has_Animator = TryGetComponent<Animator>(out anim);

        xVelHash = Animator.StringToHash("X_Velocity");
        yVelHash = Animator.StringToHash("Y_Velocity");
        zVelHash = Animator.StringToHash("Z_Velocity");
        jumpHash = Animator.StringToHash("Jump");
        groundHash = Animator.StringToHash("Grounded");
        fallingHash = Animator.StringToHash("Falling");
        crouchHash = Animator.StringToHash("Crouch");
        slideHash = Animator.StringToHash("Slide");
        wallRunHash = Animator.StringToHash("WallRun");
        wallRunRightHash = Animator.StringToHash("WallRunRight");

        ragdollRigids = new List<Rigidbody>(transform.GetComponentsInChildren<Rigidbody>());
        ragdollRigids.Remove(GetComponent<Rigidbody>());

        DeactivateRagdoll();
    }

    // Update is called once per frame
    private void Update()
    {
        if (anim.enabled == false) return;

        if (Time.deltaTime > 0.1f)
        {
            return;
        }
        else
        {
            CalculateAnimation();
            CalculateJump();
            CalculateStance();
            UpdateSlideCooldown();
            CheckForWall();
            WallRun();
        }
    }

    private void FixedUpdate()
    {
        if (anim.enabled == false) return;

        if (Time.deltaTime > 0.1f)
        {
            return;
        }
        else if (wallRunning)
        {
            CalculateWallRun();
            TrackFalling();
        }
        else if (sliding)
        {
            CalculateSlide();
            TrackFalling();
        }
        else
        {
            CalculateMovement();
            TrackFalling();
        }
    }

    private void TrackFalling()
    {
        if (!has_Animator) return;

        if (characterController.isGrounded || wallRunning)
        {
            fallTimer = 0f;
        }
        else if (wasGrounded)
        {

            fallTimer = 0f;
        }
        else
        {
            fallTimer -= Time.fixedDeltaTime * 10;
        }

        wasGrounded = characterController.isGrounded;
    }

    private void LateUpdate()
    {
        if (anim.enabled == false) return;

        if (Time.deltaTime > 0.1f)
        {
            return;
        }
        else
        {
            CalculateView();
        }
    }

    private void CalculateView()
    {
        if (!has_Animator) return;

        camera.position = cameraRoot.position;

        float targetViewClampYMin;

        if (StanceCheck(playerStandStance.StanceCollider.height))
        {
            targetViewClampYMin = crouchViewClampYMin;
        }
        else if (hardLand && landed)
        {
            targetViewClampYMin = hardLandViewClampYMin;
        }
        else
        {
            targetViewClampYMin = standViewClampYMin;
        }

        viewClampYMin = Mathf.Lerp(viewClampYMin, targetViewClampYMin, Time.deltaTime * viewClampSmoothing);

        newCharacterRotation.y += playerSettings.ViewXSensitivity * (playerSettings.ViewXInverted ? -input_View.x : input_View.x) * Time.smoothDeltaTime;
        transform.localRotation = Quaternion.Euler(newCharacterRotation);

        //float cameraRotationY = camera.localRotation.eulerAngles.y;
        //float characterRotationY = transform.localRotation.eulerAngles.y;

        //if (Mathf.Abs(cameraRotationY) > rotationThreshold &&
        //    Mathf.Abs(cameraRotationY - 360f) > rotationThreshold)
        //{
        //    newCameraRotation.y = 0;

        //    Quaternion targetRotation = Quaternion.Euler(transform.localRotation.eulerAngles.x, characterRotationY + rotationThreshold, transform.localRotation.eulerAngles.z);
        //    transform.localRotation = Quaternion.Lerp(transform.localRotation, targetRotation, rotationSpeed * Time.deltaTime);
        //}
        //else
        //{
        //    newCameraRotation.y += playerSettings.ViewXSensitivity * (playerSettings.ViewXInverted ? -input_View.x : input_View.x) * Time.smoothDeltaTime;
        //}

        newCameraRotation.x += playerSettings.ViewYSensitivity * (playerSettings.ViewYInverted ? input_View.y : -input_View.y) * Time.smoothDeltaTime;
        newCameraRotation.x = Mathf.Clamp(newCameraRotation.x, viewClampYMin, viewClampYMax);

        camera.localRotation = Quaternion.Euler(newCameraRotation);
    }
    
    private void CalculateMovement()
    {
        if (!has_Animator) return;

        if (hardLand && landed) return;

        if (fallTimer <= -10)
        {
            hardLand = true;
        }

        if (input_Movement.y <= 0.2f)
        {
            sprinting = false;
        }

        var verticalSpeed = playerSettings.WalkingForwardSpeed;
        var horizontalSpeed = playerSettings.WalkingStrafeSpeed;

        if (sprinting)
        {
            verticalSpeed = playerSettings.RunningForwardSpeed;
            horizontalSpeed = playerSettings.RunningStrafeSpeed;
        }

        if (!grounded)
        {
            playerSettings.SpeedEffector = playerSettings.FallingSpeedEffector;
        }
        else if (crouched)
        {
            playerSettings.SpeedEffector = playerSettings.CrouchSpeedEffector;
        }
        else
        {
            playerSettings.SpeedEffector = 1;
        }

        verticalSpeed *= playerSettings.SpeedEffector;
        horizontalSpeed *= playerSettings.SpeedEffector;

        newMovementSpeed = Vector3.SmoothDamp(newMovementSpeed, new Vector3(horizontalSpeed * input_Movement.x * Time.fixedDeltaTime, 0, verticalSpeed * input_Movement.y * Time.fixedDeltaTime), ref newMovementSpeedVelocity, grounded ? playerSettings.MovementSmoothing : playerSettings.FallingSmoothing);
        var movementSpeed = transform.TransformDirection(newMovementSpeed);

        if (playerGravity > gravityMin)
        {
            playerGravity -= gravity * Time.fixedDeltaTime;
        }

        if (playerGravity < -0.1 && characterController.isGrounded)
        {
            playerGravity = -0.1f;
        }

        movementSpeed.y += playerGravity;
        movementSpeed += jumpingForce * Time.fixedDeltaTime;

        characterController.Move(movementSpeed);
    }

    private void CalculateStance()
    {
        var currentStance = playerStandStance;
        
        if (playerStance == PlayerStance.Crouch)
        {
            currentStance = playerCrouchStance;
        }
        else if (playerStance == PlayerStance.Slide)
        {
            currentStance = playerSlideStance;
        }

        characterController.height = Mathf.SmoothDamp(characterController.height, currentStance.StanceCollider.height, ref stanceCapsuleHeightVelocity, playerStanceSmoothing);
        characterController.center = Vector3.SmoothDamp(characterController.center, currentStance.StanceCollider.center, ref stanceCapsuleCenterVelocity, playerStanceSmoothing);
    }

    private void CalculateJump()
    {
        if (!has_Animator) return;

        jumpingForce = Vector3.SmoothDamp(jumpingForce, Vector3.zero, ref jumpingForceVelocity, playerSettings.JumpingFalloff);
    }

    private void Jump()
    {
        if (!has_Animator) return;

        if (!characterController.isGrounded) return;

        if (crouched)
        {
            if (StanceCheck(playerStandStance.StanceCollider.height))
            {
                return;
            }

            playerStance = PlayerStance.Stand;
            crouched = false;
            return;
        }

        anim.SetTrigger(jumpHash);
        jumpingForce = Vector3.up * playerSettings.JumpingHeight;
        playerGravity = 0;
        anim.ResetTrigger(jumpHash);
    }

    public void JumpAddForce()
    {
        jumpingForce = Vector3.up * playerSettings.JumpingHeight;
        anim.ResetTrigger(jumpHash);
    }

    public void Landed()
    {
        hardLand = false;
        landed = false;
    }

    public void Landing()
    {
        landed = true;
    }

    private void Crouch()
    {
        if (sliding) return;

        if (playerStance == PlayerStance.Crouch)
        {
            if (StanceCheck(playerStandStance.StanceCollider.height))
            {
                return;
            }

            crouched = false;
            playerStance = PlayerStance.Stand;
            return;
        }

        crouched = true;
        playerStance = PlayerStance.Crouch;
    }

    private bool StanceCheck(float stanceCheckHeight)
    {
        var start = new Vector3(feetTransform.position.x, feetTransform.position.y + characterController.radius + stanceCheckErrorMargin, feetTransform.position.z);
        var end = new Vector3(feetTransform.position.x, feetTransform.position.y - characterController.radius - stanceCheckErrorMargin + stanceCheckHeight, feetTransform.position.z);

        return Physics.CheckCapsule(start, end, characterController.radius, playerMask);
    }

    private void ToggleSprint()
    {
        if (input_Movement.y <= 0.2f)
        {
            sprinting = false;
            return;
        }

        sprinting = !sprinting;
    }
    
    private void StopSprint()
    {
        if (playerSettings.HoldSprint)
        {
            sprinting = false;
        }
    }

    private void Slide()
    {
        if (!sliding && slideTimerCooldown <= 0 && sprinting && !crouched && grounded)
        {
            playerStance = PlayerStance.Slide;
            StartSlide();
        }
    }

    private void StartSlide()
    {
        sliding = true;
        slideTimer = 0.0f;
    }

    private void UpdateSlideCooldown()
    {
        if (slideTimerCooldown > 0)
        {
            slideTimerCooldown -= Time.deltaTime;
        }
        else if (slideTimerCooldown <= 0 && defaultInput.Character.Slide.triggered)
        {
            slideTimerCooldown = playerSettings.SlidingCooldown;
        }
    }

    private void CalculateSlide()
    {
        var slideMovementSpeed = transform.TransformDirection(Vector3.forward) * playerSettings.SlidingSpeed * Time.fixedDeltaTime;
        characterController.Move(slideMovementSpeed);

        slideTimer += Time.fixedDeltaTime;
        if (slideTimer >= playerSettings.SlidingDuration)
        {
            StopSlide();
        }
    }

    private void StopSlide()
    {
        sliding = false;
        slideTimerCooldown = playerSettings.SlidingCooldown;

        if (StanceCheck(playerStandStance.StanceCollider.height))
        {
            Crouch();
        }
        else
        {
            playerStance = PlayerStance.Stand;
        }
    }

    private void CheckForWall()
    {
        wallRight = Physics.Raycast(transform.position, orientation.right, out rightWallHit, playerSettings.WallRunningCheckDistance, whatIsWall);
        wallLeft = Physics.Raycast(transform.position, -orientation.right, out leftWallHit, playerSettings.WallRunningCheckDistance, whatIsWall);
    }

    private bool AboveGround()
    {
        return !Physics.Raycast(transform.position, Vector3.down, playerSettings.WallRunningJumpHeightMin, whatIsGround);
    }

    private void WallRun()
    {
        if ((wallLeft || wallRight) && input_Movement.y > 0 && AboveGround() && !exitingWall)
        {
            if (!wallRunning)
                StartWallRun();
        }
        else if (exitingWall)
        {
            if (wallRunning)
                StopWallRun();

            if (playerSettings.WallRunningExitTime > 0)
                exitWallTimer -= Time.deltaTime;

            if (exitWallTimer <= 0)
                exitingWall = false;
        }
        else
        {
            if (wallRunning)
                StopWallRun();
        }
    }

    private void StartWallRun()
    {
        wallRunning = true;
    }

    private void CalculateWallRun()
    {
        Vector3 wallNormal = wallRight ? rightWallHit.normal : leftWallHit.normal;

        Vector3 wallForward = wallRight ? Vector3.Cross(wallNormal, transform.up) : Vector3.Cross(-wallNormal, transform.up);

        if ((orientation.forward - wallForward).magnitude > (orientation.forward - -wallForward).magnitude)
            wallForward = -wallForward;

        var wallRunningMovementSpeed = playerSettings.WallRunningSpeed * Time.fixedDeltaTime * wallForward;

        wallRunningMovementSpeed.y += 0.1f;

        characterController.Move(wallRunningMovementSpeed);
    }

    private void StopWallRun()
    {
        wallRunning = false;
    }

    private void WallJump()
    {
        exitingWall = true;
        exitWallTimer = playerSettings.WallRunningExitTime;

        Vector3 wallNormal = wallRight ? rightWallHit.normal : leftWallHit.normal;

        Vector3 forceToApply = Vector3.up * playerSettings.WallRunningJumpUpForce + wallNormal * playerSettings.WallRunningJumpSideForce;

        jumpingForce = forceToApply;
        playerGravity = 0;
    }

    private void CalculateAnimation()
    {
        if (!has_Animator) return;

        if (sprinting)
        {
            currentVelocity.x = playerSettings.RunningStrafeSpeed * input_AniMove.x;
            currentVelocity.y = playerSettings.RunningForwardSpeed * input_AniMove.y;

        }
        else
        {
            currentVelocity.x = playerSettings.WalkingStrafeSpeed * input_AniMove.x;
            currentVelocity.y = playerSettings.WalkingForwardSpeed * input_AniMove.y;
        }

        anim.SetFloat(xVelHash, currentVelocity.x, animBlendSpeed, Time.deltaTime);
        anim.SetFloat(yVelHash, currentVelocity.y, animBlendSpeed, Time.deltaTime);

        if (characterController.isGrounded) grounded = true;
        else
        {
            grounded = false;
            anim.SetFloat(zVelHash, fallTimer);
        }

        anim.SetBool(fallingHash, !grounded);
        anim.SetBool(groundHash, grounded);

        anim.SetBool(crouchHash, crouched);
        anim.SetBool(slideHash, sliding);

        anim.SetBool(wallRunHash, wallRunning);
        anim.SetBool(wallRunRightHash, wallRight);
    }

    void ActivateRagdoll()
    {
        anim.enabled = false;
        for (int i = 0; i < ragdollRigids.Count; i++)
        {
            ragdollRigids[i].useGravity = true;
            ragdollRigids[i].isKinematic = false;
        }
    }

    void DeactivateRagdoll()
    {
        anim.enabled = true;
        for (int i = 0; i < ragdollRigids.Count; i++)
        {
            ragdollRigids[i].useGravity = false;
            ragdollRigids[i].isKinematic = true;
        }
    }

    public void GetKilled()
    {
        ActivateRagdoll();
    }
}
