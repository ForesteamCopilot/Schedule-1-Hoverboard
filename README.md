# An S1 Mono+IL2CPP Template I wish I had when I was learning.

## Features

- As clean as it gets
- Added AssetBundleUtils to load asset bundles (I needed this so bad for UI stuff)
- Conditional compilation for IL2CPP+Mono example
- .csproj comments
- Publicizer (With example entries)
- Includes `UnityEngine.Il2CppAssetBundleManager.dll` for assetbundle loading in Il2cpp

## How to use

- Clone the repo,
- Open .csproj file
- Change S1Dir to your folder (mono and Il2cpp)
- **Read comments in .csproj**

## Linux Build

1. Install prerequisites:

```bash
sudo pacman -S --needed dotnet-sdk
```

2. Make sure Schedule I and MelonLoader are installed, then launch the game once.
   This generates MelonLoader assemblies needed for compile.

3. Find your actual game folder:

```bash
find "$HOME/.steam/steam/steamapps/common" \
   "$HOME/.local/share/Steam/steamapps/common" \
   -maxdepth 1 -type d -iname "schedule*"
```

### Build IL2CPP (main branch):

```bash
dotnet build -c IL2CPP -p:S1Dir="/path/to/Schedule I"
```

### Build MONO (alternate branch):

```bash
dotnet build -c MONO -p:S1Dir="/path/to/Schedule I alternate"
```

## Note

I removed the reference to edgegap due to warnings congesting the console output, also I've never used it in a mod. If you need it paste the following in your csproj with all the other references

```
<Reference Include="Edgegap">
	<HintPath>$(S1Dir)\Schedule I_Data\Managed\Edgegap.dll</HintPath>
</Reference>
```

# Integration Guide

Follow these steps carefully to integrate your mod's settings with the Mod Manager & Phone App.
Ensuring compatibility allows users to configure your mod through both the in-game Phone App and the Main Menu Config panel.

# Core Requirements

1. **MelonPreferences:** Your mod must use the standard `MelonLoader.MelonPreferences` system for its settings. Custom configuration files or methods are not supported by the UI.
   - Use MelonPreferences.CreateCategory("Identifier", "Display Name") to define logical groups for your settings.
   - Use MelonPreferences_Category.CreateEntry<T>("Identifier", defaultValue, "Display Name", "Description", isHidden, ...) to define individual settings. See MelonLoader documentation for all parameters.

2. **DLL Reference:** Your C# project must reference the correct DLL provided with the current version of the Mod Manager & Phone App.
   - Update your project's assembly references in your IDE (Visual Studio, Rider, etc.). Failure to reference the correct DLL will break integration, especially if using the event system or other APIs. Check the Mod Manager's release notes or files if unsure.

3. **Namespace:** If subscribing to events or using specific API calls, ensure you use the correct namespace defined by the Mod Manager.
   - Required Namespace: ModManagerPhoneApp ([CONFIRM Current Namespace: Verify this is correct based on the DLL's code])
   - Example Usage: using ModManagerPhoneApp;

# Category Detection & Naming (Crucial)

The UI identifies settings categories associated with your mod by matching the `Identifier` used in `MelonPreferences.CreateCategory()` against your mod's official `MelonInfo` name.

- How it Works: The manager takes your `MelonInfo` name (e.g., `"My Cool Mod"`) and checks category Identifiers that start with either the exact name (`"My Cool Mod"`) or the name with spaces removed (`"MyCoolMod"`).

- `MelonInfo` Example:

```
[assembly: MelonInfo(typeof(MyModNamespace.MyModClass), "My Cool Mod", "1.0.0", "Your Name")]
```

- Required Prefix: Based on the example above, your category Identifiers must start with either `"My Cool Mod"` or `"MyCoolMod"` to be detected by the UI when filtering by mod.

- Good Identifier Examples (Detected by Mod Filter):

```
// Using space-removed prefix:
var category1 = MelonPreferences.CreateCategory("MyCoolMod_Main", "Main Settings");
var category2 = MelonPreferences.CreateCategory("MyCoolMod_Visuals", "Visual Options");

// Using exact name prefix:
var category3 = MelonPreferences.CreateCategory("My Cool Mod - Audio", "Audio Config");
```

- Bad Identifier Examples (NOT Detected by Mod Filter):

```
var categoryBad1 = MelonPreferences.CreateCategory("Visuals", "Visual Options"); // Doesn't start with required mod name prefix
var categoryBad2 = MelonPreferences.CreateCategory("MCM_Config", "My Config");    // Abbreviation doesn't match required prefix
```

(Note: These categories might still appear in the "All Settings" view, identified by their own Identifier or DisplayName.)

# Category Display & Sorting

- Display Name: Use the DisplayName parameter in CreateCategory() for the user-friendly header shown in the UI (Phone App and Main Menu Config). If omitted, the Identifier is used.
- Sorting Order: Categories are sorted alphabetically by their Identifier. Use numerical prefixes in the Identifier (e.g., 01*, 02*) to enforce a specific display order in the UI.

```
// Identifier controls sorting, DisplayName controls UI text:
var catMain = MelonPreferences.CreateCategory("MyCoolMod_01_Main", "Main Settings");
var catVisuals = MelonPreferences.CreateCategory("MyCoolMod_02_Visuals", "Visual Settings");
var catAudio = MelonPreferences.CreateCategory("MyCoolMod_03_Audio", "Audio Settings");
```

# Settings Entry Display

- DisplayName: Use the DisplayName parameter in CreateEntry<T>() for the user-friendly label shown next to the control. Highly recommended for usability. If omitted, the entry's Identifier is shown.

```
public static MelonPreferences_Entry<float> MovementSpeedPref;
// ...
MovementSpeedPref = category.CreateEntry<float>(
    Identifier: "InternalMoveSpd", // Internal use, fallback label if DisplayName missing
    DefaultValue: 1.0f,
    DisplayName: "Player Movement Speed" // Shown in UI (Phone App and Main Menu Config)
);
```

- Description: The Description parameter in CreateEntry<T>() is currently not displayed in the Mod Manager UI. It remains useful for documentation within MelonPreferences itself or for other tools that might read it.

# Supported Setting Types & UI Controls

The UI provides specific controls based on the preference entry's value type, loaded from prefabs:

- bool: Displayed as a Toggle Switch.
- string: Displayed as a Text Input Field.
- int: Displayed as a Text Input Field (accepts integers).
- float: Displayed as a Text Input Field (accepts decimals).
- double: Displayed as a Text Input Field (accepts decimals).
- UnityEngine.KeyCode: Displayed as a Key Rebind Button.
- System.Enum: Displayed as a Dropdown/ComboBox.
- UnityEngine.Color: Displayed as an Image Preview + RGBA Sliders/Inputs.

- Unsupported Types: Any .NET or Unity types not listed above are currently not displayed in the UI. Ensure your user-facing preferences use one of the supported types.

# Hiding Settings

Use the IsHidden: true parameter in MelonPreferences_Category.CreateEntry<T>() for internal settings that you do not want exposed in the UI. Hidden entries are skipped during UI population.

```
category.CreateEntry<int>("InternalUsageCounter", 0, IsHidden: true);
```
