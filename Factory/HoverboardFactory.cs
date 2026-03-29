using Hoverboard.Config;
using Hoverboard.TemplateUtils;
#if IL2CPP
using Il2CppScheduleOne;
using Il2CppScheduleOne.AvatarFramework.Equipping;
using Il2CppScheduleOne.Core.Items.Framework;
using Il2CppScheduleOne.DevUtilities;
using Il2CppScheduleOne.Experimental;
using Il2CppScheduleOne.ItemFramework;
using Il2CppScheduleOne.Skating;
using Il2CppScheduleOne.Storage;
using Il2CppScheduleOne.Weather;
#elif MONO
using ScheduleOne;
using ScheduleOne.AvatarFramework.Equipping;
using ScheduleOne.Core.Items.Framework;
using ScheduleOne.DevUtilities;
using ScheduleOne.Experimental;
using ScheduleOne.ItemFramework;
using ScheduleOne.Skating;
using ScheduleOne.Storage;
using ScheduleOne.Weather;
#endif
using UnityEngine;

namespace Hoverboard.Factory
{
    public static class HoverboardFactory
    {
        public const string ITEM_ID = "hoverboard";
        private const string ITEM_NAME = "Hoverboard";
        private const string SOURCE_SKATEBOARD_ID = "goldenskateboard";

        public static GameObject refStorage;
        public static GameObject hoverboardPrefab;
        public static GameObject visualPrefab;

        public static Skateboard hoverSkateboard;
        public static SkateboardVisuals hoverVisuals;
        public static SkateboardEffects hoverEffects;
        private static AvatarEquippable hoverAvatar;
        private static Skateboard_Equippable hoverEquippable;
        private static StoredItem hoverStored;
        private static SkateboardData hoverData;

        private static Sprite hoverIcon;
        public static MeshFilter hoverBoardFilter;
        public static MeshRenderer hoverBoardRenderer;
        public static AudioClip rollingAudio;

        // Original position of trail[0] and the midpoint between all trails,
        // cached once on first call so they never need to be recalculated.
        private static Vector3? _trail0OriginalLocalPosition;
        private static Vector3? _trailMidpointLocalPosition;

