using System;
using System.Collections.Generic;
using Il2Cpp;
using MelonLoader;
using UnityEngine;

namespace DataCenterModLoader;

/// <summary>
/// Consolidated Modig API layer providing safe, documented access to core game systems.
/// Previously split across ModigAPIs, now unified within FrikaMF runtime.
/// </summary>
public static class ModigApi
{
    // ════════════════════════════════════════════════════════════════════════════════════
    // FOUNDATION: Game Singletons Access
    // ════════════════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Checks if the game core systems are initialized and ready for API calls.
    /// </summary>
    /// <returns>True if PlayerManager and essential singletons are available.</returns>
    public static bool IsGameReady()
    {
        return PlayerManager.instance != null && PlayerManager.instance.playerClass != null;
    }

    /// <summary>
    /// Retrieves the raw Player singleton instance.
    /// May return null if game is not fully loaded.
    /// </summary>
    public static Player GetPlayerRaw()
    {
        return PlayerManager.instance?.playerClass;
    }

    /// <summary>
    /// Retrieves the raw NetworkMap singleton (network topology state).
    /// </summary>
    public static NetworkMap GetNetworkMapRaw()
    {
        return NetworkMap.instance;
    }

    /// <summary>
    /// Retrieves the raw UI elements singleton (notifications, messages).
    /// </summary>
    public static StaticUIElements GetUiRaw()
    {
        return StaticUIElements.instance;
    }

    /// <summary>
    /// Retrieves the raw TimeController singleton (in-game time).
    /// </summary>
    public static TimeController GetTimeRaw()
    {
        return TimeController.instance;
    }

    /// <summary>
    /// Retrieves the raw Localisation singleton (i18n/text system).
    /// </summary>
    public static Localisation GetLocalisationRaw()
    {
        return Localisation.instance;
    }

    // ════════════════════════════════════════════════════════════════════════════════════
    // PLAYER API: Money, XP, Reputation
    // ════════════════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Checks if the player instance is available.
    /// </summary>
    public static bool IsPlayerAvailable()
    {
        return GetPlayerRaw() != null;
    }

    /// <summary>
    /// Gets the player's current money balance.
    /// </summary>
    public static float GetPlayerMoney()
    {
        var player = GetPlayerRaw();
        return player != null ? player.money : 0f;
    }

    /// <summary>
    /// Gets the player's current XP points.
    /// </summary>
    public static float GetPlayerXp()
    {
        var player = GetPlayerRaw();
        return player != null ? player.xp : 0f;
    }

    /// <summary>
    /// Gets the player's current reputation score.
    /// </summary>
    public static float GetPlayerReputation()
    {
        var player = GetPlayerRaw();
        return player != null ? player.reputation : 0f;
    }

    /// <summary>
    /// Attempts to add money to the player balance.
    /// Dispatches MoneyChanged event if successful.
    /// </summary>
    /// <param name="amount">Amount to add (can be negative to subtract).</param>
    /// <param name="withoutSound">If true, suppresses payment sound effect.</param>
    /// <returns>True if operation succeeded; false if player unavailable.</returns>
    public static bool TryAddMoney(float amount, bool withoutSound = true)
    {
        var player = GetPlayerRaw();
        if (player == null)
            return false;

        player.UpdateCoin(amount, withoutSound);
        return true;
    }

    /// <summary>
    /// Attempts to set the player's money to an absolute value.
    /// Calculates delta internally and calls UpdateCoin.
    /// </summary>
    /// <param name="targetMoney">Absolute money target.</param>
    /// <param name="withoutSound">If true, suppresses payment sound effect.</param>
    /// <returns>True if operation succeeded; false if player unavailable.</returns>
    public static bool TrySetMoney(float targetMoney, bool withoutSound = true)
    {
        var player = GetPlayerRaw();
        if (player == null)
            return false;

        var delta = targetMoney - player.money;
        player.UpdateCoin(delta, withoutSound);
        return true;
    }

