using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using MelonLoader;

namespace DataCenterModLoader;

public class FFIBridge : IDisposable
{
    private static readonly string[] SupportedNativeExtensions = { ".dll", ".greg", ".gregr", ".gregl", ".gregp" };

    private readonly MelonLogger.Instance _logger;
    private readonly string _modsPath;
    private readonly GameAPIManager _apiManager;
    private readonly List<RustMod> _loadedMods = new();
    private readonly List<string> _shadowCopies = new();

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern IntPtr LoadLibrary(string lpFileName);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern IntPtr GetProcAddress(IntPtr hModule, string lpProcName);

    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool FreeLibrary(IntPtr hModule);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate ModInfoFFI ModInfoDelegate();

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    [return: MarshalAs(UnmanagedType.U1)]
    private delegate bool ModInitDelegate(IntPtr apiTable);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void ModUpdateDelegate(float deltaTime);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void ModOnSceneLoadedDelegate(IntPtr sceneName);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void ModShutdownDelegate();

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void ModOnEventDelegate(uint eventId, IntPtr eventData, uint dataSize);

    [StructLayout(LayoutKind.Sequential)]
    private struct ModInfoFFI
    {
        public IntPtr Id;
        public IntPtr Name;
        public IntPtr Version;
        public IntPtr Author;
        public IntPtr Description;
    }

    private class RustMod
    {
        public string FilePath = "";
        public string LoadPath = "";
        public string Id = "unknown";
        public string Name = "Unknown";
        public string Version = "0.0.0";
        public string Author = "Unknown";
        public IntPtr Handle;
        public ModUpdateDelegate Update;
        public ModUpdateDelegate FixedUpdate;
        public ModOnSceneLoadedDelegate OnSceneLoaded;
        public ModShutdownDelegate Shutdown;
        public ModOnEventDelegate OnEvent;
    }

    public FFIBridge(MelonLogger.Instance logger, string modsPath)
    {
        _logger = logger;
        _modsPath = modsPath;
        _apiManager = new GameAPIManager(logger);
    }

    public void LoadAllMods()
    {
        if (!Directory.Exists(_modsPath))
        {
            _logger.Msg("No Mods/RustMods/ directory found.");
            return;
        }

        var modFiles = GetSupportedModFiles(_modsPath);
        if (modFiles.Count == 0)
        {
            _logger.Msg("No native mod files found in Mods/RustMods/.");
            return;
        }

        _logger.Msg($"Found {modFiles.Count} native mod file(s).");

        for (int index = 0; index < modFiles.Count; index++)
        {
            string modPath = modFiles[index];
            try { LoadMod(modPath); }
            catch (Exception ex) { _logger.Error($"Failed to load '{Path.GetFileName(modPath)}': {ex.Message}"); }
        }

        _logger.Msg($"{_loadedMods.Count} native mod(s) loaded successfully.");
    }

    private static List<string> GetSupportedModFiles(string root)
    {
        var files = new List<string>();
        var allFiles = Directory.GetFiles(root, "*.*", SearchOption.AllDirectories);

        for (int index = 0; index < allFiles.Length; index++)
        {
            string path = allFiles[index];
            string ext = Path.GetExtension(path) ?? string.Empty;

            for (int extIndex = 0; extIndex < SupportedNativeExtensions.Length; extIndex++)
            {
                if (!string.Equals(ext, SupportedNativeExtensions[extIndex], StringComparison.OrdinalIgnoreCase))
                    continue;

                files.Add(path);
                break;
            }
        }

        return files;
    }

    private string GetLoadPath(string modPath)
    {
        string extension = Path.GetExtension(modPath) ?? string.Empty;
        if (string.Equals(extension, ".dll", StringComparison.OrdinalIgnoreCase))
            return modPath;

        string cacheDir = Path.Combine(Path.GetTempPath(), "FrikaMF", "NativeModShadow");
        Directory.CreateDirectory(cacheDir);

        string fileName = Path.GetFileNameWithoutExtension(modPath);
        string shadowName = $"{fileName}_{Math.Abs(modPath.GetHashCode())}.dll";
        string shadowPath = Path.Combine(cacheDir, shadowName);

        File.Copy(modPath, shadowPath, true);

        if (!_shadowCopies.Contains(shadowPath))
            _shadowCopies.Add(shadowPath);

        return shadowPath;
    }