        public static void Init()
        {
            refStorage = new GameObject("HoverboardReferenceStorage");
            refStorage.SetActive(false);
            UnityEngine.Object.DontDestroyOnLoad(refStorage);

            try
            {
                LoadCustomAssets();

                if (hoverBoardFilter != null && hoverBoardRenderer != null)
                {
                    hoverSkateboard = CreateSkateboardPrefab();
                    if (hoverSkateboard == null)
                    {
                        Utility.Error("Failed to create Skateboard prefab");
                    }

                    hoverAvatar = CreateAvatarEquippablePrefab();
                    if (hoverAvatar == null)
                    {
                        Utility.Error("Failed to create AvatarEquippable prefab");
                    }

                    hoverEquippable = CreateEquippablePrefab();
                    if (hoverEquippable == null)
                    {
                        Utility.Error("Failed to create Equippable prefab");
                    }

                    hoverStored = CreateStoredPrefab();
                    if (hoverStored == null)
                    {
                        Utility.Error("Failed to create Stored prefab");
                    }

                    CreateVisualPrefab();

                    if (hoverSkateboard != null && hoverAvatar != null && hoverEquippable != null && hoverStored != null)
                    {
                        hoverEquippable.AvatarEquippable = hoverAvatar;
                        hoverEquippable.SkateboardPrefab = hoverSkateboard;
                        hoverSkateboard.Equippable = hoverEquippable;
                        hoverSkateboard.SlowOnTerrain = false;
                        var skateData = UnityEngine.ScriptableObject.Instantiate(hoverSkateboard._defaultData,refStorage.transform);

                        if (skateData != null) { 
                            hoverData = skateData;
                            hoverSkateboard._defaultData = skateData;
                        }

                        ApplyHoverSettings();

                        NeutralizeRainOverride();

                        SkateboardAudio audioController = hoverSkateboard.gameObject.GetComponentInChildren<SkateboardAudio>();
                        if (audioController != null)
                        {
                            AudioSource rollingFx = audioController.transform.Find("Audio Source (Skateboard rolling, FX)")?.GetComponent<AudioSource>();
                            AudioSource dirtRollingFx = audioController.transform.Find("Audio Source (Skateboard rolling, FX) (1)")?.GetComponent<AudioSource>();

                            AudioSource jumpFx = audioController.transform.Find("Audio Source (, FX)")?.GetComponent<AudioSource>();
                            AudioSource landFx = audioController.transform.Find("Audio Source (, FX)")?.GetComponent<AudioSource>();

                            if (audioController.JumpAudio != null)
                            {
                                hoverSkateboard.OnJump.RemoveAllListeners();
                                AudioClip jumpClip = audioController.JumpAudio.gameObject.GetComponent<AudioSource>().clip;
                                audioController.JumpAudio.gameObject.SetActive(false);
                                if (jumpClip == null)
                                {
                                    audioController.JumpAudio.gameObject.GetComponent<AudioSource>().clip = null;
                                }
                            }

                            if (audioController.LandAudio != null)
                            {
                                audioController.LandAudio.gameObject.SetActive(false);
                            }

                            if (rollingAudio != null)
                            {

                                if (rollingFx != null)
                                {
                                    rollingFx.clip = rollingAudio;
                                }
                                else
                                {
                                    Utility.Error("Rolling FX is NULL");
                                }

                                if (dirtRollingFx != null)
                                {
                                    dirtRollingFx.clip = rollingAudio;
                                    Utility.Log("Dirt rolling audio clip assigned successfully");
                                }
                                else
                                {
                                    Utility.Error("audioController.DirtRollingAudio is NULL");
                                }
                            }
                            else
                            {
                                Utility.Error("rollingAudio clip is NULL");
                            }
                        }
                        else
                        {
                            Utility.Error("Audio controller is NULL");
                        }

                        Utility.Log("11. Configuring Visuals");
                        SkateboardVisuals visuals = hoverSkateboard.gameObject.GetComponentInChildren<SkateboardVisuals>();
                        if (visuals != null)
                        {
                            hoverVisuals = visuals;
                            hoverVisuals.MaxBoardLean = HoverboardConfig.MaxBoardLean.Value;
                            hoverVisuals.BoardLeanRate = HoverboardConfig.BoardLeanRate.Value;
                        }

                        Utility.Log("12. Configuring Effects");
                        SkateboardEffects effects = hoverSkateboard.gameObject.GetComponentInChildren<SkateboardEffects>();
                        if (effects != null)
                        {
                            hoverEffects = effects;
                            ApplyTrailSettings();
                        }

                        Utility.Log("13. Creating Storable");
                        CreateStorableDefinition();
                    }
                    else
                    {
                        Utility.Error("Failed to create one or more hoverboard prefabs - initialization incomplete");
                    }
                }
                else
                {
                    if (hoverBoardFilter == null)
                    {
                        Utility.Error("HoverBoardFilter is null - failed to load mesh");
                    }
                    if (hoverBoardRenderer == null)
                    {
                        Utility.Error("HoverBoardRenderer is null - failed to load renderer");
                    }
                    Utility.Error("Initialization aborted due to missing mesh or renderer");
                }
            }
            catch (Exception ex)
            {
                Utility.Error("=== CRITICAL ERROR during Hoverboard Factory Initialization ===");
                Utility.Log(ex.ToString());
            }
        }

        /// <summary>
        /// Returns the live SkateboardSettings from _defaultData via reflection.
        /// Writing to this object persists across weather changes since OnWeatherChange clones from it.
        /// </summary>
        public static SkateboardSettings GetDefaultSettings()
        {
            if (hoverSkateboard == null) return null;

            if (hoverSkateboard.DefaultSettings == null) return null;
            return hoverSkateboard.DefaultSettings;
        }

        /// <summary>
        /// Writes all hover-related config values into _defaultData.Settings so they survive
        /// weather changes, then triggers a settings refresh via OnWeatherChange with clear skies.
        /// </summary>
        public static void ApplyHoverSettings()
        {
            SkateboardSettings settings = GetDefaultSettings();
            if (settings == null)
            {
                Utility.Error("ApplyHoverSettings: could not get default settings");
                return;
            }

            settings.HoverHeight = HoverboardConfig.HoverHeight.Value;
            settings.TopSpeed_Kmh = HoverboardConfig.TopSpeed.Value;
            settings.HoverRayLength = HoverboardConfig.HoverHeight.Value + 0.05f;
            settings.Hover_P = HoverboardConfig.Proportional.Value;
            settings.Hover_I = HoverboardConfig.Integral.Value;
            settings.Hover_D = HoverboardConfig.Derivative.Value;

            // Refresh _settings from the updated _defaultData by simulating a weather update
            // with no rain so no rain blend is applied
            RefreshActiveSettings();

            Utility.Log($"ApplyHoverSettings: HoverHeight={settings.HoverHeight}, P={settings.Hover_P}, I={settings.Hover_I}, D={settings.Hover_D}");
        }

