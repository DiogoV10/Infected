using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static scr_Models;

public class WeaponController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] public Animator weaponAnim;

    [Header("Weapon")]
    [SerializeField] public GameObject SledgeHammer;
    private DefaultInput defaultInput;
    private Sound sound;
    [SerializeField] public bool CanAttack = true;
    [SerializeField] public bool IsAttacking = false;
    [SerializeField] public float AttackCooldown = 1.0f;
    [SerializeField] public AudioClip HammerAttackSound;

    private scr_CharacterController characterController;

    [Header("Settings")]
    [SerializeField] public WeaponSettingsModel settings;

    private bool isInitialized;

    Vector3 newWeaponRotation;
    Vector3 newWeaponRotationVelocity;
    
    Vector3 targetWeaponRotation;
    Vector3 targetWeaponRotationVelocity;

    Vector3 newWeaponMovementRotation;
    Vector3 newWeaponMovementRotationVelocity;

    Vector3 targetWeaponMovementRotation;
    Vector3 targetWeaponMovementRotationVelocity;

    private bool isGroundedTrigger;

    private float fallingDelay;

    [Header("Weapon Sway")]
    [SerializeField] public Transform weaponSwayObject;
    [SerializeField] public float swayAmountA = 1;
    [SerializeField] public float swayAmountB = 2;
    [SerializeField] public float swayScale = 600;
    [SerializeField] public float swayLerpSpeed = 14;

    private float swayTime;
    private Vector3 swayPosition;

    public void Initialize(DefaultInput input, scr_CharacterController CharacterController)
    {
        defaultInput = input;

        defaultInput.Character.Attack.performed += e => Attack();

        characterController = CharacterController;

        isInitialized = true;
    }

    private void Awake()
    {
        sound = GetComponentInParent<Sound>();
    }

    private void Start()
    {
        newWeaponRotation = transform.localRotation.eulerAngles;
    }

    private void Update()
    {
        if (!isInitialized) return;

        CalculateWeaponRotation();
        SetWeaponAnimations();
        CalculateWeaponSway();
    }

    public void TriggerJump()
    {
        isGroundedTrigger = false;
        weaponAnim.SetTrigger("Jump");
    }

    private void CalculateWeaponRotation()
    {
        targetWeaponRotation.y += settings.SwayAmount * (settings.SwayXInverted ? -characterController.input_View.x : characterController.input_View.x) * Time.deltaTime;
        targetWeaponRotation.x += settings.SwayAmount * (settings.SwayYInverted ? -characterController.input_View.y : characterController.input_View.y) * Time.deltaTime;

        targetWeaponRotation.x = Mathf.Clamp(targetWeaponRotation.x, -settings.SwayClampX, settings.SwayClampX);
        targetWeaponRotation.y = Mathf.Clamp(targetWeaponRotation.y, -settings.SwayClampY, settings.SwayClampY);
        targetWeaponRotation.z = targetWeaponRotation.y;

        targetWeaponRotation = Vector3.SmoothDamp(targetWeaponRotation, Vector3.zero, ref targetWeaponRotationVelocity, settings.SwayResetSmoothing);
        newWeaponRotation = Vector3.SmoothDamp(newWeaponRotation, targetWeaponRotation, ref newWeaponRotationVelocity, settings.SwaySmoothing);

        targetWeaponMovementRotation.z = settings.MovementSwayX * (settings.MovementSwayXInverted ? -characterController.input_Movement.x : characterController.input_Movement.x);
        targetWeaponMovementRotation.x = settings.MovementSwayY * (settings.MovementSwayYInverted ? -characterController.input_Movement.y : characterController.input_Movement.y);

        targetWeaponMovementRotation = Vector3.SmoothDamp(targetWeaponMovementRotation, Vector3.zero, ref targetWeaponMovementRotationVelocity, settings.MovementSwaySmoothing);
        newWeaponMovementRotation = Vector3.SmoothDamp(newWeaponMovementRotation, targetWeaponMovementRotation, ref newWeaponMovementRotationVelocity, settings.MovementSwaySmoothing);

        transform.localRotation = Quaternion.Euler(newWeaponRotation + newWeaponMovementRotation);
    }

    private void SetWeaponAnimations()
    {
        if (isGroundedTrigger)
        {
            fallingDelay = 0;
        }
        else
        {
            fallingDelay += Time.deltaTime;
        }

        if (characterController.grounded && !isGroundedTrigger && fallingDelay > 0.1f)
        {
            weaponAnim.SetTrigger("Land");
            isGroundedTrigger = true;
        }
        else if (!characterController.grounded && isGroundedTrigger)
        {
            weaponAnim.SetTrigger("Falling");
            isGroundedTrigger = false;
        }

        weaponAnim.SetBool("IsSprinting", characterController.sprinting);
        weaponAnim.SetFloat("WeaponAnimationSpeed", characterController.weaponAnimationSpeed);
    }

    private void CalculateWeaponSway()
    {
        var targetPosition = LissajousCurve(swayTime, swayAmountA, swayAmountB) / swayScale;

        swayPosition = Vector3.Lerp(swayPosition, targetPosition, Time.smoothDeltaTime * swayLerpSpeed);
        swayTime += Time.deltaTime;

        if (swayTime > 6.3f)
        {
            swayTime = 0;
        }

        weaponSwayObject.localPosition = swayPosition;
    }

    private Vector3 LissajousCurve(float Time, float A, float B)
    {
        return new Vector3(Mathf.Sin(Time), A * Mathf.Sin(B * Time + Mathf.PI));
    }

    private void Attack()
    {
        if (CanAttack)
        {
            HammerAttack();
            sound.shootSound = HammerAttackSound;
            sound.Fire();
        }
    }

    private void HammerAttack()
    {
        IsAttacking = true;
        CanAttack = false;

        Animator anim=SledgeHammer.GetComponent<Animator>();
        anim.SetTrigger("Attack");

        StartCoroutine(ResetAttackCooldown());
    }

    IEnumerator ResetAttackCooldown()
    {
        StartCoroutine(ResetAttackBool());
        yield return new WaitForSeconds(AttackCooldown);
        CanAttack = true;
    }

    IEnumerator ResetAttackBool()
    {
        yield return new WaitForSeconds(1.0f);
        IsAttacking = false;
    }
}
