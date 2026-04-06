using System.IO;
using MelonLoader.Utils;

namespace DataCenterModLoader;

/// <summary>
/// Canonical paths under the game root and MelonLoader <see cref="MelonEnvironment.UserDataDirectory"/>.
/// Mod-related JSON (and other mod sidecar files) live under <c>UserData/ModCfg</c>.
/// FMF plugin DLLs are deployed under <c>FMF/Plugins</c>; MelonLoader still loads mods from <see cref="MelonEnvironment.ModsDirectory"/> (see docs).
/// </summary>
public static class GameFolderLayout
{
	public static string ModCfgDirectory => Path.Combine(MelonEnvironment.UserDataDirectory, "ModCfg");

	/// <summary>FFM / FMF extension plugins (DLLs). MelonLoader does not scan this folder — see documentation for load options.</summary>
	public static string FmfPluginsDirectory => Path.Combine(MelonEnvironment.GameRootDirectory, "FMF", "Plugins");

	public static string MelonModsDirectory => MelonEnvironment.ModsDirectory;

	public static void EnsureModCfgDirectoryExists()
	{
		Directory.CreateDirectory(ModCfgDirectory);
	}

	public static void EnsureFmfPluginsDirectoryExists()
	{
		Directory.CreateDirectory(FmfPluginsDirectory);
	}

	/// <summary>
	/// Resolves a file under <see cref="ModCfgDirectory"/>. If <paramref name="migrateFromUserDataRoot"/> is true and the file is missing,
	/// copies from the legacy location <c>UserData/&lt;fileName&gt;</c> when present.
	/// </summary>
	public static string ResolveModCfgFile(string fileName, bool migrateFromUserDataRoot = true)
	{
		EnsureModCfgDirectoryExists();
		var preferred = Path.Combine(ModCfgDirectory, fileName);
		if (File.Exists(preferred))
		{
			return preferred;
		}

		if (migrateFromUserDataRoot)
		{
			var legacy = Path.Combine(MelonEnvironment.UserDataDirectory, fileName);
			if (File.Exists(legacy))
			{
				try
				{
					File.Copy(legacy, preferred, overwrite: false);
				}
				catch
				{
					return legacy;
				}
			}
		}

		return preferred;
	}
}