        /// <summary>
        /// Triggers OnWeatherChange with a clear-sky WeatherConditions so _settings is rebuilt
        /// from _defaultData without any rain blend.
        /// </summary>
        public static void RefreshActiveSettings()
        {
            if (hoverSkateboard == null) return;
            hoverSkateboard.OnWeatherChange(new WeatherConditions());
        }

        /// <summary>
        /// Replaces _rainOverrideData with a new SkateboardOverrideData that has all hover
        /// flags excluded, preventing rain from affecting hover height, force, or PID values.
        /// </summary>
        private static void NeutralizeRainOverride()
        {
            if (hoverSkateboard == null) return;

            if (hoverSkateboard._rainOverrideData == null)
            {
                Utility.Error("NeutralizeRainOverride: _rainOverrideData is null on hoverSkateboard - cannot neutralize rain override");
                return;
            }

            // Create a new override asset with no hover flags set - all hover fields will use
            // -1 sentinel in Blend(), which the game treats as "don't override" (multiplier stays 1)
            SkateboardOverrideData neutralOverride = ScriptableObject.CreateInstance<SkateboardOverrideData>();
            neutralOverride.Categories =
               SkateboardOverrideData.OverrideCategory.Turning |
                 SkateboardOverrideData.OverrideCategory.Friction;
            // Hover category intentionally omitted - no hover flags

            neutralOverride.Settings = new SkateboardSettings
            {
                // Sentinel -1 = "no override" in Blend()
                TurnForce = -1f,
                TurnChangeRate = -1f,
                TurnReturnToRestRate = -1f,
                TurnSpeedBoost = -1f,
                Gravity = -1f,
                BrakeForce = -1f,
                ReverseTopSpeed_Kmh = -1f,
                RotationClampForce = -1f,
                LongitudinalFrictionMultiplier = 0.8f, // rain makes surface slippery
                LateralFrictionForceMultiplier = 0.8f,
                JumpForce = -1f,
                JumpDuration_Min = -1f,
                JumpDuration_Max = -1f,
                JumpForwardBoost = -1f,
                // All hover fields use -1 so Blend() leaves them untouched
                HoverForce = -1f,
                HoverRayLength = -1f,
                HoverHeight = -1f,
                Hover_P = -1f,
                Hover_I = -1f,
                Hover_D = -1f,
                TopSpeed_Kmh = -1f,
                PushForceMultiplier = -1f,
                PushForceDuration = -1f,
                PushDelay = -1f,
                AirMovementForce = -1f,
                AirMovementJumpReductionDuration = -1f,
            };

            hoverSkateboard._rainOverrideData = neutralOverride;
            Utility.Log("NeutralizeRainOverride: rain override replaced - hover fields protected");
        }

