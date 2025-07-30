using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

//@TriceratopsGang
public class PlayerController : MonoBehaviour
{
    public bool CanDebug { get; private set; } = true;
    public bool CanLook { get; private set; } = true;
    public bool CanMove { get; private set; } = true;
    public bool CanInteract { get; private set; } = true;

    [Header("Dependencies")]
    [SerializeReference] private Camera fpsCamera;

    [Header("Look & Feel")]
    [SerializeField, Range(0.01f, 100f)] private float sensitivityScalar = 0.5f;
    [SerializeField, Range(0.01f, 100f)] private float yawSensitivity = 0.5f;
    [SerializeField, Range(0.01f, 100f)] private float pitchSensitivity = 0.5f;
    [SerializeField] private bool yawInversion = false;
    [SerializeField] private bool pitchInversion = false;

    [Header("Fov & Zoom")]
    [SerializeField, Range(30f, 90f)] private float defaultFov = 60f;
    [SerializeField, Range(1f, 4f)] private float zoomScalar = 2f;
    [SerializeField, Range(0f, 3f)] private float zoomDuration = 0.2f;

    [Header("Jump & Gravity")]
    [SerializeField] private float gravityScalar = 1f;
    [SerializeField] private float jumpHeight = 1f;

    [Header("Speed")]
    [SerializeField] private float walkSpeed = 4f;
    [SerializeField] private float acceleration = 24f;
    [SerializeField] private float deceleration = 32f;

    [Header("Interaction")]
    [SerializeField] private float interactionRange = 3f;

    private CharacterController characterController;
    private InputSystem_Actions inputActions;
    private VitalsComponent vitalsComponent;

    private InputAction aLook;
    private InputAction aZoom;
    private InputAction aMove;
    private InputAction aJump;
    private InputAction aInteract;
    private InputAction aHeal;
    private InputAction aDamage;

    private Vector2 moveInput;
    private Vector2 lookInput;

    private float yaw;
    private float pitch;
    private const float PitchLimit = 86f;
    private const float MinFov = 30f;
    private const float MaxFov = 90f;
    private Coroutine zoomRoutine;

    private Vector3 velocity;
    //I would like to remove this, but our current movement code does not allow that.
    private float yVelocity;
    private bool IsGrounded => characterController.isGrounded;

    private IInteractable currentInteractable;

    private void Awake()
    {
        inputActions = new InputSystem_Actions();
        characterController = GetComponent<CharacterController>();
        vitalsComponent = GetComponent<VitalsComponent>();

        aLook = inputActions.Player.Look;
        aZoom = inputActions.Player.Zoom;
        aMove = inputActions.Player.Move;
        aJump = inputActions.Player.Jump;
        aInteract = inputActions.Player.Interact;

        aHeal = inputActions.Debug.Heal;
        aDamage = inputActions.Debug.Damage;
    }

    private void OnEnable()
    {
        inputActions.Enable();
    }

    private void OnDisable()
    {
        inputActions.Disable();
    }

    private void OnDestroy()
    {
        inputActions.Dispose();
    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;

        ResetCamera();
    }

    private void Update()
    {
        if (CanMove)
        {
            ApplyGravity();

            HandleMovement();
            HandleJumping();

            ApplyVelocity();
        }

        if (CanInteract)
        {
            InteractionTrace();
            HandleInteract();
        }

        if (CanDebug)
        {
            HandleDebug();
        }
    }

    private void LateUpdate()
    {
        if (CanLook)
        {
            HandleLook();
            HandleZoom();
        }
    }

    private void HandleDebug()
    {
        if (aHeal.triggered)
        {
            vitalsComponent.TakeHealing(5f, this.gameObject);
        }

        if (aDamage.triggered)
        {
            vitalsComponent.TakeDamage(5f, this.gameObject);
        }
    }

    private void ResetCamera()
    {
        AdjustCameraFOV(defaultFov);
        fpsCamera.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
    }

