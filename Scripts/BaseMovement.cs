using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode;
using Unity.Netcode.Components;
using Unity.Cinemachine;

[RequireComponent(typeof(CharacterController))]
public class BaseMovement : NetworkBehaviour
{
    [Header("Movement Settings")]
    public float walkSpeed = 7f;
    public float runSpeed = 10f;
    public float jumpHeight = 2f;
    public float gravity = -9.81f;
    public float slideSpeed = 10f;
    public float slideDuration = 1f;
    public ParticleSystem DustParticle;
    public ParticleSystem FootstepParticle;

    [Header("Footstep Settings")]
    public float footstepInterval = 0.5f;
    private float footstepTimer;

    [Header("Camera Reference")]
    public Transform cameraTransform;

    [Header("Ground Check")]
    public LayerMask groundLayer;
    public float groundCheckDistance = 0.3f;
    public float maxSlopeAngle = 45f;

    [Header("UI")]
    public Canvas canvas;

    private CharacterController controller;
    private InputSystem_Actions inputActions;
    private Animator animator;
    private NetworkAnimator netAnimator;

    private Vector2 inputMovement;
    private Vector3 velocity;
    private bool movementEnabled = true;

    private bool isSliding;
    private float slideTimer;
    private float lastSlideTime = -Mathf.Infinity;
    public float slideCooldown = 0.3f;

