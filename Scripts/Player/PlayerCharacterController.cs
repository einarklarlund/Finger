using UnityEngine;
using UnityEngine.Events;
using Zenject;
using Signals;

[RequireComponent(typeof(CharacterController), typeof(PlayerInputHandler), typeof(AudioSource))]
public class PlayerCharacterController : Singleton<PlayerCharacterController>
{
    [Header("References")]
    [Tooltip("Reference to the main camera used for the player")]
    public Camera playerCamera;
    [Tooltip("Audio source for footsteps, jump, etc...")]
    public AudioSource audioSource;

    [Header("General")]
    [Tooltip("Force applied downward when in the air")]
    public float gravityDownForce = 20f;
    [Tooltip("Physic layers checked to consider the player grounded")]
    public LayerMask groundCheckLayers = -1;
    [Tooltip("distance from the bottom of the character controller capsule to test for grounded")]
    public float groundCheckDistance = 0.05f;

    [Header("Movement")]
    [Tooltip("Total amount of frames in which player can buffer jump before hitting the ground")]
    public int totalJumpBufferFrames = 20;
    [Tooltip("Total amount of frames that bunny hop speed boost lasts when on the ground")]
    public int jumpBoostDuration = 20;
    [Tooltip("Maximum speed boost modifier received from bunny hopping")]
    public float maxJumpSpeedBoost = 1.2f;
    [Tooltip("Max movement speed when grounded (when not sprinting)")]
    public float maxSpeedOnGround = 10f;
    [Tooltip("Sharpness for the movement when grounded, a low value will make the player accelerate and decelerate slowly, a high value will do the opposite")]
    public float movementSharpnessOnGround = 15;
    [Tooltip("Max movement speed when crouching")]
    [Range(0,1)]
    public float maxSpeedCrouchedRatio = 0.5f;
    [Tooltip("Acceleration speed when in the air")]
    public float accelerationSpeedInAir = 25f;
    [Tooltip("Multiplicator for the sprint speed (based on grounded speed)")]
    public float sprintSpeedModifier = 2f;
    [Tooltip("Height at which the stage will be reloaded when falling off the map")]
    public float reloadHeight = -50f;

    [Header("Rotation")]
    [Tooltip("Rotation speed for moving the camera")]
    public float rotationSpeed = 200f;
    [Tooltip("Minimum rotation speed when following a viewpoint automotically")]
    public float minRotationSpeed = 10f;

    [Header("Jump")]
    [Tooltip("Force applied upward when jumping")]
    public float jumpForce = 9f;

    [Header("Stance")]
    [Tooltip("Ratio (0-1) of the character height where the camera will be at")]
    public float cameraHeightRatio = 0.9f;
    [Tooltip("Height of character when standing")]
    public float capsuleHeightStanding = 1.8f;
    [Tooltip("Height of character when crouching")]
    public float capsuleHeightCrouching = 0.9f;
    [Tooltip("Speed of crouching transitions")]
    public float crouchingSharpness = 10f;

    [Header("Audio")]
    [Tooltip("Amount of footstep sounds played when moving one meter")]
    public float footstepSFXFrequency = 1f;
    [Tooltip("Amount of footstep sounds played when moving one meter while sprinting")]
    public float footstepSFXFrequencyWhileSprinting = 1f;
    [Tooltip("Sound played for footsteps")]
    public AudioClip footstepSFX;
    [Tooltip("Sound played when jumping")]
    public AudioClip jumpSFX;
    [Tooltip("Sound played when landing")]
    public AudioClip landSFX;
    [Tooltip("Sound played when taking damage froma fall")]
    public AudioClip fallDamageSFX;


    public bool disableMovement = false;

    public UnityAction<bool> onStanceChanged;

    public Vector3 characterVelocity { get; set; }
    public bool isGrounded { get; private set; }
    public bool hasJumpedThisFrame { get; private set; }
    public bool isDead { get; private set; }
    public bool isCrouching { get; private set; }
    public bool isLookingTowardsViewPoint { get; private set; }
    public bool isInvincible = false;
    
