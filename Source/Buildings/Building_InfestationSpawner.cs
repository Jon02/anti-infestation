using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AntiInfestation.Dialogs;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using Random = UnityEngine.Random;

namespace AntiInfestation.Buildings
{
    public class Building_InfestationSpawner : Building
    {

        //Attack Positions
        private Map currentMap;
        private IntVec3 currentPos;

        private int attackDuration;
        
        //Countdowns
        private int timeUntilInRange = Util.HoursInTicks(48), timeUntilOutOfRange = Util.HoursInTicks(12), timeUntilNextAttack = Util.HoursInTicks(10);
        private Dialog_InfestationTerminal terminal;

        //Comps
        private CompPowerTrader compPower;
        
        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            
            currentMap = null;
            currentPos = IntVec3.Zero;
            
            terminal = new Dialog_InfestationTerminal(this);

            compPower = GetComp<CompPowerTrader>();
        }

        public override void TickRare()
        {
            base.TickRare();
            
            //Do countdowns
            if (terminal.TerminalStatus == TerminalStatus.COOLDOWN)
            {

                if (ResearchProjectDefOf.HackEngineControls.IsFinished)
                {
                    if (TickCountdown(ref timeUntilNextAttack, Util.HoursInTicks(10)))
                    {
                        Messages.Message("NewAttackLaunchNow".Translate(), MessageTypeDefOf.PositiveEvent);
                        terminal.TerminalStatus = TerminalStatus.NOT_READY_TO_ATTACK;
                    }
                }
                else
                {
                    if (TickCountdown(ref timeUntilInRange, Util.HoursInTicks(48)))
                    {
                        Messages.Message("SatelliteNowInRange".Translate(), MessageTypeDefOf.PositiveEvent);
                        terminal.TerminalStatus = TerminalStatus.NOT_READY_TO_ATTACK;
                    }   
                }
            }
            else
            {
                if (!ResearchProjectDefOf.HackEngineControls.IsFinished)
                {
                    if (TickCountdown(ref timeUntilOutOfRange, Util.HoursInTicks(24)))
                    {
                        Messages.Message("SatelliteOutOfRange".Translate(), MessageTypeDefOf.NegativeEvent);
                        ResetPosition();
                        terminal.TerminalStatus = TerminalStatus.COOLDOWN;
                    }
                }

                if (terminal.TerminalStatus == TerminalStatus.POSITIONING)
                {
                    if (terminal.fillPercent >= 1f)
                    {
                        terminal.fillPercent = 0f;
                        terminal.TerminalStatus = TerminalStatus.COOLDOWN;
                        LaunchAttack();
                    }
                    else terminal.fillPercent += 0.02f;
                }
            }
        }

