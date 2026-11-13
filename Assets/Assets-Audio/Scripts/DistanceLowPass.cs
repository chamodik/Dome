using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(AudioLowPassFilter))]
[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class DistanceLowPass : UdonSharpBehaviour
{
    [Header("Distance Settings")]
    [Tooltip("Distance at which the cutoff is at its highest (clearest sound).")]
    public float minDistance = 2f;

    [Tooltip("Distance at which the cutoff reaches its lowest (most muffled sound).")]
    public float maxDistance = 20f;

    [Header("Frequency Settings (Hz)")]
    [Tooltip("Cutoff frequency when player is close (22000 Hz = full, clear audio).")]
    [Range(10f, 22000f)]
    public float closeCutoffFrequency = 22000f;

    [Tooltip("Cutoff frequency when player is at or beyond maxDistance (lower = more muffled).")]
    [Range(10f, 22000f)]
    public float farCutoffFrequency = 500f;

    [Header("Curve Control")]
    [Tooltip("Controls how smoothly the frequency changes over distance.")]
    public AnimationCurve falloffCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    private AudioLowPassFilter _lowPassFilter;
    private VRCPlayerApi _localPlayer;

    private void Start()
    {
        _lowPassFilter = GetComponent<AudioLowPassFilter>();
        _localPlayer = Networking.LocalPlayer;
    }

    private void Update()
    {
        if (!Utilities.IsValid(_localPlayer)) return;

        // Track the local player's head position
        Vector3 headPosition = _localPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Head).position;

        // Calculate distance and normalize it between 0 (close) and 1 (far)
        float distance = Vector3.Distance(transform.position, headPosition);
        float normalizedDistance = Mathf.InverseLerp(minDistance, maxDistance, distance);

        // Apply the curve and update the low-pass filter
        float curvedValue = falloffCurve.Evaluate(normalizedDistance);
        _lowPassFilter.cutoffFrequency = Mathf.Lerp(closeCutoffFrequency, farCutoffFrequency, curvedValue);
    }
}
