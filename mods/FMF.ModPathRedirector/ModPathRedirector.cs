using System;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Il2CppSteamworks;
using MelonLoader;

[assembly: MelonInfo(typeof(ModPathRedirector.ModPathRedirectorMod), "ModPathRedirector", "1.2.1", "DataCenterExporter")]
[assembly: MelonGame("Waseku", "Data Center")]

namespace ModPathRedirector;

/// <summary>
/// MelonLoader <b>plugin</b> (lives in <c>{GameRoot}/Plugins/</c>). Triggers Steam Workshop
/// downloads for subscribed items once the Il2Cpp Steam API is ready (the game initializes Steam
/// after plugin load). Native mods use
/// <c>Data Center_Data/StreamingAssets/Mods/workshop_<PublishedFileId>/</c> (no path redirect).
/// </summary>
public sealed class ModPathRedirectorMod : MelonPlugin
{
    private const int MaxFramesWaitForSteam = 1200; // ~20s @ 60fps

    private int _waitFrames;
    private bool _workshopTriggered;

    /// <summary>
    /// Subscribe after startup; Steam is not initialized during <see cref="OnPreModsLoaded"/>.
    /// </summary>
    public override void OnApplicationStarted()
    {
        MelonEvents.OnUpdate.Subscribe(OnUpdateTryTriggerWorkshop, 100);
    }

    private void OnUpdateTryTriggerWorkshop()
    {
        if (_workshopTriggered)
            return;

        if (_waitFrames++ > MaxFramesWaitForSteam)
        {
            _workshopTriggered = true;
            MelonEvents.OnUpdate.Unsubscribe(OnUpdateTryTriggerWorkshop);
            LoggerInstance.Warning(
                "Steam not ready in time; skipped Workshop download trigger (subscribed items may sync when the game loads them).");
            return;
        }

        try
        {
            TriggerWorkshopDownloads();
            _workshopTriggered = true;
            MelonEvents.OnUpdate.Unsubscribe(OnUpdateTryTriggerWorkshop);
        }
        catch (InvalidOperationException ex)
        {
            var msg = ex.Message ?? "";
            if (msg.Contains("initialized", StringComparison.OrdinalIgnoreCase) ||
                msg.Contains("Steamworks", StringComparison.OrdinalIgnoreCase))
            {
                // Game has not called SteamAPI_Init yet — try next frame.
                return;
            }

            _workshopTriggered = true;
            MelonEvents.OnUpdate.Unsubscribe(OnUpdateTryTriggerWorkshop);
            LoggerInstance.Warning($"Workshop download trigger failed: {ex.Message}");
        }
        catch (Exception ex)
        {
            _workshopTriggered = true;
            MelonEvents.OnUpdate.Unsubscribe(OnUpdateTryTriggerWorkshop);
            LoggerInstance.Warning($"Workshop download trigger failed: {ex.Message}");
        }
    }

    private void TriggerWorkshopDownloads()
    {
        var count = SteamUGC.GetNumSubscribedItems();
        if (count == 0)
        {
            LoggerInstance.Msg("No subscribed Workshop items.");
            return;
        }

        var items = new Il2CppStructArray<PublishedFileId_t>((long)count);
        var filled = SteamUGC.GetSubscribedItems(items, count);

        var pending = 0;
        for (int i = 0; i < (int)filled; i++)
        {
            var id = items[i];
            var state = SteamUGC.GetItemState(id);

            var installed = (state & (uint)EItemState.k_EItemStateInstalled) != 0;
            var downloading = (state & (uint)EItemState.k_EItemStateDownloading) != 0;
            var downloadPending = (state & (uint)EItemState.k_EItemStateDownloadPending) != 0;
            var needsUpdate = (state & (uint)EItemState.k_EItemStateNeedsUpdate) != 0;

            if (installed && !needsUpdate)
                continue;

            if (!downloading && !downloadPending)
            {
                SteamUGC.DownloadItem(id, true);
                pending++;
                LoggerInstance.Msg($"  Triggered download: workshop_{id.m_PublishedFileId}");
            }
        }

        if (pending > 0)
            LoggerInstance.Msg(
                $"Triggered {pending} Workshop download(s). " +
                "The game will copy items into StreamingAssets/Mods/workshop_<ID> when ready.");
        else
            LoggerInstance.Msg($"All {filled} subscribed Workshop item(s) are up to date.");
    }
}
