namespace WorkshopUploader.Models;

/// <summary>Scaffold layout under <c>content/</c> for a new Workshop project.</summary>
public enum WorkshopTemplateKind
{
	/// <summary>Game Object / Decoration style assets (vanilla Workshop delivery).</summary>
	VanillaObjectDecoration,

	/// <summary>MelonLoader mods — mirror <c>Mods/</c> when installed.</summary>
	ModdedMelonLoader,

	/// <summary>FrikaModFramework plugins under <c>FMF/Plugins/</c>.</summary>
	ModdedFrikaModFramework,
}