    /// <summary>
    /// Attempts to add XP to the player.
    /// Dispatches XPChanged event if successful.
    /// </summary>
    /// <param name="amount">XP amount to add (can be negative).</param>
    /// <returns>True if operation succeeded; false if player unavailable.</returns>
    public static bool TryAddXp(float amount)
    {
        var player = GetPlayerRaw();
        if (player == null)
            return false;

        player.UpdateXP(amount);
        return true;
    }

    /// <summary>
    /// Attempts to set the player's XP to an absolute value.
    /// </summary>
    /// <param name="targetXp">Absolute XP target.</param>
    /// <returns>True if operation succeeded; false if player unavailable.</returns>
    public static bool TrySetXp(float targetXp)
    {
        var player = GetPlayerRaw();
        if (player == null)
            return false;

        var delta = targetXp - player.xp;
        player.UpdateXP(delta);
        return true;
    }

    /// <summary>
    /// Attempts to add reputation to the player.
    /// Dispatches ReputationChanged event if successful.
    /// </summary>
    /// <param name="amount">Reputation amount to add (can be negative).</param>
    /// <returns>True if operation succeeded; false if player unavailable.</returns>
    public static bool TryAddReputation(float amount)
    {
        var player = GetPlayerRaw();
        if (player == null)
            return false;

        player.UpdateReputation(amount);
        return true;
    }

    /// <summary>
    /// Attempts to set the player's reputation to an absolute value.
    /// </summary>
    /// <param name="targetReputation">Absolute reputation target.</param>
    /// <returns>True if operation succeeded; false if player unavailable.</returns>
    public static bool TrySetReputation(float targetReputation)
    {
        var player = GetPlayerRaw();
        if (player == null)
            return false;

        var delta = targetReputation - player.reputation;
        player.UpdateReputation(delta);
        return true;
    }

    // ════════════════════════════════════════════════════════════════════════════════════
    // TIME API: Day, Hour, Multiplier
    // ════════════════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Checks if the time system is available.
    /// </summary>
    public static bool IsTimeAvailable()
    {
        return GetTimeRaw() != null;
    }

    /// <summary>
    /// Gets the current in-game day number.
    /// </summary>
    public static int GetCurrentDay()
    {
        var time = GetTimeRaw();
        return time != null ? time.day : 0;
    }

    /// <summary>
    /// Gets the current in-game hour as a float (0-24).
    /// </summary>
    public static float GetCurrentHour()
    {
        var time = GetTimeRaw();
        return time != null ? time.CurrentTimeInHours() : 0f;
    }

    /// <summary>
    /// Gets the current time multiplier (time speed).
    /// 1.0 = normal speed, 2.0 = double speed, etc.
    /// </summary>
    public static float GetTimeMultiplier()
    {
        var time = GetTimeRaw();
        return time != null ? time.timeMultiplier : 1f;
    }

    /// <summary>
    /// Attempts to set the time multiplier (game speed).
    /// </summary>
    /// <param name="multiplier">New time multiplier (typical range 0.1 - 10.0).</param>
    /// <returns>True if operation succeeded; false if time system unavailable.</returns>
    public static bool TrySetTimeMultiplier(float multiplier)
    {
        var time = GetTimeRaw();
        if (time == null)
            return false;

        time.timeMultiplier = multiplier;
        return true;
    }

    /// <summary>
    /// Checks if the current game time falls within a given hour range.
    /// Wraps around midnight if startHour > endHour.
    /// </summary>
    /// <param name="startHour">Start hour (0-24).</param>
    /// <param name="endHour">End hour (0-24).</param>
    /// <returns>True if current time is between start and end.</returns>
    public static bool IsTimeBetween(float startHour, float endHour)
    {
        var time = GetTimeRaw();
        return time != null && time.TimeIsBetween(startHour, endHour);
    }

