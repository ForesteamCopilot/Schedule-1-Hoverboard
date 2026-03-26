using MelonLoader;
using UnityEngine;
using HarmonyLib;
using MelonLoader.Utils;
using Hoverboard.TemplateUtils;
using Hoverboard.Factory;
using UnityEngine.Events;
using Hoverboard.Config;
#if IL2CPP
using Il2CppScheduleOne.Persistence;
using Il2CppScheduleOne.Dialogue;
using Il2CppScheduleOne;
using Il2CppScheduleOne.ItemFramework;
using Il2CppScheduleOne.NPCs.CharacterClasses;
using Il2CppScheduleOne.Skating;
#elif MONO
using ScheduleOne.Persistence;
using ScheduleOne.Dialogue;
using ScheduleOne;
using ScheduleOne.ItemFramework;
using ScheduleOne.NPCs.CharacterClasses;
using ScheduleOne.Skating;
#endif

[assembly: MelonInfo(typeof(Hoverboard.Core), Hoverboard.BuildInfo.Name, Hoverboard.BuildInfo.Version, Hoverboard.BuildInfo.Author, Hoverboard.BuildInfo.DownloadLink)]
[assembly: MelonColor(255, 191, 0, 255)]
[assembly: MelonGame("TVGS", "Schedule I")]

namespace Hoverboard
{
    public static class BuildInfo
    {
        public const string Name = "Hoverboard";
        public const string Description = "Adds a hoverboard to the game...because why not?";
        public const string Author = "OverweightUnicorn";
        public const string Company = "UnicornsCanMod";
        public const string Version = "1.0.0";
        public const string DownloadLink = null;
    }

    public class Core : MelonMod
    {

        public override void OnInitializeMelon()
        {
            HoverboardConfig.Initialize();
        }
        public override void OnLateInitializeMelon()
        {
            LoadManager.Instance.onLoadComplete.AddListener((UnityAction)InitMod);
        }

        public override void OnSceneWasLoaded(int buildIndex, string sceneName)
        {

            if (sceneName == "Main" && HoverboardFactory.hoverboardPrefab == null)
            {
                HoverboardFactory.Init();
            }
        }

        public void InitMod()
        {
            var jeff = GameObject.FindObjectOfType<Jeff>();
            if (jeff != null)
            {
                Utility.Success("Found Jeff in the scene");
                GameObject goJeff = jeff.gameObject;
                Transform dialogue = goJeff.transform.Find("Dialogue");
                if (dialogue != null)
                {
                    Utility.Success("Found Dialogue child on Jeff");
                    DialogueController_SkateboardSeller sellerController = dialogue.gameObject.GetComponent<DialogueController_SkateboardSeller>();
                    if (sellerController != null && Registry.ItemExists(HoverboardFactory.ITEM_ID))
                    {
                        Utility.Success("Found seller controller"); 
                        ItemDefinition hoverItemDef = Registry.GetItem<ItemDefinition>(HoverboardFactory.ITEM_ID);
                        var hoverOption = new DialogueController_SkateboardSeller.Option()
                        {
                            Name = "Hoverboard",
                            Price = HoverboardConfig.Price.Value,
                            IsAvailable = hoverItemDef != null,
                            NotAvailableReason = hoverItemDef != null ? "You aren't cool enough" : "Item definition not found",
                            Item = hoverItemDef
                        };

                        sellerController.Options.Add(hoverOption);
                        Utility.Success("Added hoverboard to seller options");
                    }
                    else
                    {
                        Utility.Log("Dialogue Controller doesn't exist");
                    }
                }
                else
                { 
                    Utility.Error("Could not find Dialogue child on Jeff");
                }
            }
        }

        [HarmonyPatch(typeof(SkateboardAudio))]
        public static class SkateboardAudio_Patch
        {

            [HarmonyPrefix]
            [HarmonyPatch(nameof(SkateboardAudio.Start))]
            public static bool StartPrefix(SkateboardAudio __instance)
            {
                if (__instance != null && __instance.transform.parent.name.Contains("Hoverboard"))
                {
                    __instance.RollingAudio.Play();
                    __instance.DirtRollingAudio.Play();
                    __instance.WindAudio.Play();
                }
                return false;
            }

            [HarmonyPrefix]
            [HarmonyPatch(nameof(SkateboardAudio.PlayLand))]
            public static bool PlayLandPrefix(SkateboardAudio __instance)
            {
                if (__instance != null && __instance.transform.parent.name.Contains("Hoverboard"))
                {
                    return false;
                }
                return true;
            }

            [HarmonyPrefix]
            [HarmonyPatch(nameof(SkateboardAudio.PlayJump))]
            public static bool PlayJumpPrefix(SkateboardAudio __instance,float force)
            {
                if (__instance != null && __instance.transform.parent.name.Contains("Hoverboard"))
                {
                    return false;
                }
                return true;
            }


        }
    }


}