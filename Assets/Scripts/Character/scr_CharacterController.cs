using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static scr_Models;

public class scr_CharacterController : MonoBehaviour
{
    List<Rigidbody> ragdollRigids;

    private CharacterController characterController;
    private DefaultInput defaultInput;
    private Animator anim;

    public Vector2 input_Movement;
    public Vector2 input_View;
    public Vector2 input_AniMove;

    private Vector3 newCameraRotation;
    private Vector3 newCharacterRotation;
    private Vector2 currentVelocity;

    private bool has_Animator;
    private bool grounded;

    private int xVelHash;
    private int yVelHash;
    private int zVelHash;
    private int jumpHash;
    private int groundHash;
    private int fallingHash;

    [Header("References")]
    public Transform cameraRoot;
    public new Transform camera;

    [Header("Settings")]
    public PlayerSettingsModel playerSettings;
    [SerializeField] public float viewClampYMin = -70;
    [SerializeField] public float viewClampYMax = 80;
    [SerializeField] private float animBlendSpeed = 0.1f;

    [Header("Gravity")]
    public float gravity;
    public float gravityMin;
    private float playerGravity;

    public Vector3 jumpingForce;
    private Vector3 jumpingForceVelocity;

    private void Awake()
    {
        defaultInput = new DefaultInput();

        defaultInput.Character.Movement.performed += e => input_Movement = e.ReadValue<Vector2>();
        defaultInput.Character.View.performed += e => input_View = e.ReadValue<Vector2>();
        defaultInput.Character.Jump.performed += e => Jump();
        defaultInput.AniMove.Movement.performed += e => input_AniMove = e.ReadValue<Vector2>();

        defaultInput.Enable();

        newCameraRotation = cameraRoot.localRotation.eulerAngles;
        newCharacterRotation = transform.localRotation.eulerAngles;

        characterController = GetComponent<CharacterController>();
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
            CalculateView();
            CalculateMovement();
            CalculateAnimation();
            CalculateJump();
        }
    }

    private void CalculateView()
    {
        if (!has_Animator) return;

        camera.position = cameraRoot.position;

        newCharacterRotation.y += playerSettings.ViewXSensitivity * (playerSettings.ViewXInverted ? -input_View.x : input_View.x) * Time.deltaTime;
        transform.localRotation = Quaternion.Euler(newCharacterRotation);

        newCameraRotation.x += playerSettings.ViewYSensitivity * (playerSettings.ViewYInverted ? input_View.y : -input_View.y) * Time.deltaTime;
        newCameraRotation.x = Mathf.Clamp(newCameraRotation.x, viewClampYMin, viewClampYMax);

        camera.localRotation = Quaternion.Euler(newCameraRotation);
    }
    
    private void CalculateMovement()
    {
        if (!has_Animator) return;

        var verticalSpeed = playerSettings.WalkingForwardSpeed * input_Movement.y * Time.deltaTime;
        var horizontalSpeed = playerSettings.WalkingStrafeSpeed * input_Movement.x * Time.deltaTime;

        var newMovementSpeed = new Vector3(horizontalSpeed, 0, verticalSpeed);
        newMovementSpeed = transform.TransformDirection(newMovementSpeed);

        if (playerGravity > gravityMin)
        {
            playerGravity -= gravity * Time.deltaTime;
        }

        if (playerGravity < -0.1 && characterController.isGrounded)
        {
            playerGravity = -0.1f;
        }

        newMovementSpeed.y += playerGravity;
        newMovementSpeed += jumpingForce * Time.deltaTime;

        characterController.Move(newMovementSpeed);
    }

    private void CalculateJump()
    {
        if (!has_Animator) return;

        jumpingForce = Vector3.SmoothDamp(jumpingForce, Vector3.zero, ref jumpingForceVelocity, playerSettings.JumpingFalloff);
    }

    private void Jump()
    {
        if (!has_Animator) return;

        if (!characterController.isGrounded)
        {
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

    private void CalculateAnimation()
    {
        if (!has_Animator) return;

        currentVelocity.x = playerSettings.WalkingStrafeSpeed * input_AniMove.x;
        currentVelocity.y = playerSettings.WalkingForwardSpeed * input_AniMove.y;

        anim.SetFloat(xVelHash, currentVelocity.x, animBlendSpeed, Time.deltaTime);
        anim.SetFloat(yVelHash, currentVelocity.y, animBlendSpeed, Time.deltaTime);

        if (characterController.isGrounded) grounded = true;
        else
        {
            grounded = false;
            anim.SetFloat(zVelHash, characterController.velocity.y);
        }

        anim.SetBool(fallingHash, !grounded);
        anim.SetBool(groundHash, grounded);
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