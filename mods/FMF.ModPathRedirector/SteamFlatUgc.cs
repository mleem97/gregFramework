using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using MelonLoader.Utils;

namespace ModPathRedirector;

/// <summary>
/// Direct <c>steam_api64.dll</c> flat API calls (same process / same Steam session as the game).
/// Resolves <c>SteamAPI_SteamUGC_v0xx</c> at runtime so different SDK versions still load.
/// </summary>
internal static class SteamFlatUgc
{
	private const string SteamDllName = "steam_api64.dll";

	private static readonly string[] UgcAccessorExports =
	[
		"SteamAPI_SteamUGC_v023",
		"SteamAPI_SteamUGC_v022",
		"SteamAPI_SteamUGC_v021",
		"SteamAPI_SteamUGC_v020",
		"SteamAPI_SteamUGC_v019",
	];

	private static IntPtr _module;
	private static IntPtr _ugc;
	private static bool _failedResolve;

	internal static bool FailedResolve => _failedResolve;

	static SteamFlatUgc()
	{
		// Load before any DllImport: the game ships steam_api64.dll under Unity's native plugin folder
		// ({GameRoot}/{ExeName}_Data/Plugins/x86_64/), not only next to the exe.
		TryPreloadSteamApi();
	}

	/// <summary>Steam item state flags (client).</summary>
	internal static class ItemState
	{
		internal const uint Installed = 4;
		internal const uint Downloading = 16;
		internal const uint DownloadPending = 32;
		internal const uint NeedsUpdate = 8;
	}

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	private delegate IntPtr GetSteamUgcFn();

	[DllImport(SteamDllName, EntryPoint = "SteamAPI_IsSteamRunning", CallingConvention = CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.I1)]
	private static extern bool SteamAPI_IsSteamRunning();

	[DllImport(SteamDllName, EntryPoint = "SteamAPI_ISteamUGC_GetNumSubscribedItems", CallingConvention = CallingConvention.Cdecl)]
	private static extern uint ISteamUGC_GetNumSubscribedItems(IntPtr instancePtr, [MarshalAs(UnmanagedType.I1)] bool bIncludeLocallyDisabled);

	[DllImport(SteamDllName, EntryPoint = "SteamAPI_ISteamUGC_GetSubscribedItems", CallingConvention = CallingConvention.Cdecl)]
	private static extern uint ISteamUGC_GetSubscribedItems(IntPtr instancePtr, [In, Out] ulong[] pvecPublishedFileID, uint cMaxEntries, [MarshalAs(UnmanagedType.I1)] bool bIncludeLocallyDisabled);

	[DllImport(SteamDllName, EntryPoint = "SteamAPI_ISteamUGC_GetItemState", CallingConvention = CallingConvention.Cdecl)]
	private static extern uint ISteamUGC_GetItemState(IntPtr instancePtr, ulong nPublishedFileID);

	[DllImport(SteamDllName, EntryPoint = "SteamAPI_ISteamUGC_DownloadItem", CallingConvention = CallingConvention.Cdecl)]
	[return: MarshalAs(UnmanagedType.I1)]
	private static extern bool ISteamUGC_DownloadItem(IntPtr instancePtr, ulong nPublishedFileID, [MarshalAs(UnmanagedType.I1)] bool bHighPriority);

	/// <summary>
	/// True when the Steam client is running and an <c>ISteamUGC*</c> pointer was obtained.
	/// </summary>
	internal static bool TryEnsureUgc(out bool steamRunning)
	{
		steamRunning = false;
		if (_failedResolve)
			return false;

		if (_ugc != IntPtr.Zero)
		{
			steamRunning = true;
			return true;
		}

		if (!TryLoadModule())
			return false;

		steamRunning = SteamAPI_IsSteamRunning();
		if (!steamRunning)
			return false;

		foreach (var export in UgcAccessorExports)
		{
			if (!NativeLibrary.TryGetExport(_module, export, out var addr))
				continue;

			var fn = Marshal.GetDelegateForFunctionPointer<GetSteamUgcFn>(addr);
			try
			{
				var p = fn();
				if (p != IntPtr.Zero)
				{
					_ugc = p;
					return true;
				}
			}
			catch
			{
				// try next export name
			}
		}

		_failedResolve = true;
		return false;
	}

	private static void TryPreloadSteamApi()
	{
		if (_module != IntPtr.Zero)
			return;

		foreach (var candidate in EnumerateSteamApiCandidates())
		{
			if (!File.Exists(candidate))
				continue;
			try
			{
				_module = NativeLibrary.Load(candidate);
				return;
			}
			catch
			{
				// try next
			}
		}

		try
		{
			_module = NativeLibrary.Load(SteamDllName);
		}
		catch
		{
			// Leave _module zero; TryEnsureUgc will fail without the DLL
		}
	}

	private static IEnumerable<string> EnumerateSteamApiCandidates()
	{
		var gameRoot = MelonEnvironment.GameRootDirectory;
		var exeName = MelonEnvironment.GameExecutableName;
		if (string.IsNullOrEmpty(exeName))
			exeName = "Data Center";

		yield return Path.Combine(gameRoot, exeName + "_Data", "Plugins", "x86_64", SteamDllName);
		yield return Path.Combine(gameRoot, SteamDllName);
	}

	private static bool TryLoadModule()
	{
		if (_module != IntPtr.Zero)
			return true;

		TryPreloadSteamApi();
		return _module != IntPtr.Zero;
	}

	internal static uint GetNumSubscribedItems(bool includeLocallyDisabled = false)
		=> ISteamUGC_GetNumSubscribedItems(_ugc, includeLocallyDisabled);

	internal static uint GetSubscribedItems(ulong[] buffer, uint maxEntries, bool includeLocallyDisabled = false)
		=> ISteamUGC_GetSubscribedItems(_ugc, buffer, maxEntries, includeLocallyDisabled);

	internal static uint GetItemState(ulong publishedFileId)
		=> ISteamUGC_GetItemState(_ugc, publishedFileId);

	internal static bool DownloadItem(ulong publishedFileId, bool highPriority)
		=> ISteamUGC_DownloadItem(_ugc, publishedFileId, highPriority);
}
