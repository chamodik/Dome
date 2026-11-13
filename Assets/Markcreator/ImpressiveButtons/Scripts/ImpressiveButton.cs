using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace Markcreator.ImpressiveButtons
{
    /// <summary>
    /// Impressive physically interactable buttons in VRChat.
    /// </summary>
    ///-----------------------------------------------------------------------
    /// <copyright>
    ///     Copyright (c) Markcreator. All rights reserved.
    /// </copyright>
    /// <author>Markcreator</author>
    ///-----------------------------------------------------------------------
    [AddComponentMenu("Markcreator/Impressive Buttons/Impressive Button"), UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class ImpressiveButton : UdonSharpBehaviour
    {
        bool isEnabled = false; // If the button in enabled
        bool inEditor = false; // If the world is being tested in the Unity Editor
        bool inVr = false; // If player is in VR cache
        bool pressed = false; // If button has been pressed
        bool vrPressing = false; // If a bone is touching the button
        HumanBodyBones usedBone = HumanBodyBones.LeftHand; // Which bone is touching the button
        bool interactPressing = false; // If the button has been pressed with VRChat's classic Interact system
        Collider desktopCollider = null; // VRChat's classic Interact collider
        Collider sliderCollider = null; // Button collider for bones
        AudioSource audioSource = null; // Audio source

        [SerializeField, Tooltip("The target script that should receive _OnPress() and _OnPress_'gameobject name'() when the button is pressed")]
        UdonBehaviour sendOnPressEventTo = null;

        [SerializeField, Tooltip("If VRChat's regular Interact system should be enabled for users in VR")]
        bool enableClassicInteractInVR = true;
        [SerializeField, Tooltip("Sound when pressed")]
        AudioClip onPressSound = null;
        [SerializeField, Tooltip("Sound when depressed")]
        AudioClip onDepressSound = null;
        [SerializeField, Tooltip("Empty gameobject at highest point of the button, should be the parent of the slider")]
        GameObject buttonTop = null;
        [SerializeField, Tooltip("The moving part of the button. Needs a collider")]
        GameObject buttonSlider = null;
        [SerializeField, Tooltip("The highest point for the button top")]
        float buttonTopHighPoint = 1f;
        [SerializeField, Tooltip("The bottom-out height for the button top")]
        float buttonTopLowPoint = 0f;
        [SerializeField, Tooltip("Button movement speed")]
        float buttonSpeed = 1f;

        //[SerializeField, Tooltip("If button presses should be sent to all connected players (Watch out for desynchronization/when people join late)")]
        //bool globalEvents = false;
        [SerializeField, Tooltip("If the button should only be visible to the world master")]
        bool masterOnly = false;
        [SerializeField, Tooltip("If players should be able to press buttons with their feet")]
        bool feetPressing = false;
        [SerializeField, Tooltip("Custom event name for the target script")]
        string customEventName = "_OnPress";

        void Start()
        {
            inEditor = Networking.LocalPlayer == null;
            inVr = Networking.LocalPlayer != null && Networking.LocalPlayer.IsUserInVR();
            desktopCollider = GetComponent<Collider>();
            if (desktopCollider == null) Debug.LogError("ERROR: Button '" + name + "' needs a collider.");
            if (buttonTop == null) Debug.LogError("ERROR: Button '" + name + "' needs a top.");
            if (buttonSlider == null) Debug.LogError("ERROR: Button '" + name + "' needs a slider."); else if (sliderCollider = buttonSlider.GetComponent<Collider>()){}; // Force error in Editor
            audioSource = GetComponentInParent<AudioSource>();

            if (inEditor || !masterOnly || (masterOnly && Networking.LocalPlayer.isMaster)) _ShowButton(); else _HideButton();
        }

        public void _ShowButton()
        {
            isEnabled = true;
            // Disable VRChat's classic Interact system when in VR
            desktopCollider.enabled = (inVr && enableClassicInteractInVR) || !inVr;
            // Show all children
            GameObject target = gameObject.name.Contains("Setting") && gameObject.transform.parent ? gameObject.transform.parent.gameObject : gameObject;
            foreach (MeshRenderer child in target.GetComponentsInChildren<MeshRenderer>()) if (child) child.enabled = true;
        }

        public void _HideButton()
        {
            isEnabled = false;
            desktopCollider.enabled = false;
            GameObject target = gameObject.name.Contains("Setting") && gameObject.transform.parent ? gameObject.transform.parent.gameObject : gameObject;
            foreach (MeshRenderer child in target.GetComponentsInChildren<MeshRenderer>()) if (child) child.enabled = false;
        }

        private void FixedUpdate()
        {
            inVr = Networking.LocalPlayer != null && Networking.LocalPlayer.IsUserInVR();
            if (isEnabled) desktopCollider.enabled = (inVr && enableClassicInteractInVR) || !inVr;
        }

        float boundsSpeed = 1f / 60; // 60Hz
        float lastBoundsCheck = 0;
        void Update()
        {
            if (!isEnabled) return; // No expensive button behavior if disabled

            // Check bounds every boundsSpeed seconds
            lastBoundsCheck += Time.deltaTime;
            if (lastBoundsCheck > boundsSpeed)
            {
                lastBoundsCheck -= boundsSpeed;

                if (inVr)
                {
                    vrPressing = sliderCollider.bounds.Contains(Networking.LocalPlayer.GetBonePosition(usedBone = HumanBodyBones.LeftHand)) ||
                                 sliderCollider.bounds.Contains(Networking.LocalPlayer.GetBonePosition(usedBone = HumanBodyBones.RightHand)) ||
                                 sliderCollider.bounds.Contains(Networking.LocalPlayer.GetBonePosition(usedBone = HumanBodyBones.LeftIndexDistal)) ||
                                 sliderCollider.bounds.Contains(Networking.LocalPlayer.GetBonePosition(usedBone = HumanBodyBones.RightIndexDistal)); // This could be faster

                    if(!vrPressing && feetPressing)
                    {
                        vrPressing = sliderCollider.bounds.Contains(Networking.LocalPlayer.GetBonePosition(usedBone = HumanBodyBones.LeftFoot)) ||
                                     sliderCollider.bounds.Contains(Networking.LocalPlayer.GetBonePosition(usedBone = HumanBodyBones.RightFoot));
                    }
                }
            }

            // Update button height every frame if latest bounds check was successful
            if (vrPressing)
            {
                Vector3 bonePos = Networking.LocalPlayer.GetBonePosition(usedBone);
                float height = transform.InverseTransformPoint(bonePos).y;
                buttonTop.transform.localPosition = new Vector3(0, height, 0);
            }

            // Permanent upwards force except when VRChat's classic Interact happened
            if (interactPressing)
            {
                buttonTop.transform.localPosition = buttonTop.transform.localPosition + Vector3.down * Time.fixedDeltaTime * buttonSpeed * 2;
            }
            else
            {
                if (buttonTop.transform.localPosition.y < buttonTopHighPoint) // Prevent busy position updating
                {
                    buttonTop.transform.localPosition = buttonTop.transform.localPosition + Vector3.up * Time.fixedDeltaTime * buttonSpeed;
                }
            }

            // Enforce button range
            if (buttonTop.transform.localPosition.y <= buttonTopLowPoint)
            {
                if (buttonTop.transform.localPosition.y < buttonTopLowPoint) buttonTop.transform.localPosition = new Vector3(0, buttonTopLowPoint, 0);
                interactPressing = false;
                if (!pressed)
                {
                    // Button pressed
                    pressed = true;
                    if (sendOnPressEventTo != null) {
                        GameObject target = gameObject.name.Contains("Setting") && gameObject.transform.parent ? gameObject.transform.parent.gameObject : gameObject;

                        sendOnPressEventTo.SendCustomEvent(customEventName);
                        sendOnPressEventTo.SendCustomEvent(customEventName + "_" + target.name);
                    }
                    _PlayPress();
                }
            }
            if (buttonTop.transform.localPosition.y >= buttonTopHighPoint)
            {
                if (buttonTop.transform.localPosition.y > buttonTopHighPoint) buttonTop.transform.localPosition = new Vector3(0, buttonTopHighPoint, 0);
                if (pressed)
                {
                    pressed = false;
                    _PlayDepress();
                }
            }
        }

        public void _PlayPress()
        {
            if (onPressSound != null && audioSource) audioSource.PlayOneShot(onPressSound);
        }

        public void _PlayDepress()
        {
            if (onDepressSound != null && audioSource) audioSource.PlayOneShot(onDepressSound);
        }

        public override void Interact()
        {
            interactPressing = true;
        }

        public override void OnPlayerLeft(VRCPlayerApi player)
        {
            if (!isEnabled && masterOnly && Networking.LocalPlayer.isMaster) _ShowButton();
        }

        public bool _GetClassicInteract()
        {
            return enableClassicInteractInVR;
        }

        public void _SetClassicInteract(bool enable)
        {
            enableClassicInteractInVR = enable;
            if (isEnabled) desktopCollider.enabled = (inVr && enableClassicInteractInVR) || !inVr;
        }
    }

/*#if UNITY_EDITOR && !COMPILER_UDONSHARP
    [CustomEditor(typeof(ImpressiveButton)), CanEditMultipleObjects]
    public class ImpressiveButtonEditor : Editor
    {
        static readonly string[] advancedSettingNames = { "masterOnly", "feetPressing", "customEventName" };
        static readonly string customEventNameDefault = "_OnPress";

        ImpressiveButton o;
        SerializedObject so;
        bool showAdvancedSettings = false;

        void OnEnable()
        {
            o = target as ImpressiveButton;
            so = serializedObject;
        }

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            so.Update();

            EditorGUILayout.Space();

            if (showAdvancedSettings = EditorGUILayout.Foldout(showAdvancedSettings, "Advanced Settings", true))
            {
                foreach (string advancedSetting in advancedSettingNames) EditorGUILayout.PropertyField(so.FindProperty(advancedSetting));

                if (GUILayout.Button("Reset event name")) so.FindProperty("customEventName").stringValue = customEventNameDefault;
            }

            so.ApplyModifiedProperties();
        }
    }
#endif*/
}
