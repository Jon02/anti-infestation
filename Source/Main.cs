using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Verse;

namespace AntiInfestation
{
    public class Main : Mod
    {
        public Settings settings;

        public Main(ModContentPack content) : base(content)
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            Log.Message("AntiInfestation v" + assembly.GetName().Version + " successfully loaded!");
            Harmony h = new Harmony("com.jonny02.harmony.rimworld.antiinfestation");
            h.PatchAll(assembly);

            settings = GetSettings<Settings>();
        }

        public override string SettingsCategory()
        {
            return "Anti Infestation";
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            Widgets.Label(new Rect(inRect.x, inRect.y, 300f, 42f), "Infestation Preventer Mood Penalty");
            settings.moodDebuff = Widgets.TextField(new Rect(inRect.x + 320f, inRect.y, 80f, 30f), settings.moodDebuff);
            base.DoSettingsWindowContents(inRect);
        }
    }

    public class Settings : ModSettings
    {
        public string moodDebuff;
        
        public override void ExposeData()
        {
            Scribe_Values.Look(ref moodDebuff, "moodDebuff", "10");
            base.ExposeData();
        }
    }
    
}