using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class WorldFootsteps : UdonSharpBehaviour
{
    [Header("Audio Pool")]
    public AudioSource[] audioPool;
    [Range(0f, 1f)] public float stepVolume = 0.6f;

    [Header("Surface Materials")]
    public PhysicMaterial woodMaterial;
    public PhysicMaterial grassMaterial;
    public PhysicMaterial metalMaterial;

    [Header("Surface Audio Clips")]
    public AudioClip[] defaultClips;
    public AudioClip[] woodClips;
    public AudioClip[] grassClips;
    public AudioClip[] metalClips;

    [Header("Step & Raycast Settings")]
    public float baseStepDistance = 1.6f;
    public float minMoveSpeed = 0.4f;
    public LayerMask groundLayers = -1;

    private VRCPlayerApi[] players = new VRCPlayerApi[100];
    private Vector3[] lastPositions = new Vector3[100];
    private float[] distanceAccumulated = new float[100];
    private int poolIndex = 0;

    void Update()
    {
        if (audioPool == null || audioPool.Length == 0) return;

        int playerCount = VRCPlayerApi.GetPlayerCount();
        VRCPlayerApi.GetPlayers(players);

        for (int i = 0; i < playerCount; i++)
        {
            VRCPlayerApi player = players[i];
            if (!Utilities.IsValid(player)) continue;

            int id = player.playerId;
            if (id >= lastPositions.Length) continue;

            Vector3 currentPos = player.GetPosition();

            if (lastPositions[id] == Vector3.zero)
            {
                lastPositions[id] = currentPos;
                continue;
            }

            Vector3 flatCurrent = new Vector3(currentPos.x, 0f, currentPos.z);
            Vector3 flatLast = new Vector3(lastPositions[id].x, 0f, lastPositions[id].z);
            float moved = Vector3.Distance(flatCurrent, flatLast);
            float speed = moved / Time.deltaTime;

            if (player.IsPlayerGrounded() && speed > minMoveSpeed)
            {
                distanceAccumulated[id] += moved;

                if (distanceAccumulated[id] >= baseStepDistance)
                {
                    distanceAccumulated[id] = 0f;
                    PlaySurfaceStep(currentPos);
                }
            }
            else
            {
                distanceAccumulated[id] = 0f;
            }

            lastPositions[id] = currentPos;
        }
    }

    private void PlaySurfaceStep(Vector3 footPos)
    {
        AudioClip[] selectedClips = defaultClips;

        RaycastHit hit;
        if (Physics.Raycast(footPos + Vector3.up * 0.25f, Vector3.down, out hit, 0.6f, groundLayers, QueryTriggerInteraction.Ignore))
        {
            PhysicMaterial hitMat = hit.collider.sharedMaterial;

            if (hitMat != null)
            {
                if (hitMat == woodMaterial && woodClips != null && woodClips.Length > 0)
                {
                    selectedClips = woodClips;
                }
                else if (hitMat == grassMaterial && grassClips != null && grassClips.Length > 0)
                {
                    selectedClips = grassClips;
                }
                else if (hitMat == metalMaterial && metalClips != null && metalClips.Length > 0)
                {
                    selectedClips = metalClips;
                }
            }
        }

        if (selectedClips == null || selectedClips.Length == 0) return;

        AudioSource source = audioPool[poolIndex];
        poolIndex = (poolIndex + 1) % audioPool.Length;

        source.transform.position = footPos;
        source.pitch = Random.Range(0.92f, 1.08f);
        source.PlayOneShot(selectedClips[Random.Range(0, selectedClips.Length)], stepVolume);
    }
}