        public override string GetInspectString()
        {
            
            StringBuilder sb = new StringBuilder(base.GetInspectString());

            if (terminal.TerminalStatus == TerminalStatus.COOLDOWN)
            {
                if (ResearchProjectDefOf.HackEngineControls.IsFinished)
                {
                    sb.Append("\n" + "TimeUntilNextAttackLaunch".Translate(Util.TicksToHours(timeUntilNextAttack)));
                }
                else
                {
                    sb.Append("\n" + "TimeUntilSatelliteInRange".Translate(Util.TicksToHours(timeUntilInRange)));
                }
            }
            else
            {
                if (!ResearchProjectDefOf.HackEngineControls.IsFinished)
                {
                    sb.Append("\n" + "TimeUntilSatelliteOutOfRange".Translate(Util.TicksToHours(timeUntilOutOfRange)));
                }
            }

            return sb.ToString();
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (Gizmo gizmo in base.GetGizmos())
            {
                yield return gizmo;
            }

            yield return new Command_Action()
            {
                action = () => Find.WindowStack.Add(terminal),
                defaultDesc = "Open terminal",
                icon = Textures.BeginInfestation,
                disabled = !IsOperational || terminal.TerminalStatus == TerminalStatus.COOLDOWN,
                disabledReason = !IsOperational ? "NoPower".Translate() : "SatelliteOutOfRange".Translate()
            };
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref timeUntilInRange, "timeUntilInRange");
            Scribe_Values.Look(ref timeUntilNextAttack, "timeUntilNextAttack");
            Scribe_Values.Look(ref timeUntilOutOfRange, "timeUntilOutOfRange");
        }
        
        public void SelectTargetOnWorldMap()
        {
            Find.WorldSelector.ClearSelection();
            CameraJumper.TryJump(CameraJumper.GetWorldTarget(this));
            
            Find.WorldTargeter.BeginTargeting(
                delegate(GlobalTargetInfo info)
                {
                    LongEventHandler.QueueLongEvent(() => GenerateMap(info), "GeneratingMapForNewEncounter", false,
                        null);
                    
                    return true;
                }, true, null, false,
                null,
                delegate(GlobalTargetInfo info)
                {
                    if (!Find.WorldObjects.AnyMapParentAt(info.Tile))
                    {
                        return "Target must be a settlement or bandit camp!";
                    }

                    if ((Find.WorldObjects.SiteAt(info.Tile) == null || Find.WorldObjects.SiteAt(info.Tile).MainSitePartDef != SitePartDefOf.BanditCamp) &&
                         !Find.WorldObjects.AnySettlementAt(info.Tile))
                    {
                        return "Target must be a settlement or bandit camp!";
                    }
                    
                    return null;
                }, delegate(GlobalTargetInfo info)
                {
                    if (Find.WorldObjects.SiteAt(info.Tile) != null && Find.WorldObjects.SiteAt(info.Tile).MainSitePartDef == SitePartDefOf.BanditCamp ||
                        Find.WorldObjects.AnySettlementAt(info.Tile))
                    {
                        return true;
                    }
                    
                    return false;
                });
        }
        
        private void GenerateMap(GlobalTargetInfo info)
        {
            Map map = GetOrGenerateMapUtility.GetOrGenerateMap(info.Tile, null);

            if (map == null) return;

            CameraJumper.TryJump(map.Center, map);
            Find.TickManager.Pause();
            
            Find.DesignatorManager.Select(new Designator_InfestationTarget(this));
        }

        public void RegisterPosition(Map map, IntVec3 pos)
        {
            if(currentMap != null && currentMap != map && !currentMap.IsPlayerHome) Current.Game.DeinitAndRemoveMap(currentMap); //Deinit map if other map was chosen
            
            currentMap = map;
            currentPos = pos;

            CameraJumper.TryJump(this);
            Find.WindowStack.Add(terminal);
        }

        public void ResetPosition()
        {
            currentMap = null;
            currentPos = IntVec3.Zero;
        }

        public void CancelAttack()
        {
            ResetPosition();
            
            terminal.fillPercent = 0f;
            terminal.TerminalStatus = TerminalStatus.NOT_READY_TO_ATTACK;
            
            Messages.Message("AttackCanceled".Translate(), MessageTypeDefOf.NeutralEvent);
        }

        public void LaunchAttack()
        {
            
            int price = Convert.ToInt32(terminal.GetStrength);

            if (!TradeUtility.ColonyHasEnoughSilver(Map, price))
            {
                Messages.Message("You need " + price + " Silver near a trade beacon to launch the attack!", MessageTypeDefOf.NegativeEvent);
                CancelAttack();
                return;
            }
            
            if (currentMap.ParentFaction != Faction.OfPlayer && currentMap.ParentFaction.AllyOrNeutralTo(Faction.OfPlayer))
            {
                currentMap.ParentFaction.ChangeGoodwill_Debug(Faction.OfPlayer, -100);
            }
            
            TradeUtility.LaunchSilver(Map, price);
            
            int hives = 0;
            switch (terminal.GetStrength)
            {
                case PsychicWaveStrength.WEAK:
                    hives = Random.Range(4, 6);
                    break;
                case PsychicWaveStrength.MODERATE:
                    hives = Random.Range(7, 9);
                    break;
                case PsychicWaveStrength.STRONG:
                    hives = Random.Range(10, 12);
                    break;
            }
            
            IncidentParms parms = new IncidentParms
            {
                spawnCenter = currentPos,
                target = currentMap,
                forced = true,
                faction = Faction.OfPlayer,
                points = hives
            };

            IncidentDefOf.InfestationWithGivenPosition.Worker.TryExecute(parms);
            Messages.Message("The attack has been launched!", currentPos.GetThingList(currentMap).RandomElement(), MessageTypeDefOf.PositiveEvent);

            terminal.Close();
            terminal.TerminalStatus = TerminalStatus.ATTACKING;
            attackDuration = Find.TickManager.TicksGame;
        }

        
        private bool TickCountdown(ref int countdown, int originValue)
        {
            if (countdown > 0)
            {
                countdown -= 250;
                return false;
            }
            
            //Countdown finished
            countdown = originValue;
            return true;
        }

        public static Thing GetBuilding()
        {
            foreach (Map map in Find.Maps)
            {
                if (map.listerBuildings.ColonistsHaveBuilding(BuildingDefOf.InfestationSpawner))
                {
                    return map.listerThings.ThingsOfDef(BuildingDefOf.InfestationSpawner).First();
                }
            }

            return null;
        }

        public bool IsCurrentlyAttacked(Map map) => currentMap == map;

        public Map CurrentMap
        {
            get => currentMap;
        }

        public int AttackDuration
        {
            get => attackDuration;
        }

        public IntVec3 CurrentPos
        {
            get => currentPos;
        }

        public Dialog_InfestationTerminal Terminal => terminal;

        public bool IsPositionSet => currentPos != IntVec3.Zero;

        public bool IsOperational => compPower != null && compPower.PowerOn;

    }
}