        /// <summary>
        /// Applies all trail settings from config onto the prefab's SkateboardEffects.
        /// Called once during Init - changes are baked into the prefab so every
        /// subsequent instantiation inherits them automatically via Awake().
        /// </summary>
        public static void ApplyTrailSettings()
        {
            if (hoverEffects == null)
            {
                Utility.Error("ApplyTrailSettings: hoverEffects is null");
                return;
            }

            TrailRenderer[] trails = hoverEffects.Trails;
            if (trails == null || trails.Length == 0)
            {
                Utility.Error("ApplyTrailSettings: no trails found on hoverEffects");
                return;
            }

            // Cache original position and midpoint once - these never change after Init
            if (_trail0OriginalLocalPosition == null)
            {
                _trail0OriginalLocalPosition = trails[0].transform.localPosition;

                Vector3 midpoint = Vector3.zero;
                for (int i = 0; i < trails.Length; i++)
                    midpoint += trails[i].transform.localPosition;
                midpoint /= trails.Length;
                _trailMidpointLocalPosition = midpoint;
            }

            int count = Mathf.Clamp(HoverboardConfig.TrailCount.Value, 0, trails.Length);
            float width = HoverboardConfig.TrailWidth.Value;
            float spread = HoverboardConfig.TrailSpread.Value;

            if (count == 0)
            {
                // Hide all trails
                foreach (var t in trails)
                    t.gameObject.SetActive(false);
            }
            else if (count == 1)
            {
                // Single trail centred at midpoint, second hidden
                trails[0].gameObject.SetActive(true);
                trails[0].transform.localPosition = _trailMidpointLocalPosition.Value;
                ApplyTrailAppearance(trails[0], width, HoverboardConfig.TrailColors[0].Value);

                for (int i = 1; i < trails.Length; i++)
                    trails[i].gameObject.SetActive(false);
            }
            else
            {
                // Dual trails: trail[0] back at original, trail[1] mirrored, both visible
                // Spread symmetrically around the midpoint X
                float centreX = _trailMidpointLocalPosition.Value.x;
                float halfSpread = spread / 2f;

                trails[0].gameObject.SetActive(true);
                trails[0].transform.localPosition = _trail0OriginalLocalPosition.Value;
                ApplyTrailAppearance(trails[0], width, HoverboardConfig.TrailColors[0].Value);

                if (trails.Length > 1)
                {
                    trails[1].gameObject.SetActive(true);
                    // Mirror trail[0]'s X offset around the centre
                    float trail0OffsetFromCentre = _trail0OriginalLocalPosition.Value.x - centreX;
                    Vector3 trail1Pos = _trailMidpointLocalPosition.Value;
                    trail1Pos.x = centreX - trail0OffsetFromCentre;
                    // Apply spread on top
                    trails[0].transform.localPosition = new Vector3(
                centreX - halfSpread,
               _trail0OriginalLocalPosition.Value.y,
                  _trail0OriginalLocalPosition.Value.z);
                    trails[1].transform.localPosition = new Vector3(
             centreX + halfSpread,
                  trail1Pos.y,
           trail1Pos.z);
                    ApplyTrailAppearance(trails[1], width, HoverboardConfig.TrailColors[1].Value);
                }
            }
        }

        /// <summary>
        /// Applies width and color to a single TrailRenderer.
        /// Alpha is preserved from the prefab so SkateboardEffects speed-fade still works.
        /// </summary>
        private static void ApplyTrailAppearance(TrailRenderer trail, float width, Color color)
        {
            trail.startWidth = width;
            trail.endWidth = 0f;

            float existingAlpha = trail.startColor.a;
            trail.startColor = new Color(color.r, color.g, color.b, existingAlpha);
            trail.endColor = new Color(color.r, color.g, color.b, 0f);
        }


        public static void LoadCustomAssets()
        {
            AssetBundleUtils.LoadAssetBundle("hoverboard");

            GameObject hoverboardAsset = AssetBundleUtils.LoadAssetFromBundle<GameObject>("hoverboard.prefab", "hoverboard");
            if (hoverboardAsset != null && hoverboardPrefab == null)
            {
                hoverboardPrefab = GameObject.Instantiate(hoverboardAsset, refStorage.transform);
                UnityEngine.Object.DontDestroyOnLoad(hoverboardPrefab);
            }
            else
            {
                if (hoverboardAsset == null)
                {
                    Utility.Error("Failed to load hoverboard prefab from asset bundle - asset is null");
                }
                else if (hoverboardPrefab != null)
                {
                    Utility.Error("Hoverboard prefab already exists - skipping instantiation");
                }
                return;
            }

            Sprite icon = AssetBundleUtils.LoadAssetFromBundle<Sprite>("hoverboardvisual_icon.png", "hoverboard");
            if (icon != null)
            {
                hoverIcon = icon;
                UnityEngine.Object.DontDestroyOnLoad(hoverIcon);
            }
            else
            {
                Utility.Error("Failed to load icon sprite - icon will be missing");
            }

            AudioClip rollingSfx = AssetBundleUtils.LoadAssetFromBundle<AudioClip>("hoverloop1.mp3", "hoverboard");
            if (rollingSfx != null)
            {
                rollingAudio = rollingSfx;
                UnityEngine.Object.DontDestroyOnLoad(rollingAudio);
            }
            else
            {
                Utility.Error("Failed to load jump audio 1 clip - jump sound will be missing");
            }

            var mesh = hoverboardPrefab.GetComponent<MeshFilter>();
            if (mesh != null)
            {
                hoverBoardFilter = mesh;
            }
            else
            {
                Utility.Error("MeshFilter component not found on hoverboard prefab");
            }

            var renderer = hoverboardPrefab.GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                hoverBoardRenderer = renderer;
            }
            else
            {
                Utility.Error("MeshRenderer component not found on hoverboard prefab");
            }


        }

