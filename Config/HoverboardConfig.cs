using Hoverboard.Factory;
using MelonLoader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Hoverboard.Config
{
    public static class HoverboardConfig
    {
        private static MelonPreferences_Category _category;

        public static MelonPreferences_Entry<float> Price;
        public static MelonPreferences_Entry<float> ResellMultiplier;
        public static MelonPreferences_Entry<float> HoverHeight;
        public static MelonPreferences_Entry<float> TopSpeed;
        public static MelonPreferences_Entry<float> Proportional;
        public static MelonPreferences_Entry<float> Integral;
        public static MelonPreferences_Entry<float> Derivative;
        public static MelonPreferences_Entry<float> MaxBoardLean;
        public static MelonPreferences_Entry<float> BoardLeanRate;

        // Trail settings
        public static MelonPreferences_Entry<int> TrailCount;
        public static MelonPreferences_Entry<float> TrailWidth;
        public static MelonPreferences_Entry<float> TrailSpread;
        public static MelonPreferences_Entry<Color>[] TrailColors;

        // Maximum number of trail slots the skateboard prefab supports
        private const int MaxTrails = 2;

        public static void Initialize()
        {
            _category = MelonPreferences.CreateCategory("Hoverboard");
            Price = _category.CreateEntry("Price", 2000f, "(Requires Save Reload) The price of the hoverboard in the in-game shop.\nDefault: 2000");
            ResellMultiplier = _category.CreateEntry("Resell Multiplier", 0.6f, "(Requires Save Reload) The resell price multiplier.\nDefault: 0.6");

            HoverHeight = _category.CreateEntry("Hover Height", 2.0f, "The height at which the hoverboard hovers above the ground.\nDefault: 2");
            HoverHeight.OnEntryValueChanged.Subscribe((oldValue, newValue) =>
            {
                var s = HoverboardFactory.GetDefaultSettings();
                if (s != null)
                {
                    s.HoverHeight = newValue;
                    s.HoverRayLength = newValue + 0.05f;
                    HoverboardFactory.RefreshActiveSettings();
                }
            });

            TopSpeed = _category.CreateEntry("Top Speed", 32.0f, "The maximum speed of the hoverboard.\nDefault: 32");
            TopSpeed.OnEntryValueChanged.Subscribe((oldValue, newValue) =>
            {
                var s = HoverboardFactory.GetDefaultSettings();
                if (s != null)
                {
                    s.TopSpeed_Kmh = newValue;
                    HoverboardFactory.RefreshActiveSettings();
                }
            });

            MaxBoardLean = _category.CreateEntry("Max Board Lean", 8f, "The maximum angle the board leans when turning.\nDefault: 8");
            MaxBoardLean.OnEntryValueChanged.Subscribe((oldValue, newValue) =>
            {
                if (HoverboardFactory.hoverVisuals != null)
                    HoverboardFactory.hoverVisuals.MaxBoardLean = newValue;
            });

            BoardLeanRate = _category.CreateEntry("Board Lean Rate", 2f, "How quickly the board leans when turning.\nDefault: 2");
            BoardLeanRate.OnEntryValueChanged.Subscribe((oldValue, newValue) =>
            {
                if (HoverboardFactory.hoverVisuals != null)
                    HoverboardFactory.hoverVisuals.BoardLeanRate = newValue;
            });

            Proportional = _category.CreateEntry("Proportional", 2.7f, "How strongly the board reacts to height errors.\nHigher = Snappier response | Lower = Sluggish response\nDefault: 2.7");
            Proportional.OnEntryValueChanged.Subscribe((oldValue, newValue) =>
            {
                var s = HoverboardFactory.GetDefaultSettings();
                if (s != null)
                {
                    s.Hover_P = newValue;
                    HoverboardFactory.RefreshActiveSettings();
                }
            });

            Integral = _category.CreateEntry("Integral", 0.1f, "How much the board corrects over time to reach exact height.\nHigher = Rigid, locked height | Lower = Floaty, drifty feel\nDefault: 0.1");
            Integral.OnEntryValueChanged.Subscribe((oldValue, newValue) =>
            {
                var s = HoverboardFactory.GetDefaultSettings();
                if (s != null)
                {
                    s.Hover_I = newValue;
                    HoverboardFactory.RefreshActiveSettings();
                }
            });

            Derivative = _category.CreateEntry("Derivative", 0.5f, "How much the board resists sudden height changes.\nHigher = Smooth over bumps, less bounce | Lower = Bouncy, reactive\nDefault: 0.5");
            Derivative.OnEntryValueChanged.Subscribe((oldValue, newValue) =>
            {
                var s = HoverboardFactory.GetDefaultSettings();
                if (s != null)
                {
                    s.Hover_D = newValue;
                    HoverboardFactory.RefreshActiveSettings();
                }
            });

            // --- Trail Settings ---
            TrailCount = _category.CreateEntry("Trail Count", 2, "Number of active trails. 0 = none, 2 = max");
            TrailCount.OnEntryValueChanged.Subscribe((oldValue, newValue) => HoverboardFactory.ApplyTrailSettings());

            TrailWidth = _category.CreateEntry("Trail Width", 0.05f, "Width of each trail renderer.\n Default: 0.05");
            TrailWidth.OnEntryValueChanged.Subscribe((oldValue, newValue) => HoverboardFactory.ApplyTrailSettings());

            TrailSpread = _category.CreateEntry("Trail Spread", 0.15f, "Lateral spacing between trails when using multiple.\n0 = stacked on centre, higher = further apart.\n Default: 0.15");
            TrailSpread.OnEntryValueChanged.Subscribe((oldValue, newValue) => HoverboardFactory.ApplyTrailSettings());

            // Per-trail hex color entries
            Color[] defaultColors = { Color.white, Color.white };
            TrailColors = new MelonPreferences_Entry<Color>[MaxTrails];
            for (int i = 0; i < MaxTrails; i++)
            {
                int index = i; // capture for lambda
                TrailColors[i] = _category.CreateEntry(
                    $"Trail {i + 1} Color",
                    defaultColors[i],
                    $"Color for trail {i + 1}.");
                TrailColors[i].OnEntryValueChanged.Subscribe((oldValue, newValue) => HoverboardFactory.ApplyTrailSettings());
            }
        }
    }
}