    GameManager m_GameManager;
    PlayerInputHandler m_InputHandler;
    CharacterController m_Controller;
    PlayerPickupManager m_PlayerPickupManager;
    Transform m_ViewPoint;
    PlayerStartPosition m_PlayerStartPosition;
    Camera m_Camera;

    Vector3 m_GroundNormal;
    Vector3 m_CharacterVelocity;
    Vector3 m_LatestImpactSpeed;

    float m_LastTimeJumped = 0f;
    float m_CameraVerticalAngle = 0f;
    float m_footstepDistanceCounter;
    float m_TargetCharacterHeight;
    
    int m_JumpBuffer = 0;
    int m_JumpBoostDuration = 0; 
    float m_JumpBoostCharge = 0f;
    float m_LookAtViewPointTime = 0f;
    float m_LookAtViewPointDuration = 0f;

    private int _layerMask = 4094;
    private float m_InstantiationTime;

    const float k_JumpGroundingPreventionTime = 0.2f;
    const float k_GroundCheckDistanceInAir = 0.07f;

    [Inject]
    public void Construct(GameManager gameManager, PlayerStartPosition playerStartPosition, Camera camera)
    {
        m_GameManager = gameManager;
        m_PlayerStartPosition = playerStartPosition;
        m_Camera = camera;
    }

    void Start()
    {
        // fetch components on the same gameObject
        m_Controller = GetComponent<CharacterController>();
        if(!m_Controller)
        {
            Debug.LogError("[PlayerCharacterController] Could not find m_Controller");
        }

        m_InputHandler = GetComponent<PlayerInputHandler>();
        if(!m_InputHandler)
        {
            Debug.LogError("[PlayerCharacterController] Could not find m_InputHandler");
        }

        m_PlayerPickupManager = GetComponent<PlayerPickupManager>();
        if(!m_PlayerPickupManager)
        {
            Debug.LogError("[PlayerCharacterController] Could not find PlayerPickupManager");
        }

        m_Controller.enableOverlapRecovery = true;

        // force the crouch state to false when starting
        SetCrouchingState(false, true);
        UpdateCharacterHeight(true);

        isDead = false;

        m_InstantiationTime = Time.time;
    }

    void Update()
    {
        if(isDead || m_GameManager.CurrentGameState != GameState.RUNNING)
            return;

        // check Y for reload
        if(transform.position.y < reloadHeight)
        {
            m_GameManager?.TransitionToStage(m_GameManager.currentStage);
        }

        hasJumpedThisFrame = false;

        bool wasGrounded = isGrounded;
        GroundCheck();

        //reset the boost's charge if it has expired its duration on the ground
        if(m_JumpBoostDuration <= 0)
        {
            m_JumpBoostCharge = 0;
        }

        //subtract from bhop speed boost dyration
        if(isGrounded && m_JumpBoostDuration > 0)
        {
            m_JumpBoostDuration--;
        }

        // landing
        // only play sfx if landing is at least 1 second after instantiation
        if (Time.time - m_InstantiationTime > 1f && isGrounded && !wasGrounded)
        {
            //reset the jump boost duration on landing
            m_JumpBoostDuration = jumpBoostDuration;

            // land SFX
            audioSource.PlayOneShot(landSFX);
        }

        // crouching
        if (m_InputHandler.GetCrouchInputDown())
        {
            SetCrouchingState(!isCrouching, false);
        }

        UpdateCharacterHeight(false);

        if(isLookingTowardsViewPoint)
        {
            HandleLookTowardsViewPoint();
        }
        else if(!disableMovement)
        {
            HandleCharacterMovement();
        }
    }