    // ════════════════════════════════════════════════════════════════════════════════════
    // NETWORK API: Servers, Switches, Repair
    // ════════════════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Checks if the network topology system is available.
    /// </summary>
    public static bool IsNetworkAvailable()
    {
        return GetNetworkMapRaw() != null;
    }

    /// <summary>
    /// Gets a snapshot of all active servers in the network.
    /// </summary>
    /// <returns>List of Server instances; empty if network unavailable.</returns>
    public static List<Server> GetServersSnapshot()
    {
        var result = new List<Server>();
        var map = GetNetworkMapRaw();
        if (map == null || map.servers == null)
            return result;

        foreach (var kv in map.servers)
        {
            if (kv.Value != null)
                result.Add(kv.Value);
        }

        return result;
    }

    /// <summary>
    /// Gets a snapshot of all active network switches.
    /// </summary>
    /// <returns>List of NetworkSwitch instances; empty if network unavailable.</returns>
    public static List<NetworkSwitch> GetSwitchesSnapshot()
    {
        var result = new List<NetworkSwitch>();
        var map = GetNetworkMapRaw();
        if (map == null || map.switches == null)
            return result;

        foreach (var kv in map.switches)
        {
            if (kv.Value != null)
                result.Add(kv.Value);
        }

        return result;
    }

    /// <summary>
    /// Gets a snapshot of all currently broken servers.
    /// </summary>
    /// <returns>List of broken Server instances; empty if network unavailable.</returns>
    public static List<Server> GetBrokenServersSnapshot()
    {
        var result = new List<Server>();
        var map = GetNetworkMapRaw();
        if (map == null || map.brokenServers == null)
            return result;

        foreach (var kv in map.brokenServers)
        {
            if (kv.Value != null)
                result.Add(kv.Value);
        }

        return result;
    }

    /// <summary>
    /// Gets a snapshot of all currently broken network switches.
    /// </summary>
    /// <returns>List of broken NetworkSwitch instances; empty if network unavailable.</returns>
    public static List<NetworkSwitch> GetBrokenSwitchesSnapshot()
    {
        var result = new List<NetworkSwitch>();
        var map = GetNetworkMapRaw();
        if (map == null || map.brokenSwitches == null)
            return result;

        foreach (var kv in map.brokenSwitches)
        {
            if (kv.Value != null)
                result.Add(kv.Value);
        }

        return result;
    }

    /// <summary>
    /// Attempts to break a server (mark as failed).
    /// Dispatches ServerBroken event.
    /// </summary>
    /// <param name="server">Server instance to break.</param>
    /// <returns>True if operation succeeded; false if server is null.</returns>
    public static bool TryBreakServer(Server server)
    {
        if (server == null)
            return false;

        server.ItIsBroken();
        return true;
    }

    /// <summary>
    /// Attempts to break a network switch.
    /// Dispatches SwitchBroken event.
    /// </summary>
    /// <param name="networkSwitch">NetworkSwitch instance to break.</param>
    /// <returns>True if operation succeeded; false if switch is null.</returns>
    public static bool TryBreakSwitch(NetworkSwitch networkSwitch)
    {
        if (networkSwitch == null)
            return false;

        networkSwitch.ItIsBroken();
        return true;
    }

    /// <summary>
    /// Attempts to repair a server and optionally power it back on.
    /// Clears warning and error signs as part of repair.
    /// Dispatches ServerRepaired event.
    /// </summary>
    /// <param name="server">Server instance to repair.</param>
    /// <param name="powerOn">If true, turns server back on after repair.</param>
    /// <returns>True if operation succeeded; false if server is null.</returns>
    public static bool TryRepairServer(Server server, bool powerOn = true)
    {
        if (server == null)
            return false;

        server.RepairDevice();
        server.ClearWarningSign(false);
        server.ClearErrorSign();
        if (powerOn)
            server.PowerButton(true);

        return true;
    }