    private void AdjustCameraFOV(float value)
    {
        value = Mathf.Clamp(value, MinFov, MaxFov);
        fpsCamera.fieldOfView = value;
    }

    private void HandleLook()
    {
        lookInput = aLook.ReadValue<Vector2>() * sensitivityScalar;

        yaw = lookInput.x * (yawInversion ? -yawSensitivity : yawSensitivity);
        pitch -= lookInput.y * (pitchInversion ? -pitchSensitivity : pitchSensitivity);
        pitch = Mathf.Clamp(pitch, -PitchLimit, PitchLimit);

        transform.Rotate(Vector3.up * yaw);
        fpsCamera.transform.localRotation = Quaternion.Euler(pitch, 0f, 0f);
    }

    private void HandleZoom()
    {
        if (aZoom.WasPerformedThisFrame())
        {
            if (zoomRoutine != null)
            {
                StopCoroutine(zoomRoutine);
                zoomRoutine = null;
            }
            zoomRoutine = StartCoroutine(ToggleZoom(true));
        }

        if (aZoom.WasCompletedThisFrame())
        {
            if (zoomRoutine != null)
            {
                StopCoroutine(zoomRoutine);
                zoomRoutine = null;
            }
            zoomRoutine = StartCoroutine(ToggleZoom(false));
        }
    }

    private IEnumerator ToggleZoom(bool isEnter)
    {
        float targetFov = isEnter ? defaultFov / zoomScalar : defaultFov;
        float startingFov = fpsCamera.fieldOfView;
        float eTime = 0f;

        while (eTime < zoomDuration)
        {
            AdjustCameraFOV(Mathf.Lerp(startingFov, targetFov, eTime / zoomDuration));
            eTime += Time.deltaTime;
            yield return null;
        }

        AdjustCameraFOV(targetFov);
        zoomRoutine = null;
    }

    private void ApplyVelocity()
    {
        velocity.y = yVelocity;
        characterController.Move(velocity * Time.deltaTime);
    }

    private void ApplyGravity()
    {
        if (IsGrounded)
        {
            yVelocity = -1f;
        }

        else
        {
            yVelocity += Physics.gravity.y * Time.deltaTime * gravityScalar;
        }
    }

    private void HandleMovement()
    {
        //Read input (WASD / stick)
        moveInput = aMove.ReadValue<Vector2>();

        //Calculate the *target* velocity from input
        Vector3 inputDir = transform.right * moveInput.x + transform.forward * moveInput.y;
        Vector3 targetVelocity = inputDir * walkSpeed;

        //Choose which rate to use (accelerating or decelerating)
        float rate = (moveInput.sqrMagnitude > 0.01f) ? acceleration : deceleration;

        //Smoothly move current velocity toward target velocity
        velocity = Vector3.Lerp(velocity, targetVelocity, rate * Time.deltaTime);
    }

    private void HandleJumping()
    {
        if (aJump.IsPressed() && IsGrounded)
        {
            yVelocity = Mathf.Sqrt((jumpHeight * -2f * Physics.gravity.y) * gravityScalar);
        }
    }

    private void InteractionTrace()
    {
        Ray ray = fpsCamera.ViewportPointToRay(Vector3.one / 2f);
        RaycastHit hit;

        IInteractable hitInteractable = null;

        if (Physics.Raycast(ray, out hit, interactionRange))
        {
            hitInteractable = hit.collider.transform.root.GetComponentInChildren<IInteractable>();
        }

        if (hitInteractable != currentInteractable)
        {
            if (currentInteractable != null)
                currentInteractable.SetFocus(false);

            if (hitInteractable != null)
                hitInteractable.SetFocus(true);

            currentInteractable = hitInteractable;
        }
    }

    private void HandleInteract()
    {
        if (aInteract.triggered && currentInteractable != null)
        {
            currentInteractable.InteractWith();
        }
    }
}