    public void SetPositionAndRotation(Vector3 position, Vector3 rotation)
    {
        transform.position = position;
        transform.Rotate(new Vector3(0, rotation.y, 0));
        // m_CameraVerticalAngle = m_PlayerStartPosition.GetRotation().x;
        if(rotation.x < 180)
        {
            m_CameraVerticalAngle = rotation.x;
        }
        else
        {
            m_CameraVerticalAngle = rotation.x - 360;
        }

        playerCamera.transform.localEulerAngles = new Vector3(m_CameraVerticalAngle, 0, 0);
    }

    public void OnItemThrown(ItemThrownSignal signal)
    {
        // launch the item from the position of playertransform   
        signal.transform.position = transform.position + transform.forward * 1.2f + Vector3.up * 1.5f;
        signal.transform.rotation = transform.rotation;

        signal.rigidbody.velocity = signal.transform.forward * signal.throwableItem.throwSpeed;
    }

    public void OnInteractableSpeechBegan(InteractableSpeechBeganSignal signal)
    {
        if(signal.interactableSpeech.viewPoint)
        {
            LookAtViewPoint(signal.interactableSpeech.viewPoint);
        }
    }

    public void OnInteractableSpeechEnded()
    {
        StopLookingAtViewPoint();
    }

    public void LookAtViewPoint(Transform viewPoint, float duration = 1.5f)
    {
        isLookingTowardsViewPoint = true;
        m_LookAtViewPointTime = Time.time;
        m_LookAtViewPointDuration = duration;
        m_ViewPoint = viewPoint;
    }

    public void StopLookingAtViewPoint()
    {
        isLookingTowardsViewPoint = false;
    }

    public void HandleLookTowardsViewPoint()
    {
        // calculate the amount of degrees to rotate on this frame
        float t = (Time.time - m_LookAtViewPointTime) * Time.deltaTime;

        // set delta theta to the solution of theta = -t^2 + d*t. if LookAtViewPointDuration has been passed, set it to the midpoint of the graph of theta. 
        float deltaTheta = t > m_LookAtViewPointDuration ? 
            -(m_LookAtViewPointDuration * m_LookAtViewPointDuration / 4) + (m_LookAtViewPointDuration * m_LookAtViewPointDuration) / 2 : 
            -t * t + m_LookAtViewPointDuration * t;
        
        // find the target vector using Vector3.RotateTowards with the vector viewpoint - cameraposition
        Vector3 targetVector = Vector3.RotateTowards(playerCamera.transform.forward, 
            m_ViewPoint.position - playerCamera.transform.position, deltaTheta, 0.0f);
        
        // project target vector onto local x-z plane and to the local y-z plane to get x & y components
        Vector3 targetVectorXZ = Vector3.ProjectOnPlane(targetVector, transform.up);
        Vector3 targetVectorYZ = Vector3.ProjectOnPlane(targetVector, transform.right);

        // project camera forward vector onto local x-z plane and to the local y-z plane to get x & y components
        Vector3 cameraForwardXZ = Vector3.ProjectOnPlane(playerCamera.transform.forward, transform.up);
        Vector3 cameraForwardYZ = Vector3.ProjectOnPlane(playerCamera.transform.forward, transform.right);

        // find angle between target vector components and camera forward components
        float displacementHorizontal = Vector3.Angle(targetVectorXZ, cameraForwardXZ);
        float displacementVertical = Vector3.Angle(targetVectorYZ, cameraForwardYZ);

        // put target and camera forward vectors into local space, so that we can tell if displacement needs to be added or subtracted to the rotation
        Vector3 targetVectorLocal = transform.InverseTransformVector(targetVector);
        Vector3 cameraForwardLocal = transform.InverseTransformVector(playerCamera.transform.forward);


        // change polarity of the rotation if necessary
        displacementHorizontal *= targetVectorLocal.x > cameraForwardLocal.x ? 1 : -1;
        displacementVertical *= targetVectorLocal.y > cameraForwardLocal.y ? -1 : 1;

        // horizontal character rotation
        {
            // rotate the transform with the input speed around its local Y axis
            transform.Rotate(new Vector3(0f, displacementHorizontal, 0f), Space.Self);
        }

        // vertical camera rotation
        {
            // add vertical inputs to the camera's vertical angle
            m_CameraVerticalAngle += displacementVertical;
            playerCamera.transform.Rotate(new Vector3(displacementVertical, 0, 0));
        }
    }