    private NetworkVariable<float> netSpeed = new(writePerm: NetworkVariableWritePermission.Owner);
    private NetworkVariable<bool> netGrounded = new(writePerm: NetworkVariableWritePermission.Owner);
    private NetworkVariable<bool> netSliding = new(writePerm: NetworkVariableWritePermission.Owner);

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponentInChildren<Animator>();
        netAnimator = animator.GetComponent<NetworkAnimator>();
        inputActions = new InputSystem_Actions();
    }

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            inputActions.Player.Enable();
            inputActions.Player.Jump.performed += ctx => Jump();
            inputActions.Player.Slide.performed += ctx => StartSlide();
            inputActions.Player.Action.performed += ctx => DoAction();

            if (canvas != null)
                canvas.gameObject.SetActive(true);

            var cineCam = Object.FindFirstObjectByType<CinemachineCamera>();
            if (cineCam != null)
            {
                cineCam.Follow = transform;
                cineCam.LookAt = transform;
                cameraTransform = cineCam.transform;
            }
            else
            {
                Debug.LogWarning("CinemachineCamera not found in scene!");
            }
        }
        else
        {
            if (canvas != null)
                canvas.gameObject.SetActive(false);
        }
    }

    private void OnDisable()
    {
        if (IsOwner && inputActions != null)
            inputActions.Player.Disable();
    }

    void Update()
    {
        if (IsOwner)
        {
            if (!movementEnabled)
            {
                controller.Move(Vector3.zero);
                return;
            }

            bool isGrounded = CheckIfGrounded();
            if (isGrounded && velocity.y < 0)
                velocity.y = -2f;

            inputMovement = inputActions.Player.Move.ReadValue<Vector2>();
            Vector3 move = GetMoveDirection(inputMovement);

            float currentSpeed = inputActions.Player.Sprint.IsPressed() ? runSpeed : walkSpeed;


            

            if (isSliding)
            {
                move *= slideSpeed;
                if (DustParticle != null) DustParticle.Play();

                slideTimer -= Time.deltaTime;
                if (slideTimer <= 0f) isSliding = false;
            }
            else
            {
                move *= currentSpeed;
            }

            if (move.magnitude > 0.1f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(move);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
            }

            controller.Move(move * Time.deltaTime);

            velocity.y += gravity * Time.deltaTime;
            controller.Move(velocity * Time.deltaTime);

            ResolveWallStuck();

            PlayFootstepEffect(move);

            netSpeed.Value = move.magnitude / runSpeed;
            netGrounded.Value = isGrounded;
            netSliding.Value = isSliding;
        }


        UpdateAnimations();
    }

    Vector3 GetMoveDirection(Vector2 input)
    {
        if (cameraTransform == null) return Vector3.zero;

        Vector3 forward = cameraTransform.forward;
        Vector3 right = cameraTransform.right;

        forward.y = 0f;
        right.y = 0f;
        forward.Normalize();
        right.Normalize();

        return forward * input.y + right * input.x;
    }

    public void Jump()
    {
        if (netGrounded.Value && !isSliding)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            netAnimator.SetTrigger("Jump");
        }
    }

    public void StartSlide()
    {
        if (Time.time < lastSlideTime + slideCooldown) return;
        if (!netGrounded.Value || isSliding) return;

        isSliding = true;
        slideTimer = slideDuration;
        lastSlideTime = Time.time;

        netAnimator.SetTrigger("Slide");
    }

    public void OnSlideAnimationEnd() => isSliding = false;

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (isSliding && hit.collider != null && !hit.collider.isTrigger && hit.gameObject.tag != "Ground")
        {
            isSliding = false;
            slideTimer = 0f;

            animator.ResetTrigger("Slide");
            animator.SetFloat("Speed", 0f);
            animator.SetBool("IsSliding", false);

            netAnimator.SetTrigger("Idle");
        }
    }

    public void DoAction()
    {
        var roleHandler = GetComponent<RoleHandler>();
        if (roleHandler == null) return;

        if (roleHandler.CurrentRole == PlayerRole.Chaser)
        {
            GetComponent<ChaserRole>()?.TryFreeze();
        }
        else if (roleHandler.CurrentRole == PlayerRole.Runner)
        {
            GetComponent<RunnerRole>()?.TryUnfreeze();
        }
    }

    void UpdateAnimations()
    {
        animator.SetFloat("Speed", netSpeed.Value);
        animator.SetBool("IsGrounded", netGrounded.Value);
        animator.SetBool("IsSliding", netSliding.Value);

        if (!netGrounded.Value)
        {
            if (netSpeed.Value > 0.1f)
                animator.Play("RunJump");
        }
    }

    public void SetMovementEnabled(bool enabled)
    {
        movementEnabled = enabled;
        if (!enabled)
        {
            inputMovement = Vector2.zero;
            velocity = Vector3.zero;
            animator.SetFloat("Speed", 0f);
        }
    }

    public void PlayFootstepEffect(Vector3 move)
    {
        if (netGrounded.Value && move.magnitude > 0.1f && !isSliding)
        {
            float speedFactor = inputActions.Player.Sprint.IsPressed() ? runSpeed : walkSpeed;
            float interval = footstepInterval * (walkSpeed / speedFactor);

            footstepTimer -= Time.deltaTime;
            if (footstepTimer <= 0f)
            {
                if (FootstepParticle != null)
                    FootstepParticle.Play();

                footstepTimer = interval;
            }
        }
        else
        {
            footstepTimer = 0f;
        }
    }

    private void ResolveWallStuck()
    {
        if (!controller.enabled) return;

        Collider[] hits = Physics.OverlapCapsule(
            controller.bounds.center + Vector3.up * -controller.height / 2f,
            controller.bounds.center + Vector3.up * controller.height / 2f,
            controller.radius,
            ~0,
            QueryTriggerInteraction.Ignore);

        foreach (var hit in hits)
        {
            if (hit.transform == transform) continue;

            Vector3 direction;
            float distance;
            if (Physics.ComputePenetration(
                controller, transform.position, transform.rotation,
                hit, hit.transform.position, hit.transform.rotation,
                out direction, out distance))
            {
                Vector3 resolve = direction * (distance + 0.01f);
                controller.Move(resolve);
            }
        }
    }




    bool CheckIfGrounded()
    {
        RaycastHit hit;
        Vector3 origin = transform.position + Vector3.up * 0.1f;
        if (Physics.Raycast(origin, Vector3.down, out hit, groundCheckDistance, groundLayer))
        {
            float slopeAngle = Vector3.Angle(hit.normal, Vector3.up);
            return slopeAngle <= maxSlopeAngle;
        }
        return false;
    }
}
