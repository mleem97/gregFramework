using System;
using MelonLoader;

[assembly: MelonInfo(typeof(ModPathRedirector.ModPathRedirectorMod), "ModPathRedirector", "1.3.0", "DataCenterExporter")]
[assembly: MelonGame("Waseku", "Data Center")]

namespace ModPathRedirector;

/// <summary>
/// MelonLoader <b>plugin</b> (<c>{GameRoot}/Plugins/</c>). Uses the game's <c>steam_api64.dll</c>
/// flat API (same Steam session as the executable). Triggers Workshop downloads for subscribed items
/// once the Steam client and UGC interface are available.
/// </summary>
public sealed class ModPathRedirectorMod : MelonPlugin
{
	private const int MaxFramesWaitForSteam = 1200;

	private int _waitFrames;
	private bool _workshopTriggered;

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
				"Timed out waiting for Steam; skipped Workshop download trigger.");
			return;
		}

		if (!SteamFlatUgc.TryEnsureUgc(out var steamRunning))
		{
			if (!steamRunning)
				return;

			if (SteamFlatUgc.FailedResolve)
			{
				_workshopTriggered = true;
				MelonEvents.OnUpdate.Unsubscribe(OnUpdateTryTriggerWorkshop);
				LoggerInstance.Warning(
					"Could not resolve ISteamUGC from steam_api64.dll (no matching SteamAPI_SteamUGC_v0xx export).");
			}

			return;
		}

		try
		{
			TriggerWorkshopDownloads();
			_workshopTriggered = true;
			MelonEvents.OnUpdate.Unsubscribe(OnUpdateTryTriggerWorkshop);
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
		var count = SteamFlatUgc.GetNumSubscribedItems();
		if (count == 0)
		{
			LoggerInstance.Msg("No subscribed Workshop items.");
			return;
		}

		var items = new ulong[count];
		var filled = SteamFlatUgc.GetSubscribedItems(items, count);

		var pending = 0u;
		for (var i = 0; i < (int)filled; i++)
		{
			var id = items[i];
			var state = SteamFlatUgc.GetItemState(id);

			var installed = (state & SteamFlatUgc.ItemState.Installed) != 0;
			var downloading = (state & SteamFlatUgc.ItemState.Downloading) != 0;
			var downloadPending = (state & SteamFlatUgc.ItemState.DownloadPending) != 0;
			var needsUpdate = (state & SteamFlatUgc.ItemState.NeedsUpdate) != 0;

			if (installed && !needsUpdate)
				continue;

			if (!downloading && !downloadPending)
			{
				if (SteamFlatUgc.DownloadItem(id, true))
				{
					pending++;
					LoggerInstance.Msg($"  Triggered download: workshop_{id}");
				}
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
