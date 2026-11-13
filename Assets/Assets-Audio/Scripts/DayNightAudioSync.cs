using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class DayNightAudioSync : UdonSharpBehaviour
{
    [Header("Sun Reference")]
    [Tooltip("Assign the rotating Sun Directional Light Transform.")]
    public Transform sunTransform;

    [Tooltip("Check this if Day sounds play at Night and vice versa.")]
    public bool invertSunDirection = false;

    [Header("Horizon & Twilight Band")]
    [Tooltip("Center of the horizon (-sun.forward.y). 0 = exact geometric horizon.")]
    [Range(-0.3f, 0.3f)]
    public float horizonCenter = 0.0f;

    [Tooltip("Width of the twilight crossfade zone around the horizon.")]
    [Range(0.01f, 0.5f)]
    public float twilightWidth = 0.12f;

    [Tooltip("Extra smoothing time (seconds) for smooth transitions when scrubbing time.")]
    public float smoothingTime = 1.5f;

    [Header("Voice Virtualization")]
    [Tooltip("Priority assigned when volume is 0 so Unity forces these sources into Virtual Voices.")]
    [Range(0, 256)]
    public int virtualizedPriority = 256;

    [Header("Day Audio Groups (Audible Above Horizon)")]
    public AudioSource[] daybirdsSources;
    public AudioSource[] cicadasSources;

    [Header("Night Audio Groups (Audible Below Horizon)")]
    public AudioSource[] nightbirdsSources;
    public AudioSource[] cricketsSources;

    [Header("Live ClientSim Debug (Read-Only in Play Mode)")]
    public float currentSunElevation;
    public float currentDayWeight;
    public float currentNightWeight;
    public bool dayGroupVirtualized;
    public bool nightGroupVirtualized;

    private float[] _daybirdsBaseVols;
    private float[] _cicadasBaseVols;
    private float[] _nightbirdsBaseVols;
    private float[] _cricketsBaseVols;
    private int[] _daybirdsBasePriorities;
    private int[] _cicadasBasePriorities;
    private int[] _nightbirdsBasePriorities;
    private int[] _cricketsBasePriorities;

    private float _smoothedDayLinear = 0f;

    void Start()
    {
        _daybirdsBaseVols = CacheAndStartSources(daybirdsSources);
        _cicadasBaseVols = CacheAndStartSources(cicadasSources);
        _nightbirdsBaseVols = CacheAndStartSources(nightbirdsSources);
        _cricketsBaseVols = CacheAndStartSources(cricketsSources);
        _daybirdsBasePriorities = CacheBasePriorities(daybirdsSources);
        _cicadasBasePriorities = CacheBasePriorities(cicadasSources);
        _nightbirdsBasePriorities = CacheBasePriorities(nightbirdsSources);
        _cricketsBasePriorities = CacheBasePriorities(cricketsSources);

        _smoothedDayLinear = CalculateTargetDayLinear();
        ApplyAllGroups(_smoothedDayLinear);
    }

    void Update()
    {
        if (sunTransform == null) return;

        float targetDayLinear = CalculateTargetDayLinear();

        if (smoothingTime > 0.001f)
        {
            float maxStep = Time.deltaTime / smoothingTime;
            _smoothedDayLinear = Mathf.MoveTowards(_smoothedDayLinear, targetDayLinear, maxStep);
        }
        else
        {
            _smoothedDayLinear = targetDayLinear;
        }

        ApplyAllGroups(_smoothedDayLinear);
    }

    private float CalculateTargetDayLinear()
    {
        if (sunTransform == null) return 1f;

        float rawElevation = invertSunDirection ? sunTransform.forward.y : -sunTransform.forward.y;
        currentSunElevation = rawElevation;

        float minElev = horizonCenter - twilightWidth;
        float maxElev = horizonCenter + twilightWidth;

        return Mathf.Clamp01((rawElevation - minElev) / (maxElev - minElev));
    }

    private void ApplyAllGroups(float linearDay)
    {
        float smoothDay = Mathf.SmoothStep(0f, 1f, linearDay);
        float smoothNight = 1f - smoothDay;

        currentDayWeight = smoothDay * smoothDay;
        currentNightWeight = smoothNight * smoothNight;

        dayGroupVirtualized = (currentDayWeight <= 0.0001f);
        nightGroupVirtualized = (currentNightWeight <= 0.0001f);

        ApplyGroupState(daybirdsSources, _daybirdsBaseVols, _daybirdsBasePriorities, currentDayWeight, dayGroupVirtualized);
        ApplyGroupState(cicadasSources, _cicadasBaseVols, _cicadasBasePriorities, currentDayWeight, dayGroupVirtualized);
        ApplyGroupState(nightbirdsSources, _nightbirdsBaseVols, _nightbirdsBasePriorities, currentNightWeight, nightGroupVirtualized);
        ApplyGroupState(cricketsSources, _cricketsBaseVols, _cricketsBasePriorities, currentNightWeight, nightGroupVirtualized);
    }

    private float[] CacheAndStartSources(AudioSource[] sources)
    {
        if (sources == null) return new float[0];
        float[] vols = new float[sources.Length];
        for (int i = 0; i < sources.Length; i++)
        {
            AudioSource src = sources[i];
            if (src != null)
            {
                vols[i] = (src.volume > 0.001f) ? src.volume : 1f;
                // Ensure the source is playing continuously so Unity manages Real <-> Virtual transitions automatically
                if (!src.isPlaying && src.clip != null)
                {
                    src.Play();
                }
            }
        }
        return vols;
    }

    private int[] CacheBasePriorities(AudioSource[] sources)
    {
        if (sources == null) return new int[0];
        int[] priorities = new int[sources.Length];
        for (int i = 0; i < sources.Length; i++)
        {
            if (sources[i] != null)
            {
                priorities[i] = sources[i].priority;
            }
        }
        return priorities;
    }

    private void ApplyGroupState(AudioSource[] sources, float[] baseVols, int[] basePriorities, float weight, bool isVirtualized)
    {
        if (sources == null || baseVols == null || basePriorities == null) return;

        for (int i = 0; i < sources.Length; i++)
        {
            AudioSource src = sources[i];
            if (src == null) continue;

            src.volume = baseVols[i] * weight;

            int targetPriority = isVirtualized ? virtualizedPriority : basePriorities[i];
            if (src.priority != targetPriority)
            {
                src.priority = targetPriority;
            }
        }
    }
}