using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Models;

public class CharacterController : MonoBehaviour
{
    Animator anim;

    List<Rigidbody> ragdollRigids;

    private DefaultInput defaultInput;
    private Animator animator;

    public Vector2 input_Movement;
    public Vector2 input_View;

    private Vector3 newCameraRotation;
    private Vector3 newCharacterRotation;

    private bool has_Animator;

    [Header("References")]
    public Transform cameraRoot;
    public Transform camera;

    [Header("Settings")]
    public PlayerSettingsModel playerSettings;
    [SerializeField] public float viewClampYMin = -70;
    [SerializeField] public float viewClampYMax = 80;

    private void Awake()
    {
        defaultInput = new DefaultInput();

        defaultInput.Character.Movement.performed += e => input_Movement = e.ReadValue<Vector2>();
        defaultInput.Character.View.performed += e => input_View = e.ReadValue<Vector2>();

        defaultInput.Enable();

        newCameraRotation = cameraRoot.localRotation.eulerAngles;
        newCharacterRotation = transform.localRotation.eulerAngles;
    }

    // Start is called before the first frame update
    private void Start()
    {
        has_Animator = TryGetComponent<Animator>(out animator);

        anim = GetComponent<Animator>();

        ragdollRigids = new List<Rigidbody>(transform.GetComponentsInChildren<Rigidbody>());
        ragdollRigids.Remove(GetComponent<Rigidbody>());

        DeactivateRagdoll();
    }

    // Update is called once per frame
    private void Update()
    {
        CalculateView();
        CalculateMovement();
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
