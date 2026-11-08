
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

[UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
public class PXR_AdminLock : UdonSharpBehaviour
{
    // =====================================
    // ADMIN CONFIGURATION
    // =====================================

    [Header("Admin List")]
    public PXR_AdminNames adminList;

    // =====================================
    // PROTECTED OBJECTS
    // =====================================

    [Header("Protected Objects")]
    public GameObject[] protectedObjects =
        new GameObject[0];

    // Legacy single-object reference.
    //
    // Used only when the new array is empty.
    // Existing scene references can therefore
    // be migrated without being lost.

    [HideInInspector]
    public GameObject protectedContent;

    // =====================================
    // CACHED COMPONENTS
    // =====================================

    private Renderer[] hiddenRenderers =
        new Renderer[0];

    private Collider[] hiddenColliders =
        new Collider[0];

    private Canvas[] hiddenCanvases =
        new Canvas[0];

    private Light[] hiddenLights =
        new Light[0];

    private bool restricted;

    // =====================================
    // INITIALIZATION
    // =====================================

    private void Start()
    {
        DisableInteractive = true;

        // Fail closed if no admin list is
        // assigned, rather than making
        // protected content visible to everyone.

        if (!Utilities.IsValid(adminList))
        {
            Debug.LogWarning(
                "[PXR_AdminLock] No PXR_AdminNames " +
                "component assigned on " +
                gameObject.name
            );
        } else if (adminList.IsLocalPlayerAdmin())
        {
            // Administrators retain the
            // scene's original component states.

            restricted = false;
            return;
        }

        restricted = true;

        CacheProtectedComponents();
        ApplyRestrictions();
    }

    // =====================================
    // BUILD PROTECTED COMPONENT LIST
    // =====================================

    private void CacheProtectedComponents()
    {
        // Prefer the new multi-object array.

        if (protectedObjects != null &&
            protectedObjects.Length > 0)
        {
            for (int i = 0;
                 i < protectedObjects.Length;
                 i++)
            {
                GameObject target =
                    protectedObjects[i];

                if (!Utilities.IsValid(target))
                    continue;

                CacheTarget(target);
            }
        } else
        {
            // Backward compatibility with
            // the previous single target.

            if (Utilities.IsValid(protectedContent))
            {
                CacheTarget(protectedContent);
            }
        }
    }

    private void CacheTarget(GameObject target)
    {
        // Find all visual and interactive
        // components beneath the target.
        //
        // includeInactive=true ensures
        // initially inactive children
        // are included in the cache.

        Renderer[] renderers =
            target.GetComponentsInChildren<Renderer>(
                true
            );

        Collider[] colliders =
            target.GetComponentsInChildren<Collider>(
                true
            );

        Canvas[] canvases =
            target.GetComponentsInChildren<Canvas>(
                true
            );

        Light[] lights =
            target.GetComponentsInChildren<Light>(
                true
            );

        // =================================
        // APPEND RENDERERS
        // =================================

        int oldRendererCount =
            hiddenRenderers.Length;

        Renderer[] combinedRenderers =
            new Renderer[
                oldRendererCount + renderers.Length
            ];

        for (int i = 0;
             i < oldRendererCount;
             i++)
        {
            combinedRenderers[i] =
                hiddenRenderers[i];
        }

        for (int i = 0;
             i < renderers.Length;
             i++)
        {
            combinedRenderers[
                oldRendererCount + i
            ] = renderers[i];
        }

        hiddenRenderers = combinedRenderers;

        // =================================
        // APPEND COLLIDERS
        // =================================

        int oldColliderCount =
            hiddenColliders.Length;

        Collider[] combinedColliders =
            new Collider[
                oldColliderCount + colliders.Length
            ];

        for (int i = 0;
             i < oldColliderCount;
             i++)
        {
            combinedColliders[i] =
                hiddenColliders[i];
        }

        for (int i = 0;
             i < colliders.Length;
             i++)
        {
            combinedColliders[
                oldColliderCount + i
            ] = colliders[i];
        }

        hiddenColliders = combinedColliders;

        // =================================
        // APPEND CANVASES
        // =================================

        int oldCanvasCount =
            hiddenCanvases.Length;

        Canvas[] combinedCanvases =
            new Canvas[
                oldCanvasCount + canvases.Length
            ];

        for (int i = 0;
             i < oldCanvasCount;
             i++)
        {
            combinedCanvases[i] =
                hiddenCanvases[i];
        }

        for (int i = 0;
             i < canvases.Length;
             i++)
        {
            combinedCanvases[
                oldCanvasCount + i
            ] = canvases[i];
        }

        hiddenCanvases = combinedCanvases;

        // =================================
        // APPEND LIGHTS
        // =================================

        int oldLightCount =
            hiddenLights.Length;

        Light[] combinedLights =
            new Light[
                oldLightCount + lights.Length
            ];

        for (int i = 0;
             i < oldLightCount;
             i++)
        {
            combinedLights[i] =
                hiddenLights[i];
        }

        for (int i = 0;
             i < lights.Length;
             i++)
        {
            combinedLights[
                oldLightCount + i
            ] = lights[i];
        }

        hiddenLights = combinedLights;
    }

    // =====================================
    // APPLY LOCAL RESTRICTIONS
    // =====================================

    private void ApplyRestrictions()
    {
        // Hide meshes, sprites, particles,
        // skinned meshes and other renderers.

        for (int i = 0;
             i < hiddenRenderers.Length;
             i++)
        {
            Renderer component =
                hiddenRenderers[i];

            if (Utilities.IsValid(component) &&
                component.enabled)
            {
                component.enabled = false;
            }
        }

        // Prevent local collider interactions.

        for (int i = 0;
             i < hiddenColliders.Length;
             i++)
        {
            Collider component =
                hiddenColliders[i];

            if (Utilities.IsValid(component) &&
                component.enabled)
            {
                component.enabled = false;
            }
        }

        // Hide Unity UI.

        for (int i = 0;
             i < hiddenCanvases.Length;
             i++)
        {
            Canvas component =
                hiddenCanvases[i];

            if (Utilities.IsValid(component) &&
                component.enabled)
            {
                component.enabled = false;
            }
        }

        // Disable lights belonging to
        // protected object hierarchies.

        for (int i = 0;
             i < hiddenLights.Length;
             i++)
        {
            Light component =
                hiddenLights[i];

            if (Utilities.IsValid(component) &&
                component.enabled)
            {
                component.enabled = false;
            }
        }
    }

    // =====================================
    // ENFORCE VISIBILITY
    // =====================================

    private void LateUpdate()
    {
        if (!restricted)
            return;

        // Prevent another behavior or animation
        // from accidentally leaving a cached
        // visual component enabled.
        //
        // Component searches are NOT repeated
        // each frame; only cached references
        // are checked.

        ApplyRestrictions();
    }
}