    private void LoadMod(string modPath)
    {
        var fileName = Path.GetFileName(modPath);
        string extension = Path.GetExtension(modPath) ?? string.Empty;
        string loadPath = GetLoadPath(modPath);

        _logger.Msg($"Loading native mod: {fileName}");

        if (string.Equals(extension, ".gregl", StringComparison.OrdinalIgnoreCase)
            || string.Equals(extension, ".gregp", StringComparison.OrdinalIgnoreCase))
        {
            _logger.Warning($"  '{fileName}' is loaded as native adapter module. Embedded Lua/Python host is not implemented in core.");
        }

        CrashLog.Log($"LoadMod: about to call LoadLibrary for '{fileName}'");
        var handle = LoadLibrary(loadPath);
        if (handle == IntPtr.Zero)
        {
            var error = Marshal.GetLastWin32Error();
            throw new Exception($"LoadLibrary failed with error code {error}");
        }
        CrashLog.Log($"LoadMod: LoadLibrary succeeded for '{fileName}', handle=0x{handle.ToInt64():X}");

        var mod = new RustMod { FilePath = modPath, LoadPath = loadPath, Handle = handle };

        // mod_info
        var modInfoPtr = GetProcAddress(handle, "mod_info");
        if (modInfoPtr != IntPtr.Zero)
        {
            var modInfoFn = Marshal.GetDelegateForFunctionPointer<ModInfoDelegate>(modInfoPtr);
            CrashLog.Log($"LoadMod: about to call modInfoFn() for '{fileName}'");
            var info = modInfoFn();
            CrashLog.Log($"LoadMod: modInfoFn() returned for '{fileName}'");

            mod.Id = Marshal.PtrToStringAnsi(info.Id) ?? "unknown";
            mod.Name = Marshal.PtrToStringAnsi(info.Name) ?? "Unknown";
            mod.Version = Marshal.PtrToStringAnsi(info.Version) ?? "0.0.0";
            mod.Author = Marshal.PtrToStringAnsi(info.Author) ?? "Unknown";
            var description = Marshal.PtrToStringAnsi(info.Description) ?? "";

            _logger.Msg($"  Mod: {mod.Name} v{mod.Version} by {mod.Author}");
            _logger.Msg($"  Description: {description}");
        }
        else
        {
            _logger.Warning($"  '{fileName}' has no mod_info() export.");
        }

        // mod_init
        var modInitPtr = GetProcAddress(handle, "mod_init");
        if (modInitPtr != IntPtr.Zero)
        {
            var modInitFn = Marshal.GetDelegateForFunctionPointer<ModInitDelegate>(modInitPtr);
            CrashLog.Log($"LoadMod: about to call modInitFn() for '{mod.Name}'");
            if (!modInitFn(_apiManager.GetTablePointer()))
            {
                _logger.Error($"  Mod '{mod.Name}' mod_init() returned false.");
                FreeLibrary(handle);
                return;
            }
            CrashLog.Log($"LoadMod: modInitFn() succeeded for '{mod.Name}'");
            _logger.Msg($"  Mod '{mod.Name}' initialized.");
        }
        else
        {
            _logger.Warning($"  '{fileName}' has no mod_init() export.");
        }

        // Optional exports
        CrashLog.Log($"LoadMod: resolving optional export 'mod_update' for '{mod.Name}'");
        var updatePtr = GetProcAddress(handle, "mod_update");
        if (updatePtr != IntPtr.Zero)
            mod.Update = Marshal.GetDelegateForFunctionPointer<ModUpdateDelegate>(updatePtr);

        CrashLog.Log($"LoadMod: resolving optional export 'mod_fixed_update' for '{mod.Name}'");
        var fixedUpdatePtr = GetProcAddress(handle, "mod_fixed_update");
        if (fixedUpdatePtr != IntPtr.Zero)
            mod.FixedUpdate = Marshal.GetDelegateForFunctionPointer<ModUpdateDelegate>(fixedUpdatePtr);

        CrashLog.Log($"LoadMod: resolving optional export 'mod_on_scene_loaded' for '{mod.Name}'");
        var sceneLoadedPtr = GetProcAddress(handle, "mod_on_scene_loaded");
        if (sceneLoadedPtr != IntPtr.Zero)
            mod.OnSceneLoaded = Marshal.GetDelegateForFunctionPointer<ModOnSceneLoadedDelegate>(sceneLoadedPtr);

        CrashLog.Log($"LoadMod: resolving optional export 'mod_shutdown' for '{mod.Name}'");
        var shutdownPtr = GetProcAddress(handle, "mod_shutdown");
        if (shutdownPtr != IntPtr.Zero)
            mod.Shutdown = Marshal.GetDelegateForFunctionPointer<ModShutdownDelegate>(shutdownPtr);

        CrashLog.Log($"LoadMod: resolving optional export 'mod_on_event' for '{mod.Name}'");
        var onEventPtr = GetProcAddress(handle, "mod_on_event");
        if (onEventPtr != IntPtr.Zero)
        {
            mod.OnEvent = Marshal.GetDelegateForFunctionPointer<ModOnEventDelegate>(onEventPtr);
            _logger.Msg($"  Mod '{mod.Name}' supports game events.");
        }

        CrashLog.Log($"LoadMod: finished loading '{mod.Name}' successfully");
        _loadedMods.Add(mod);
    }

