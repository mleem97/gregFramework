using Il2Cpp;
using UnityEngine;

namespace FMF.HexLabelMod;

/// <summary>Raycast from the main camera for cable spinner / rack color under the crosshair.</summary>
internal static class HexTargetResolver
{
    private const float MaxRayDistance = 48f;

    public static bool TryGetAimedColor(out string hex, out string label)
    {
        hex = null;
        label = null;

        var cam = Camera.main;
        if (cam == null)
            return false;

        var ray = new Ray(cam.transform.position, cam.transform.forward);
        if (!Physics.Raycast(ray, out var hit, MaxRayDistance, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Collide))
            return false;

        var spinner = hit.collider.GetComponentInParent<CableSpinner>();
        if (spinner != null && GameObjectColorHex.TryGetSpinnerHex(spinner, out hex))
        {
            label = "Spinner";
            return true;
        }

        var rack = hit.collider.GetComponentInParent<Rack>();
        if (rack != null && GameObjectColorHex.TryGetRackHex(rack, out hex))
        {
            label = "Rack";
            return true;
        }

        return false;
    }
}