    /// <summary>
    /// Attempts to repair a network switch and optionally power it back on.
    /// </summary>
    /// <param name="networkSwitch">NetworkSwitch instance to repair.</param>
    /// <param name="powerOn">If true, turns switch back on after repair.</param>
    /// <returns>True if operation succeeded; false if switch is null.</returns>
    public static bool TryRepairSwitch(NetworkSwitch networkSwitch, bool powerOn = true)
    {
        if (networkSwitch == null)
            return false;

        networkSwitch.RepairDevice();
        networkSwitch.ClearWarningSign(false);
        networkSwitch.ClearErrorSign();
        if (powerOn)
            networkSwitch.PowerButton(true);

        return true;
    }

    /// <summary>
    /// Bulk repairs all currently broken servers and switches.
    /// </summary>
    /// <param name="powerOn">If true, powers on devices after repair.</param>
    /// <returns>Total count of devices successfully repaired.</returns>
    public static int RepairAllBrokenDevices(bool powerOn = true)
    {
        var repaired = 0;

        var brokenServers = GetBrokenServersSnapshot();
        for (var i = 0; i < brokenServers.Count; i++)
        {
            if (TryRepairServer(brokenServers[i], powerOn))
                repaired++;
        }

        var brokenSwitches = GetBrokenSwitchesSnapshot();
        for (var i = 0; i < brokenSwitches.Count; i++)
        {
            if (TryRepairSwitch(brokenSwitches[i], powerOn))
                repaired++;
        }

        return repaired;
    }

    /// <summary>
    /// Gets a snapshot of network device counts (total and broken).
    /// </summary>
    /// <returns>NetworkDeviceCounts struct with server and switch counts.</returns>
    public static NetworkDeviceCounts GetNetworkDeviceCounts()
    {
        return new NetworkDeviceCounts
        {
            TotalServers = GetServersSnapshot().Count,
            BrokenServers = GetBrokenServersSnapshot().Count,
            TotalSwitches = GetSwitchesSnapshot().Count,
            BrokenSwitches = GetBrokenSwitchesSnapshot().Count,
        };
    }

    // ════════════════════════════════════════════════════════════════════════════════════
    // LOCALISATION API: Language, Text Resolution
    // ════════════════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Checks if the localisation system is available.
    /// </summary>
    public static bool IsLocalisationAvailable()
    {
        return GetLocalisationRaw() != null;
    }

    /// <summary>
    /// Gets the current language name.
    /// </summary>
    /// <returns>Language name string, or "Unknown" if unavailable.</returns>
    public static string GetCurrentLanguageName()
    {
        var loc = GetLocalisationRaw();
        return loc != null ? loc.currentlySelectedLanguage.ToString() : "Unknown";
    }

    /// <summary>
    /// Gets the current language UID used for text lookups.
    /// </summary>
    /// <returns>Language UID, or -1 if unavailable.</returns>
    public static int GetCurrentLanguageUid()
    {
        var loc = GetLocalisationRaw();
        return loc != null ? loc.loadLanguageUID : -1;
    }

    /// <summary>
    /// Gets localised text by its string ID.
    /// </summary>
    /// <param name="uid">Text unique identifier.</param>
    /// <returns>Localised text string, or empty if not found.</returns>
    public static string GetTextById(int uid)
    {
        var loc = GetLocalisationRaw();
        if (loc == null)
            return string.Empty;

        var text = loc.ReturnTextByID(uid);
        return text ?? string.Empty;
    }

    /// <summary>
    /// Attempts to change the active language.
    /// </summary>
    /// <param name="languageUid">Target language UID.</param>
    /// <returns>True if language change succeeded; false if localisation unavailable.</returns>
    public static bool TryChangeLanguage(int languageUid)
    {
        var loc = GetLocalisationRaw();
        if (loc == null)
            return false;

        loc.ChangeLocalisation(languageUid);
        return true;
    }