        private static void HideTrucks(Transform prefab, string path)
        {
            Transform boardContainer = prefab.Find(path);
            if (boardContainer == null) return;

            if (boardContainer != null)
            {
                Transform truck1 = boardContainer.Find("Truck");
                if (truck1 != null)
                {
                    truck1.gameObject.SetActive(false);
                }
                else
                {
                    Utility.Error("Truck (1) not found in BoardContainer");
                }

                Transform truck2 = boardContainer.Find("Truck (1)");
                if (truck2 != null)
                {
                    truck2.gameObject.SetActive(false);
                }
                else
                {
                    Utility.Error("Truck (2) not found in BoardContainer");
                }
            }
            else
            {
                Utility.Error("BoardContainer not found - unable to hide trucks");
            }

        }

        public static Skateboard CreateSkateboardPrefab()
        {
            GameObject prefab = Resources.Load<GameObject>("skateboards/goldenskateboard/GoldSkateboard");
            var skateboardPrefab = GameObject.Instantiate(prefab, refStorage.transform);
            skateboardPrefab.name = "Hoverboard";
            
            // Hide the trucks since hoverboards don't need them
            HideTrucks(skateboardPrefab.transform, "Model/Skateboard");

            // Replace the board mesh and materials
            Transform container = skateboardPrefab.transform.Find("Model/Skateboard/BoardContainer/Board");
            if (container != null)
            {
                container.GetComponent<MeshFilter>().mesh = hoverBoardFilter.mesh;
                container.GetComponent<MeshRenderer>().materials = hoverBoardRenderer.materials;

                // Check for nested BoardModel
                Transform boardModel = container.Find("BoardModel");
                if (boardModel != null)
                {
                    boardModel.GetComponent<MeshFilter>().mesh = hoverBoardFilter.mesh;
                    boardModel.GetComponent<MeshRenderer>().materials = hoverBoardRenderer.materials;

                    // Check for deeply nested visual
                    Transform boardVisual = boardModel.Find("GoldSkateboardVisual/GoldSkateboard/BoardContainer/Board");
                    if (boardVisual != null)
                    {
                        boardVisual.GetComponent<MeshFilter>().mesh = hoverBoardFilter.mesh;
                        boardVisual.GetComponent<MeshRenderer>().materials = hoverBoardRenderer.materials;
                    }
                }
                return skateboardPrefab.GetComponent<Skateboard>();
            }

            return null;
        }

        public static AvatarEquippable CreateAvatarEquippablePrefab()
        {
            GameObject prefab = Resources.Load<GameObject>("skateboards/goldenskateboard/GoldSkateboard_AvatarEquippable");
            var avatarEquippablePrefab = GameObject.Instantiate(prefab, refStorage.transform);
            avatarEquippablePrefab.name = "Hoverboard_AvatarEquippable";

            // Hide the trucks since hoverboards don't need them
            HideTrucks(avatarEquippablePrefab.transform, "GoldSkateboardVisual/GoldSkateboard");

            Transform container = avatarEquippablePrefab.transform.Find("GoldSkateboardVisual/GoldSkateboard/BoardContainer/Board");
            if (container != null)
            {
                container.GetComponent<MeshFilter>().mesh = hoverBoardFilter.mesh;
                container.GetComponent<MeshRenderer>().materials = hoverBoardRenderer.materials;
                return avatarEquippablePrefab.GetComponent<AvatarEquippable>();
            }

            return null;
        }

        public static Skateboard_Equippable CreateEquippablePrefab()
        {
            GameObject prefab = Resources.Load<GameObject>("skateboards/goldenskateboard/GoldSkateboard_Equippable");
            var equippablePrefab = GameObject.Instantiate(prefab, refStorage.transform);
            equippablePrefab.name = "Hoverboard_Equippable";

            var equippableComponent = equippablePrefab.GetComponent<Skateboard_Equippable>();

            HideTrucks(equippablePrefab.transform, "GoldSkateboardVisual/GoldSkateboard");

            equippableComponent.SkateboardPrefab = null;

            Transform container = equippablePrefab.transform.Find("GoldSkateboardVisual/GoldSkateboard/BoardContainer/Board");
            if (container != null)
            {
                container.GetComponent<MeshFilter>().mesh = hoverBoardFilter.mesh;
                container.GetComponent<MeshRenderer>().materials = hoverBoardRenderer.materials;
                return equippableComponent;
            }

            return null;
        }

