
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

[UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
public class PXR_UnifiedMenu : UdonSharpBehaviour
{
    // =====================================
    // ADMIN SETTINGS
    // =====================================

    [Header("Administrator Access")]
    public bool adminOnly = false;

    public PXR_AdminNames adminList;

    // =====================================
    // HAND SETTINGS
    // =====================================

    [Header("Hand Settings")]

    [Tooltip(
        "True = right hand and right thumbstick. " +
        "False = left hand and left thumbstick."
    )]
    public bool rightHand = true;

    // =====================================
    // MENU POSITIONING
    // =====================================

    [Header("Menu Positioning")]

    [Tooltip(
        "Position relative to the hand's horizontal " +
        "direction. Z extends forward from the hand. " +
        "A zero vector uses the Launchpad default."
    )]
    public Vector3 offset =
        new Vector3(0f, 0.10f, 0.24f);

    [Tooltip(
        "True: panel faces the player. " +
        "False: panel follows hand yaw while " +
        "remaining upright."
    )]
    public bool facePlayer = true;

    [Tooltip(
        "Additional panel rotation in degrees. " +
        "Applied after the upright orientation."
    )]
    public Vector3 rotationOffset =
        new Vector3(15f, 0f, 0f);

    [Tooltip(
        "Additional horizontal direction adjustment " +
        "for controller calibration, in degrees."
    )]
    public float handYawOffset = 0f;

    // =====================================
    // MENU CONTENT
    // =====================================

    [Header("Menu Content")]

    public GameObject menuContent;

    // =====================================
    // THUMBSTICK INPUT
    // =====================================

    [Header("VR Thumbstick Input")]

    public string leftThumbstickButton =
        "Oculus_CrossPlatform_PrimaryThumbstick";

    public string rightThumbstickButton =
        "Oculus_CrossPlatform_SecondaryThumbstick";

    // =====================================
    // DESKTOP INPUT
    // =====================================

    [Header("Desktop Input")]

    public bool enableDesktopFallback = true;

    public KeyCode desktopToggleKey = KeyCode.M;

    public Vector3 desktopOffset =
        new Vector3(0f, -0.15f, 0.55f);

    // =====================================
    // RUNTIME
    // =====================================

    private VRCPlayerApi localPlayer;

    private bool initialized;
    private bool menuOpen;

    private bool thumbstickWasPressed;

    // VR mode cannot change during a session, so it is read once.
    private bool vrChecked;
    private bool userInVR;
    private float startTime;

    // Access is re-checked about once per second instead of every
    // frame. ShowMenu() still checks access live on every open.
    private bool cachedAccess;
    private float nextAccessCheck;

    // =====================================
    // INITIALIZATION
    // =====================================

    private void Start()
    {
        DisableInteractive = true;

        startTime = Time.time;
        localPlayer = Networking.LocalPlayer;

        if (!Utilities.IsValid(menuContent) ||
            menuContent == gameObject)
        {
            Debug.LogWarning(
                "[PXR_UnifiedMenu] Assign a child " +
                "GameObject as Menu Content."
            );

            return;
        }

        menuContent.SetActive(false);

        menuOpen = false;
        thumbstickWasPressed = false;

        if (adminOnly &&
            !Utilities.IsValid(adminList))
        {
            Debug.LogWarning(
                "[PXR_UnifiedMenu] Admin Only is " +
                "enabled, but no PXR_AdminNames " +
                "component is assigned. " +
                "Menu access will be denied."
            );
        }

        initialized = true;

        UpdateHandTransform();
    }

    // =====================================
    // ADMIN ACCESS
    // =====================================

    public bool HasMenuAccess()
    {
        if (!adminOnly)
            return true;

        if (!Utilities.IsValid(adminList))
            return false;

        return adminList.IsLocalPlayerAdmin();
    }

    // =====================================
    // THUMBSTICK INPUT
    // =====================================

    private void CheckThumbstickInput()
    {
        // The activation hand matches
        // the menu's assigned hand.

        string buttonName = rightHand
            ? rightThumbstickButton
            : leftThumbstickButton;

        if (string.IsNullOrEmpty(buttonName))
        {
            thumbstickWasPressed = false;
            return;
        }

        bool pressed = Input.GetButton(buttonName);

        bool newlyPressed =
            pressed && !thumbstickWasPressed;

        thumbstickWasPressed = pressed;

        if (newlyPressed)
        {
            ToggleMenu();
        }
    }

    // =====================================
    // UPDATE
    // =====================================

    private void Update()
    {
        if (!initialized)
            return;

        if (!Utilities.IsValid(localPlayer))
        {
            localPlayer = Networking.LocalPlayer;
            return;
        }

        if (menuOpen)
        {
            if (Time.time >= nextAccessCheck)
            {
                cachedAccess = HasMenuAccess();
                nextAccessCheck = Time.time + 1f;
            }

            if (!cachedAccess)
                HideMenu();
        }

        if (IsVR())
        {
            CheckThumbstickInput();
        } else
        {
            thumbstickWasPressed = false;

            if (enableDesktopFallback &&
                Input.GetKeyDown(desktopToggleKey))
            {
                ToggleMenu();
            }
        }
    }

    // =====================================
    // MENU TRACKING
    // =====================================

    public override void PostLateUpdate()
    {
        if (initialized && menuOpen)
        {
            UpdateHandTransform();
        }
    }

    // =====================================
    // LAUNCHPAD OFFSET
    // =====================================

    private Vector3 GetEffectiveOffset()
    {
        // Existing Unity components may retain
        // a serialized zero offset from the
        // older wrist-mounted implementation.
        //
        // Use a Launchpad-style position
        // in that case.

        if (offset.sqrMagnitude < 0.000001f)
        {
            return new Vector3(
                0f,
                0.10f,
                0.24f
            );
        }

        return offset;
    }

    // =====================================
    // HAND HORIZONTAL ROTATION
    // =====================================

    private Quaternion GetHandYawRotation(
        Quaternion handRotation,
        Quaternion headRotation)
    {
        // Obtain the hand's forward direction.

        Vector3 forward =
            handRotation * Vector3.forward;

        // Ignore controller pitch and roll.
        // The menu remains upright instead
        // of lying against the wrist.

        forward.y = 0f;

        // Handle controllers pointing almost
        // directly up or down.

        if (forward.sqrMagnitude < 0.0001f)
        {
            forward =
                headRotation * Vector3.forward;

            forward.y = 0f;
        }

        if (forward.sqrMagnitude < 0.0001f)
        {
            forward = Vector3.forward;
        }

        Quaternion yawRotation =
            Quaternion.LookRotation(
                forward.normalized,
                Vector3.up
            );

        // Optional controller alignment.

        return yawRotation *
            Quaternion.Euler(
                0f,
                handYawOffset,
                0f
            );
    }

    // =====================================
    // VIEWER-FACING ROTATION
    // =====================================

    private Quaternion GetBookRotation(
        Vector3 menuPosition,
        Vector3 headPosition,
        Quaternion headRotation)
    {
        // The Canvas's local +Z direction
        // should face away from the viewer.

        Vector3 awayFromHead =
            menuPosition - headPosition;

        // Stay upright.

        awayFromHead.y = 0f;

        if (awayFromHead.sqrMagnitude < 0.0001f)
        {
            awayFromHead =
                headRotation * Vector3.forward;

            awayFromHead.y = 0f;
        }

        if (awayFromHead.sqrMagnitude < 0.0001f)
        {
            awayFromHead = Vector3.forward;
        }

        return Quaternion.LookRotation(
            awayFromHead.normalized,
            Vector3.up
        );
    }

    // =====================================
    // POSITION AND ORIENTATION
    // =====================================

    private void UpdateHandTransform()
    {
        if (!Utilities.IsValid(localPlayer))
            return;

        VRCPlayerApi.TrackingData headData =
            localPlayer.GetTrackingData(
                VRCPlayerApi.TrackingDataType.Head
            );

        Vector3 position;
        Quaternion rotation;

        if (IsVR())
        {
            VRCPlayerApi.TrackingDataType handType =
                rightHand
                    ? VRCPlayerApi.TrackingDataType.RightHand
                    : VRCPlayerApi.TrackingDataType.LeftHand;

            VRCPlayerApi.TrackingData handData =
                localPlayer.GetTrackingData(handType);

            // Establish a yaw-only hand frame.
            // This is shared by both modes.

            Quaternion handYaw =
                GetHandYawRotation(
                    handData.rotation,
                    headData.rotation
                );

            // Move the panel outward from
            // the hand, not onto the wrist.
            //
            // Controller pitch and roll do
            // not change this outward offset.

            position =
                handData.position +
                handYaw * GetEffectiveOffset();

            if (facePlayer)
            {
                // Face the player while
                // keeping the menu upright.

                rotation = GetBookRotation(
                    position,
                    headData.position,
                    headData.rotation
                );
            } else
            {
                // Launchpad-style hand-relative
                // orientation.
                //
                // Follow hand yaw, not wrist
                // pitch or roll.

                rotation = handYaw;
            }
        } else
        {
            // Desktop fallback.

            position =
                headData.position +
                headData.rotation * desktopOffset;

            if (facePlayer)
            {
                rotation = GetBookRotation(
                    position,
                    headData.position,
                    headData.rotation
                );
            } else
            {
                // Upright view-relative panel
                // without tracking head tilt.

                Vector3 forward =
                    headData.rotation * Vector3.forward;

                forward.y = 0f;

                if (forward.sqrMagnitude < 0.0001f)
                {
                    forward = Vector3.forward;
                }

                rotation = Quaternion.LookRotation(
                    forward.normalized,
                    Vector3.up
                );
            }
        }

        // Fine adjustment applied after
        // the primary orientation.

        rotation =
            rotation *
            Quaternion.Euler(rotationOffset);

        transform.SetPositionAndRotation(
            position,
            rotation
        );
    }

    // =====================================
    // MENU CONTROLS
    // =====================================

    public void ToggleMenu()
    {
        if (!initialized)
            return;

        if (menuOpen)
        {
            HideMenu();
        } else
        {
            ShowMenu();
        }
    }

    public void ShowMenu()
    {
        if (!initialized)
            return;

        if (!HasMenuAccess())
            return;

        // Access was just confirmed; the periodic re-check in
        // Update() resumes a second from now.
        cachedAccess = true;
        nextAccessCheck = Time.time + 1f;

        UpdateHandTransform();

        menuOpen = true;
        menuContent.SetActive(true);
    }

    public void HideMenu()
    {
        if (!initialized)
            return;

        menuOpen = false;
        menuContent.SetActive(false);
    }

    public bool IsMenuOpen()
    {
        return menuOpen;
    }

    private bool IsVR()
    {
        if (vrChecked)
            return userInVR;

        if (!Utilities.IsValid(localPlayer))
            return false;

        userInVR = localPlayer.IsUserInVR();

        // Keep reading live during the first seconds after load,
        // then lock the value in.
        vrChecked = Time.time - startTime > 2f;

        return userInVR;
    }
}