    /// <summary>
    /// Registers a custom text resolver that intercepts text resolution.
    /// Allows mods to override or augment localised text dynamically.
    /// </summary>
    /// <param name="resolverId">Unique resolver identifier for later unregistration.</param>
    /// <param name="resolver">Delegate to resolve text (called in sequence order).</param>
    public static void RegisterLocalisationResolver(string resolverId, LocalisationResolverDelegate resolver)
    {
        LocalisationBridge.RegisterResolver(resolverId, resolver);
    }

    /// <summary>
    /// Unregisters a previously registered text resolver.
    /// </summary>
    /// <param name="resolverId">Resolver identifier previously used in registration.</param>
    public static void UnregisterLocalisationResolver(string resolverId)
    {
        LocalisationBridge.UnregisterResolver(resolverId);
    }

    // ════════════════════════════════════════════════════════════════════════════════════
    // UI API: Notifications and Messages
    // ════════════════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Checks if the UI system is available.
    /// </summary>
    public static bool IsUiAvailable()
    {
        return GetUiRaw() != null;
    }

    /// <summary>
    /// Attempts to show a notification (HUD popup).
    /// </summary>
    /// <param name="text">Notification text to display.</param>
    /// <param name="localisationUid">Optional localisation UID (-1 for no localisation override).</param>
    /// <param name="sprite">Optional sprite icon to display with notification.</param>
    /// <returns>True if notification was shown; false if UI unavailable.</returns>
    public static bool TryNotify(string text, int localisationUid = -1, Sprite sprite = null)
    {
        var ui = GetUiRaw();
        if (ui == null)
            return false;

        ui.SetNotification(localisationUid, sprite, text ?? string.Empty);
        return true;
    }

    /// <summary>
    /// Attempts to add a message to the in-game message log.
    /// </summary>
    /// <param name="message">Message text to log.</param>
    /// <returns>True if message was logged; false if UI unavailable.</returns>
    public static bool TryAddMessage(string message)
    {
        var ui = GetUiRaw();
        if (ui == null)
            return false;

        ui.AddMeesageInField(message ?? string.Empty);
        return true;
    }

    // ════════════════════════════════════════════════════════════════════════════════════
    // WORLD API: Shop and UI Screen Discovery
    // ════════════════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Finds all active computer shop instances in the current scene.
    /// </summary>
    /// <returns>List of ComputerShop instances; empty if none found.</returns>
    public static List<ComputerShop> FindComputerShops()
    {
        var result = new List<ComputerShop>();
        var shops = UnityEngine.Object.FindObjectsOfType<ComputerShop>();
        if (shops == null)
            return result;

        for (var i = 0; i < shops.Length; i++)
        {
            if (shops[i] != null)
                result.Add(shops[i]);
        }

        return result;
    }

    /// <summary>
    /// Finds the first computer shop that has a network map screen attached.
    /// </summary>
    /// <returns>ComputerShop instance with network map, or null if not found.</returns>
    public static ComputerShop FindFirstShopWithNetworkMapScreen()
    {
        var shops = FindComputerShops();
        for (var i = 0; i < shops.Count; i++)
        {
            var shop = shops[i];
            if (shop != null && shop.networkMapScreen != null)
                return shop;
        }

        return null;
    }

    /// <summary>
    /// Gets the network map screen GameObject from the first available shop.
    /// </summary>
    /// <returns>Network map screen GameObject, or null if not found.</returns>
    public static GameObject GetNetworkMapScreen()
    {
        var shop = FindFirstShopWithNetworkMapScreen();
        return shop?.networkMapScreen;
    }
}

/// <summary>
/// Network device count snapshot.
/// Provides aggregated statistics about server and switch inventory.
/// </summary>
public struct NetworkDeviceCounts
{
    /// <summary>Total number of active servers.</summary>
    public int TotalServers;

    /// <summary>Number of currently broken/offline servers.</summary>
    public int BrokenServers;

    /// <summary>Total number of active network switches.</summary>
    public int TotalSwitches;

    /// <summary>Number of currently broken/offline switches.</summary>
    public int BrokenSwitches;
}