    void GroundCheck()
    {
        // Make sure that the ground check distance while already in air is very small, to prevent suddenly snapping to ground
        float chosenGroundCheckDistance = isGrounded ? (m_Controller.skinWidth + groundCheckDistance) : k_GroundCheckDistanceInAir;

        // reset values before the ground check
        isGrounded = false;
        m_GroundNormal = Vector3.up;

        // only try to detect ground if it's been a short amount of time since last jump; otherwise we may snap to the ground instantly after we try jumping
        if (Time.time >= m_LastTimeJumped + k_JumpGroundingPreventionTime)
        {
            // if we're grounded, collect info about the ground normal with a downward capsule cast representing our character capsule
            if (Physics.CapsuleCast(GetCapsuleBottomHemisphere(), GetCapsuleTopHemisphere(m_Controller.height), m_Controller.radius, Vector3.down, out RaycastHit hit, chosenGroundCheckDistance, groundCheckLayers, QueryTriggerInteraction.Ignore))
            {
                // storing the upward direction for the surface found
                m_GroundNormal = hit.normal;

                // Only consider this a valid ground hit if the ground normal goes in the same direction as the character up
                // and if the slope angle is lower than the character controller's limit
                if (Vector3.Dot(hit.normal, transform.up) > 0f &&
                    IsNormalUnderSlopeLimit(m_GroundNormal))
                {
                    isGrounded = true;

                    // handle snapping to the ground
                    if (hit.distance > m_Controller.skinWidth)
                    {
                        m_Controller.Move(Vector3.down * hit.distance);
                    }
                }
            }
        }
    }

