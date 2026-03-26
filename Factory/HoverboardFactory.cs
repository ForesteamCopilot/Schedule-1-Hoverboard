using Hoverboard.Config;
using Hoverboard.TemplateUtils;
#if IL2CPP
using Il2CppScheduleOne;
using Il2CppScheduleOne.Audio;
using Il2CppScheduleOne.AvatarFramework.Equipping;
using Il2CppScheduleOne.Core.Items.Framework;
using Il2CppScheduleOne.DevUtilities;
using Il2CppScheduleOne.Equipping;
using Il2CppScheduleOne.ItemFramework;
using Il2CppScheduleOne.Skating;
using Il2CppScheduleOne.Storage;
#elif MONO
using ScheduleOne;
using ScheduleOne.Audio;
using ScheduleOne.AvatarFramework.Equipping;
using ScheduleOne.Core.Items.Framework;
using ScheduleOne.DevUtilities;
using ScheduleOne.Equipping;
using ScheduleOne.ItemFramework;
using ScheduleOne.Skating;
using ScheduleOne.Storage;
#endif
using MelonLoader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

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

        private static Sprite hoverIcon;
        public static MeshFilter hoverBoardFilter;
        public static MeshRenderer hoverBoardRenderer;
        public static AudioClip rollingAudio;

        public static void Init()
        {
            Utility.Log("Initializing Hoverboard Factory...");
            refStorage = new GameObject("HoverboardReferenceStorage");
            refStorage.SetActive(false);
            UnityEngine.Object.DontDestroyOnLoad(refStorage);

            try
            {
                Utility.Log("2. Loading Custom Assets");
                LoadCustomAssets();
                Utility.Log("3. Custom Assets Loaded");
                if (hoverBoardFilter != null && hoverBoardRenderer != null)
                {
                    Utility.Log("4. Creating Prefabs");
                    hoverSkateboard = CreateSkateboardPrefab();
                    if (hoverSkateboard == null)
                    {
                        Utility.Error("Failed to create Skateboard prefab");
                    }

                    Utility.Log("5. Skateboard Prefab Created");
                    hoverAvatar = CreateAvatarEquippablePrefab();
                    if (hoverAvatar == null)
                    {
                        Utility.Error("Failed to create AvatarEquippable prefab");
                    }

                    Utility.Log("6. AvatarEquippable Prefab Created");
                    hoverEquippable = CreateEquippablePrefab();
                    if (hoverEquippable == null)
                    {
                        Utility.Error("Failed to create Equippable prefab");
                    }

                    Utility.Log("7. Equippable Prefab Created");
                    hoverStored = CreateStoredPrefab();
                    if (hoverStored == null)
                    {
                        Utility.Error("Failed to create Stored prefab");
                    }

                    CreateVisualPrefab();
                    Utility.Log("8. Visual Prefab Created");
                    if (hoverSkateboard != null && hoverAvatar != null && hoverEquippable != null && hoverStored != null)
                    {
                        Utility.Log("9. Configuring Prefabs");
                        hoverEquippable.AvatarEquippable = hoverAvatar;
                        hoverEquippable.SkateboardPrefab = hoverSkateboard;
                        hoverSkateboard.Equippable = hoverEquippable;
                        hoverSkateboard.HoverHeight = HoverboardConfig.HoverHeight.Value;
                        hoverSkateboard.HoverRayLength = HoverboardConfig.HoverHeight.Value + 0.05f;
                        hoverSkateboard.SlowOnTerrain = false;
                        hoverSkateboard.Hover_P = HoverboardConfig.Proportional.Value;
                        hoverSkateboard.Hover_I = HoverboardConfig.Integral.Value;
                        hoverSkateboard.Hover_D = HoverboardConfig.Derivative.Value;

                        Utility.Log("10. Configuring Audio");
                        SkateboardAudio audioController = hoverSkateboard.gameObject.GetComponentInChildren<SkateboardAudio>();
                        if (audioController != null)
                        {
                            Utility.Log("Audio controller found");

                            if (rollingAudio != null)
                            {
                                Utility.Log("Rolling audio clip is not null");

                                if (audioController.RollingAudio != null)
                                {
                                    Utility.Log("RollingAudio component exists");

                                    if (audioController.RollingAudio._audioSource != null)
                                    {
                                        Utility.Log("RollingAudio._audioSource exists");
                                        audioController.RollingAudio._audioSource.clip = rollingAudio;
                                        Utility.Log("Rolling audio clip assigned successfully");
                                    }
                                    else
                                    {
                                        Utility.Error("RollingAudio._audioSource is NULL");
                                    }
                                }
                                else
                                {
                                    Utility.Error("audioController.RollingAudio is NULL");
                                }

                                if (audioController.DirtRollingAudio != null)
                                {
                                    Utility.Log("DirtRollingAudio component exists");

                                    if (audioController.DirtRollingAudio._audioSource != null)
                                    {
                                        Utility.Log("DirtRollingAudio._audioSource exists");
                                        audioController.DirtRollingAudio._audioSource.clip = rollingAudio;
                                        Utility.Log("Dirt rolling audio clip assigned successfully");
                                    }
                                    else
                                    {
                                        Utility.Error("DirtRollingAudio._audioSource is NULL");
                                    }
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

        public static void LoadCustomAssets()
        {
            AssetBundleUtils.LoadAssetBundle("hoverboard");
            //AssetBundleUtils.ListBundleContents("hoverboard");

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

        public static Skateboard CreateSkateboardPrefab()
        {
            GameObject prefab = Resources.Load<GameObject>("skateboards/goldenskateboard/GoldSkateboard");
            var skateboardPrefab = GameObject.Instantiate(prefab, refStorage.transform);
            skateboardPrefab.name = "Hoverboard";

            // Hide the trucks since hoverboards don't need them
            Transform boardContainer = skateboardPrefab.transform.Find("Model/Skateboard");
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

            // ⚠️ CRITICAL: Clear the serialized reference first
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
                    //hoverDef.Keywords = new string[] { "hover", "skateboard", "futuristic" };
                    hoverDef.ResellMultiplier = HoverboardConfig.ResellMultiplier.Value;
                    //hoverDef.LabelDisplayColor = new Color(0f, 0.8f, 1f);

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
    }
}
