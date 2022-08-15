using System;
using System.ComponentModel;
using AntiInfestation.Buildings;
using RimWorld;
using UnityEngine;
using UnityEngine.U2D;
using Verse;

namespace AntiInfestation.Dialogs
{
    [StaticConstructorOnStartup]
    public class Dialog_InfestationTerminal : Window
    {
        
        private Building_InfestationSpawner spawner;
        private PsychicWaveStrength strength;

        public static readonly Texture2D CloseXSmall = ContentFinder<Texture2D>.Get("UI/Widgets/CloseXSmall", true);
        
        public override Vector2 InitialSize => new Vector2(640f, 460f);
        private TerminalStatus status;

        public float fillPercent;
        
        public TerminalStatus TerminalStatus
        {
            get => status;
            set => status = value;
        }

        public PsychicWaveStrength GetStrength => strength;

        private TaggedString GetStatus()
        {
            if (!spawner.IsPositionSet) return "PositionNotSet".Translate();
            
            if (status == TerminalStatus.POSITIONING) return "PositioningSatellite".Translate();
            
            status = TerminalStatus.READY_TO_ATTACK;
            return "ReadyToAttack".Translate();
        }

        public Dialog_InfestationTerminal(Building_InfestationSpawner spawner)
        {
            this.spawner = spawner;
            
            strength = PsychicWaveStrength.WEAK;
            status = TerminalStatus.NOT_READY_TO_ATTACK;
            fillPercent = 0f;
        }
        
        public override void DoWindowContents(Rect inRect)
        {
            if(!spawner.IsOperational) Close();
            
            Text.Font = GameFont.Medium;
            Widgets.Label(new Rect(inRect.x, inRect.y, inRect.width, 42f), "Satellite Terminal");
            Widgets.DrawLine(new Vector2(inRect.x, 35f), new Vector2(inRect.x + inRect.width, 35f), Color.gray, 2f);

            Text.Font = GameFont.Small;
            if (Widgets.ButtonImage(new Rect(inRect.xMax - 20f, inRect.y, 20f, 20f), CloseXSmall, true))
            {
                Close();
            }

            if (!spawner.IsPositionSet)
            {
                if (Widgets.ButtonText(new Rect(inRect.x, 60f, 250f, 30f), "Set coordinates", true, true, true))
                {
                    Close();
                    spawner.SelectTargetOnWorldMap();
                }
            }
            else
            {
                Rect rect = new Rect(inRect.x, 65f, 300f, 30f);
                Widgets.Label(rect, "Position set to faction: " + spawner.CurrentMap.ParentFaction.Name);
                
                if (status != TerminalStatus.POSITIONING)
                {
                    if (Widgets.ButtonText(new Rect(inRect.xMax - 150f, rect.y - 5f, 150f, rect.height), "Set new coordinates", true, true, true))
                    {
                        Close();
                        spawner.SelectTargetOnWorldMap();
                    }
                    
                    if (Widgets.ButtonText(new Rect(inRect.xMax - 310f, rect.y - 5f, 150f, rect.height),
                        "Jump to coordinates"))
                    {
                        Close();
                        CameraJumper.TryJump(spawner.CurrentPos, spawner.CurrentMap);
                    }
                }
            }

            //Coordinates set, procceed with strength of wave
            
            Widgets.DrawLine(new Vector2(inRect.x, 115f), new Vector2(inRect.x + inRect.width, 115f), Color.gray, 2f);
            Widgets.Label(new Rect(inRect.x, inRect.y + 120f, inRect.width, 25f), "Strength of Psychic Wave:");
            //Widgets.DrawLine(new Vector2(inRect.x, 145f), new Vector2(inRect.x + inRect.width, 145f), Color.gray, 1f);
            
            if (Widgets.RadioButtonLabeled(new Rect(inRect.x, inRect.y + 155f, inRect.width, 25f), "Weak (4 - 6 Hives)", strength == PsychicWaveStrength.WEAK))
            {
                if(status == TerminalStatus.READY_TO_ATTACK || status == TerminalStatus.NOT_READY_TO_ATTACK) strength = PsychicWaveStrength.WEAK;
            }
            
            if (Widgets.RadioButtonLabeled(new Rect(inRect.x, inRect.y + 180f, inRect.width, 25f), "Moderate (7 - 9 Hives)", strength == PsychicWaveStrength.MODERATE))
            {
                if(status == TerminalStatus.READY_TO_ATTACK || status == TerminalStatus.NOT_READY_TO_ATTACK) strength = PsychicWaveStrength.MODERATE;
            }
            
            if (Widgets.RadioButtonLabeled(new Rect(inRect.x, inRect.y + 205f, inRect.width, 25f), "Strong (10 - 12 Hives)", strength == PsychicWaveStrength.STRONG))
            {
                if (status == TerminalStatus.READY_TO_ATTACK || status == TerminalStatus.NOT_READY_TO_ATTACK) strength = PsychicWaveStrength.STRONG;
            }
            
            Widgets.DrawLine(new Vector2(inRect.x, 245f), new Vector2(inRect.x + inRect.width, 245f), Color.gray, 2f);
            Widgets.Label(new Rect(inRect.x, 260f, inRect.width, 30f), GetStatus());
            
            int price = Convert.ToInt32(strength);
            
            Widgets.Label(new Rect(inRect.x, 280f, inRect.width, 25f), "Price: " + price + " Silver");
            
            if (status == TerminalStatus.READY_TO_ATTACK)
            {
                if (Widgets.ButtonText(new Rect(inRect.x, 310f, 200f, 30f), "Begin attack!"))
                {
                    
                    if (TradeUtility.ColonyHasEnoughSilver(spawner.Map,
                        price))
                    {
                        Messages.Message("Attack procedure started! Satellite is being positioned...", MessageTypeDefOf.PositiveEvent);
                        status = TerminalStatus.POSITIONING;
                    }
                    else
                    {
                        Messages.Message("You need " + price + " Silver near a trade beacon to launch the attack!", MessageTypeDefOf.NegativeEvent);
                    }
                }
            }

            if (status == TerminalStatus.POSITIONING)
            {
                Widgets.FillableBar(new Rect(inRect.x, 310f, inRect.width, 30f), fillPercent);
                if (Widgets.ButtonText(new Rect(inRect.x, 345f, inRect.width, 30f), "Cancel attack"))
                {
                    spawner.CancelAttack();
                }
            }
        }
        
    }

    public enum PsychicWaveStrength
    {
        WEAK = 1000, MODERATE = 2000, STRONG = 3000
    }

    public enum TerminalStatus
    {
        POSITIONING,
        COOLDOWN,
        READY_TO_ATTACK,
        ATTACKING,
        NOT_READY_TO_ATTACK
    }
}