    void HandleCharacterMovement()
    {
        // horizontal character rotation
        {
            // rotate the transform with the input speed around its local Y axis
            transform.Rotate(new Vector3(0f, (m_InputHandler.GetLookInputsHorizontal() * rotationSpeed), 0f), Space.Self);
        }

        // vertical camera rotation
        {
            // add vertical inputs to the camera's vertical angle
            m_CameraVerticalAngle += m_InputHandler.GetLookInputsVertical() * rotationSpeed;

            // limit the camera's vertical angle to min/max
            m_CameraVerticalAngle = Mathf.Clamp(m_CameraVerticalAngle, -89f, 89f);

            // apply the vertical angle as a local rotation to the camera transform along its right axis (makes it pivot up and down)
            playerCamera.transform.localEulerAngles = new Vector3(m_CameraVerticalAngle, 0, 0);
        }

        // character movement handling
        bool isSprinting = m_InputHandler.GetSprintInputHeld();
        {
            if (isSprinting)
            {
                isSprinting = SetCrouchingState(false, false);
            }

            float speedModifier = isSprinting ? sprintSpeedModifier : 1f;
            // float speedModifier = sprintSpeedModifier;
            
            // converts move input to a worldspace vector based on our character's transform orientation
            Vector3 worldspaceMoveInput = transform.TransformVector(m_InputHandler.GetMoveInput());
            
            if(m_InputHandler.GetJumpInputDown())
            {
                m_JumpBuffer = totalJumpBufferFrames;
            }

            float jumpSpeedBoost = (maxJumpSpeedBoost - 1f) * m_JumpBoostCharge + 1f;

            // handle grounded movement
            if (isGrounded)
            {
                //linear interpolate bewteen 1 and jumpSpeedBoost using t = m_JumpBoostDuration/jumpBoostDuration
                float lerpedSpeedBoost = (jumpSpeedBoost - 1) * m_JumpBoostDuration/jumpBoostDuration + 1;
                // calculate the desired velocity from inputs, max speed, and current slope
                Vector3 targetVelocity;
                if(m_InputHandler.GetFireInputHeld())
                {
                    targetVelocity = worldspaceMoveInput * maxSpeedOnGround * speedModifier * lerpedSpeedBoost;
                }
                else
                {
                    targetVelocity = worldspaceMoveInput * maxSpeedOnGround * speedModifier * lerpedSpeedBoost;
                }
                // reduce speed if crouching by crouch speed ratio
                if (isCrouching)
                    targetVelocity *= maxSpeedCrouchedRatio;
                targetVelocity = GetDirectionReorientedOnSlope(targetVelocity.normalized, m_GroundNormal) * targetVelocity.magnitude;

                // smoothly interpolate between our current velocity and the target velocity based on acceleration speed
                characterVelocity = Vector3.Lerp(characterVelocity, targetVelocity, movementSharpnessOnGround * Time.deltaTime);

                // jumping
                // if (isGrounded && m_InputHandler.GetJumpInputDown())
                if(isGrounded && m_JumpBuffer > 0)
                {
                    //build up the bunny hopping jump boost
                    if(m_JumpBoostCharge < 1f)
                    {
                        m_JumpBoostCharge += 0.2f;
                    }

                    // force the crouch state to false
                    if (SetCrouchingState(false, false))
                    {
                        // start by canceling out the vertical component of our velocity
                        characterVelocity = new Vector3(characterVelocity.x, 0f, characterVelocity.z);

                        // then, add the jumpSpeed value upwards
                        characterVelocity += Vector3.up * jumpForce;

                        // play sound
                        audioSource.PlayOneShot(jumpSFX);

                        // remember last time we jumped because we need to prevent snapping to ground for a short time
                        m_LastTimeJumped = Time.time;
                        hasJumpedThisFrame = true;

                        // Force grounding to false
                        isGrounded = false;
                        m_GroundNormal = Vector3.up;
                    }

                }

                // footsteps sound
                float chosenFootstepSFXFrequency = (isSprinting ? footstepSFXFrequencyWhileSprinting : footstepSFXFrequency);
                if (m_footstepDistanceCounter >= 1f / chosenFootstepSFXFrequency)
                {
                    m_footstepDistanceCounter = 0f;
                    audioSource.PlayOneShot(footstepSFX);
                }

                // keep track of distance traveled for footsteps sound
                m_footstepDistanceCounter += characterVelocity.magnitude * Time.deltaTime;
            }
            // handle air movement
            else
            {
                // add air acceleration
                characterVelocity += worldspaceMoveInput * accelerationSpeedInAir * Time.deltaTime * jumpSpeedBoost;
                // characterVelocity += worldspaceMoveInput * accelerationSpeedInAir * speedModifier Time.deltaTime;

                // limit air speed to a maximum, but only horizontally
                float verticalVelocity = characterVelocity.y;
                Vector3 horizontalVelocity = Vector3.ProjectOnPlane(characterVelocity, Vector3.up);
                // horizontalVelocity = Vector3.ClampMagnitude(horizontalVelocity, maxSpeedInAir * speedModifier);
                if(!m_InputHandler.GetFireInputHeld())
                {
                    horizontalVelocity = Vector3.ClampMagnitude(horizontalVelocity, maxSpeedOnGround * speedModifier * jumpSpeedBoost);
                }
                else
                {
                    horizontalVelocity = Vector3.ClampMagnitude(horizontalVelocity, maxSpeedOnGround * speedModifier * jumpSpeedBoost);
                }
                characterVelocity = horizontalVelocity + (Vector3.up * verticalVelocity);

                // apply the gravity to the velocity
                characterVelocity += Vector3.down * gravityDownForce * Time.deltaTime;
            }
        }

        if(m_JumpBuffer > 0)
        {
            m_JumpBuffer--;
        }

        // apply the final calculated velocity value as a character movement
        Vector3 capsuleBottomBeforeMove = GetCapsuleBottomHemisphere();
        Vector3 capsuleTopBeforeMove = GetCapsuleTopHemisphere(m_Controller.height);
        m_Controller.Move(characterVelocity * Time.deltaTime);

        // detect obstructions to adjust velocity accordingly
        m_LatestImpactSpeed = Vector3.zero;
        //layermask will account for layers 1-9: 1111 1111 1110 = 4094
        if (Physics.CapsuleCast(capsuleBottomBeforeMove, capsuleTopBeforeMove, m_Controller.radius, characterVelocity.normalized, out RaycastHit hit, characterVelocity.magnitude * Time.deltaTime, _layerMask, QueryTriggerInteraction.Ignore))
        {
            // We remember the last impact speed because the fall damage logic might need it
            m_LatestImpactSpeed = characterVelocity;

            characterVelocity = Vector3.ProjectOnPlane(characterVelocity, hit.normal);
        }
    }

