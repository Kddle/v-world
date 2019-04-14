using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class TPSCharacter : MonoBehaviour
{
    // Necessary Components
    private Transform ArrowComponent;
    private Transform CharacterModel;
    private Transform CameraPole;
    private Camera PlayerCamera;

    // TPS Camera Settings
    [SerializeField]
    private float CameraDistance = 5f;
    [SerializeField]
    private float CameraMaxDistance = 15f;
    [SerializeField]
    private float CameraMinDistance = .5f;
    [SerializeField]
    private float CameraAngleMin = -20.0f;
    [SerializeField]
    private float CameraAngleMax = 60.0f;
    [SerializeField]
    private float CameraSmoothFactor = .125f;

    // Camera and Body Settings
    [SerializeField]
    private float PlayerSpeed = 5f;
    [SerializeField]
    private float Sensitivity = 120f;

    // Inputs Axis Names
    [SerializeField]
    private string HorizontalAxisName = "Horizontal";
    [SerializeField]
    private string VerticalAxisName = "Vertical";
    [SerializeField]
    private string XLookAxisName = "Mouse X";
    [SerializeField]
    private string YLookAxisName = "Mouse Y";
    [SerializeField]
    private string JumpAxisName = "Jump";

    public bool InvertCameraY = true;

    // ----
    float YRotation;
    float XRotation;
    Vector3 Velocity;

    Rigidbody PlayerRigidbody;

    Quaternion BodyRotation;

    [SerializeField]
    private float BodyRotationSmoothFactor = 0.15f;

    [SerializeField]
    private float JumpForce = 50f;

    [SerializeField]
    private float FloorDistanceToJump = 1.15f;

    // Accessors
    public float CurrentCameraDistance
    {
        get { return CameraDistance; }
        set { CameraDistance = value; }
    }

    private bool Jump = false;

    void Start()
    {
        ArrowComponent = transform.Find("ArrowComponent");
        CharacterModel = transform.Find("CharacterModel");
        CameraPole = transform.Find("CameraPole");

        PlayerRigidbody = GetComponent<Rigidbody>();

        if (CameraPole != null)
            PlayerCamera = CameraPole.GetComponentInChildren<Camera>();

        if (ArrowComponent == null || CharacterModel == null || CameraPole == null || PlayerCamera == null)
        {
            Debug.LogException(new System.Exception("TPSCharacterController is corrupted. Base linked gameobjects have been deleted or renamed, please use the default Prefab as model."));
            return;
        }

        // Camera Positionning
        PlayerCamera.transform.position = new Vector3(PlayerCamera.transform.position.x, PlayerCamera.transform.position.y, -CameraDistance);

        // Cursor Setup
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        // // Body Movement
        float hValue = Input.GetAxisRaw(HorizontalAxisName);
        float vValue = Input.GetAxisRaw(VerticalAxisName);

        Vector3 HorizontalMove = ArrowComponent.transform.right * hValue;
        Vector3 VerticalMove = ArrowComponent.transform.forward * vValue;

        Velocity = (HorizontalMove + VerticalMove).normalized * PlayerSpeed;

        if (Velocity != Vector3.zero)
            BodyRotation = Quaternion.LookRotation((HorizontalMove + VerticalMove).normalized, CharacterModel.up);

        // Camera & Body Rotation
        YRotation += Input.GetAxis(XLookAxisName) * Sensitivity * Time.deltaTime;

        XRotation += Input.GetAxis(YLookAxisName) * Sensitivity * Time.deltaTime * (InvertCameraY ? -1f : 1f);
        XRotation = Mathf.Clamp(XRotation, CameraAngleMin, CameraAngleMax);

        // Jump
        if (Input.GetAxisRaw(JumpAxisName) != 0.0f)
            if (Physics.Raycast(transform.position, transform.up * -1f, FloorDistanceToJump))
                Jump = true;
    }

    void FixedUpdate()
    {
        ArrowComponent.localRotation = Quaternion.Euler(0f, YRotation, 0f); // Forward Direction
        CameraPole.localRotation = Quaternion.Euler(XRotation, YRotation, 0f); // Forward Direction

        // Apply Body Movement
        if (Velocity != Vector3.zero)
        {
            PlayerRigidbody.MovePosition(PlayerRigidbody.position + Velocity * Time.fixedDeltaTime);
            CharacterModel.localRotation = Quaternion.Slerp(CharacterModel.rotation, BodyRotation, BodyRotationSmoothFactor);
        }

        // Character Animation
        CharacterModel.GetComponent<Animator>().SetFloat("Speed", Velocity.magnitude);

        // Apply Jump Movement
        if(Jump)
        {
            PlayerRigidbody.AddForceAtPosition(new Vector3(0f, JumpForce, 0f), transform.position - (transform.up * -1f));
            Jump = false;
        }
    }


}