    public void OnUpdate(float deltaTime)
    {
        try
        {
            foreach (var mod in _loadedMods)
            {
                try { mod.Update?.Invoke(deltaTime); }
                catch (Exception ex)
                {
                    _logger.Error($"[{mod.Name}] mod_update crashed: {ex.Message}");
                    CrashLog.LogException($"[{mod.Name}] mod_update", ex);
                }
            }
        }
        catch (Exception ex)
        {
            CrashLog.LogException("FFIBridge.OnUpdate outer", ex);
        }
    }

    public void OnFixedUpdate(float deltaTime)
    {
        try
        {
            foreach (var mod in _loadedMods)
            {
                try { mod.FixedUpdate?.Invoke(deltaTime); }
                catch (Exception ex)
                {
                    _logger.Error($"[{mod.Name}] mod_fixed_update crashed: {ex.Message}");
                    CrashLog.LogException($"[{mod.Name}] mod_fixed_update", ex);
                }
            }
        }
        catch (Exception ex)
        {
            CrashLog.LogException("FFIBridge.OnFixedUpdate outer", ex);
        }
    }

    public void OnSceneLoaded(string sceneName)
    {
        var ptr = Marshal.StringToHGlobalAnsi(sceneName);
        try
        {
            foreach (var mod in _loadedMods)
            {
                try { mod.OnSceneLoaded?.Invoke(ptr); }
                catch (Exception ex) { _logger.Error($"[{mod.Name}] mod_on_scene_loaded crashed: {ex.Message}"); }
            }
        }
        finally { Marshal.FreeHGlobal(ptr); }
    }

    public void DispatchEvent(uint eventId, IntPtr eventData, uint dataSize)
    {
        foreach (var mod in _loadedMods)
        {
            if (mod.OnEvent == null) continue;
            CrashLog.Log($"DispatchEvent: calling mod_on_event(id={eventId}, dataSize={dataSize}) on '{mod.Name}'");
            try { mod.OnEvent.Invoke(eventId, eventData, dataSize); }
            catch (Exception ex) { _logger.Error($"[{mod.Name}] mod_on_event(id={eventId}) crashed: {ex.Message}"); }
        }
    }

    public void Shutdown()
    {
        foreach (var mod in _loadedMods)
        {
            try
            {
                _logger.Msg($"Shutting down: {mod.Name}");
                mod.Shutdown?.Invoke();
            }
            catch (Exception ex) { _logger.Error($"[{mod.Name}] mod_shutdown crashed: {ex.Message}"); }
        }
    }

    public int ReloadAllMods()
    {
        _logger.Msg("Reloading Rust mods (hotload)...");
        UnloadLoadedMods(callShutdown: true);
        LoadAllMods();
        return _loadedMods.Count;
    }

    private void UnloadLoadedMods(bool callShutdown)
    {
        if (callShutdown)
        {
            foreach (var mod in _loadedMods)
            {
                try
                {
                    _logger.Msg($"Shutting down: {mod.Name}");
                    mod.Shutdown?.Invoke();
                }
                catch (Exception ex)
                {
                    _logger.Error($"[{mod.Name}] mod_shutdown crashed: {ex.Message}");
                }
            }
        }

        foreach (var mod in _loadedMods)
        {
            if (mod.Handle == IntPtr.Zero)
                continue;

            try
            {
                FreeLibrary(mod.Handle);
            }
            catch
            {
            }
        }

        _loadedMods.Clear();

        for (int index = 0; index < _shadowCopies.Count; index++)
        {
            string shadowPath = _shadowCopies[index];
            try
            {
                if (File.Exists(shadowPath))
                    File.Delete(shadowPath);
            }
            catch
            {
            }
        }

        _shadowCopies.Clear();
    }

    public void Dispose()
    {
        UnloadLoadedMods(callShutdown: false);
        _apiManager.Dispose();
    }
}