    // Returns true if the slope angle represented by the given normal is under the slope angle limit of the character controller
    bool IsNormalUnderSlopeLimit(Vector3 normal)
    {
        return Vector3.Angle(transform.up, normal) <= m_Controller.slopeLimit;
    }

    // Gets the center point of the bottom hemisphere of the character controller capsule    
    Vector3 GetCapsuleBottomHemisphere()
    {
        return transform.position + (transform.up * m_Controller.radius);
    }

    // Gets the center point of the top hemisphere of the character controller capsule    
    Vector3 GetCapsuleTopHemisphere(float atHeight)
    {
        return transform.position + (transform.up * (atHeight - m_Controller.radius));
    }

    // Gets a reoriented direction that is tangent to a given slope
    public Vector3 GetDirectionReorientedOnSlope(Vector3 direction, Vector3 slopeNormal)
    {
        Vector3 directionRight = Vector3.Cross(direction, transform.up);
        return Vector3.Cross(slopeNormal, directionRight).normalized;
    }

    void UpdateCharacterHeight(bool force)
    {
        // Update height instantly
        if (force)
        {
            m_Controller.height = m_TargetCharacterHeight;
            m_Controller.center = Vector3.up * m_Controller.height * 0.5f;
            playerCamera.transform.localPosition = Vector3.up * m_TargetCharacterHeight * cameraHeightRatio;
        }
        // Update smooth height
        else if (m_Controller.height != m_TargetCharacterHeight)
        {
            // resize the capsule and adjust camera position
            m_Controller.height = Mathf.Lerp(m_Controller.height, m_TargetCharacterHeight, crouchingSharpness * Time.deltaTime);
            m_Controller.center = Vector3.up * m_Controller.height * 0.5f;
            playerCamera.transform.localPosition = Vector3.Lerp(playerCamera.transform.localPosition, Vector3.up * m_TargetCharacterHeight * cameraHeightRatio, crouchingSharpness * Time.deltaTime);
        }
    }

    // returns false if there was an obstruction
    bool SetCrouchingState(bool crouched, bool ignoreObstructions)
    {
        // set appropriate heights
        if (crouched)
        {
            m_TargetCharacterHeight = capsuleHeightCrouching;
        }
        else
        {
            // Detect obstructions
            if (!ignoreObstructions)
            {
                Collider[] standingOverlaps = Physics.OverlapCapsule(
                    GetCapsuleBottomHemisphere(),
                    GetCapsuleTopHemisphere(capsuleHeightStanding),
                    m_Controller.radius,
                    _layerMask,
                    QueryTriggerInteraction.Ignore);
                foreach (Collider c in standingOverlaps)
                {
                    if (c != m_Controller)
                    {
                        return false;
                    }
                }
            }

            m_TargetCharacterHeight = capsuleHeightStanding;
        }

        if (onStanceChanged != null)
        {
            onStanceChanged.Invoke(crouched);
        }

        isCrouching = crouched;
        return true;
    }
}