        public static StoredItem CreateStoredPrefab()
        {
            GameObject prefab = Resources.Load<GameObject>("skateboards/goldenskateboard/GoldSkateboard_Stored");
            var storedPrefab = GameObject.Instantiate(prefab, refStorage.transform);
            storedPrefab.name = "Hoverboard_Stored";

            HideTrucks(storedPrefab.transform, "GoldSkateboardVisual/GoldSkateboard");

            Transform container = storedPrefab.transform.Find("GoldSkateboardVisual/GoldSkateboard/BoardContainer/Board");
            if (container != null)
            {
                container.GetComponent<MeshFilter>().mesh = hoverBoardFilter.mesh;
                container.GetComponent<MeshRenderer>().materials = hoverBoardRenderer.materials;
                return storedPrefab.GetComponent<StoredItem>();
            }

            return null;
        }

        public static StorableItemDefinition CreateStorableDefinition()
        {
            ItemDefinition itemDef = Registry.GetItem<ItemDefinition>(SOURCE_SKATEBOARD_ID);
            if (itemDef != null)
            {
#if IL2CPP
                StorableItemDefinition baseDef = itemDef.TryCast<StorableItemDefinition>();
#elif MONO
                StorableItemDefinition baseDef = itemDef as StorableItemDefinition;
#endif
                StorableItemDefinition hoverDef = UnityEngine.Object.Instantiate(baseDef);
                if (hoverDef != null)
                {
                    hoverDef.ID = ITEM_ID;
                    hoverDef.name = ITEM_NAME;
                    hoverDef.Name = ITEM_NAME;
                    hoverDef.Description = "A futuristic skateboard that hovers above the ground.";
                    hoverDef.StackLimit = 1;
                    hoverDef.Equippable = hoverEquippable;
                    hoverDef.StoredItem = hoverStored;
                    hoverDef.legalStatus = ELegalStatus.Legal;
                    hoverDef.Category = EItemCategory.Tools;
                    hoverDef.AvailableInDemo = true;
                    hoverDef.BasePurchasePrice = HoverboardConfig.Price.Value;
                    hoverDef.ResellMultiplier = HoverboardConfig.ResellMultiplier.Value;

                    if (hoverIcon != null)
                    {
                        hoverDef.Icon = hoverIcon;
                    }

                    Singleton<Registry>.Instance.AddToRegistry(hoverDef);
                    return hoverDef;
                }
            }

            return null;
        }

        public static void CreateVisualPrefab()
        {
            GameObject prefab = Resources.Load<GameObject>("skateboards/goldenskateboard/GoldSkateboardVisual");
            visualPrefab = GameObject.Instantiate(prefab, refStorage.transform);
            visualPrefab.name = "Hoverboard_Visuals";
            Transform container = visualPrefab.transform.Find("GoldSkateboard/BoardContainer/Board");
            if (container != null)
            {
                container.GetComponent<MeshFilter>().mesh = hoverBoardFilter.mesh;
                container.GetComponent<MeshRenderer>().materials = hoverBoardRenderer.materials;
            }
        }

        /// <summary>
        /// Resets all scene-bound state so Init() can run cleanly on the next load.
        /// Called when returning to the Menu scene.
        /// Asset references (hoverboardPrefab, hoverBoardFilter, hoverBoardRenderer,
        /// rollingAudio, hoverIcon) are DontDestroyOnLoad and intentionally preserved.
        /// Reflection field caches are type-bound and intentionally preserved.
        /// </summary>
        public static void Reset()
        {
            // Scene-instantiated GameObjects - destroyed on scene unload
            refStorage = null;
            visualPrefab = null;

            // Component references that live on scene-instantiated objects
            hoverboardPrefab = null;
            hoverSkateboard = null;
            hoverVisuals = null;
            hoverEffects = null;
            hoverAvatar = null;
            hoverEquippable = null;
            hoverStored = null;

            // Trail position cache is read from the prefab's trail transforms.
            // Must be cleared so it is re-read from the fresh prefab on next Init().
            _trail0OriginalLocalPosition = null;
            _trailMidpointLocalPosition = null;

            Utility.Log("HoverboardFactory.Reset: scene state cleared");
        }
    }
}
