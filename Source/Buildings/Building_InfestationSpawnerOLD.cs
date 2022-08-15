using System;
using System.Collections.Generic;
using System.Data;
using System.Runtime.Remoting.Messaging;
using System.Text;
using AntiInfestation.Dialogs;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using Random = UnityEngine.Random;

namespace AntiInfestation.Buildings
{
    
    /*
     OUTDATED
     
     public class Building_InfestationSpawnerB : Building
    {
        
        public static Map currentMap;
        public static IntVec3 pos;
        
        private GlobalTargetInfo target;
        public CompPowerTrader power;
        
        private int timeUntilSatelliteInRange, timeUnitlNextAttackLaunch, timeUntilOutOfRange;

        private bool satelliteInRange, connectedToSatellite;

        private Dialog_InfestationTerminal terminal;
        
        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            power = GetComp<CompPowerTrader>();

            timeUntilSatelliteInRange = 2500 * 49;
            timeUnitlNextAttackLaunch = 2500 * 10;
            timeUntilOutOfRange = 2500 * 12;
            
          //  terminal = new Dialog_InfestationTerminal(this);
            satelliteInRange = true;
            connectedToSatellite = false;
        }
        
        public override void TickRare()
        {
            base.TickRare();
            if (IsOperational)
            {
                if (terminal.TerminalStatus == TerminalStatus.POSITIONING)
                {
                    if (terminal.fillPercent >= 1f)
                    {
                        terminal.TerminalStatus = TerminalStatus.COOLDOWN;
                        LaunchAttack(terminal.GetStrength);
                        terminal.fillPercent = 0f;
                    }
                    else
                    {
                        terminal.fillPercent += 0.02f;
                    }
                }

                if (ResearchProjectDefOf.HackEngineControls.IsFinished && terminal.TerminalStatus == TerminalStatus.COOLDOWN)
                {
                    //Cooldown now down to 10h

                    if (timeUnitlNextAttackLaunch == 0)
                    {
                        terminal.TerminalStatus = TerminalStatus.NOT_READY_TO_ATTACK;
                        Messages.Message(
                            "The satellite can now process another attack request!",
                            MessageTypeDefOf.PositiveEvent);

                        timeUnitlNextAttackLaunch = 2500 * 10;
                        satelliteInRange = true;
                    }
                    else timeUnitlNextAttackLaunch -= 250;

                } else if (terminal.TerminalStatus == TerminalStatus.COOLDOWN)
                {
                    if (timeUntilSatelliteInRange == 0)
                    {
                        terminal.TerminalStatus = TerminalStatus.NOT_READY_TO_ATTACK;
                        Messages.Message(
                            "The satellite is now in range! You can connect to it via the infestation spawner.",
                            MessageTypeDefOf.PositiveEvent);

                        timeUntilSatelliteInRange = 2500 * 48;
                        satelliteInRange = true;

                    }
                    else timeUntilSatelliteInRange -= 250;
                }
                else
                {
                    if (timeUntilOutOfRange == 0)
                    {
                        Messages.Message("The satellite is now out of range!", MessageTypeDefOf.NegativeEvent);
                        timeUntilOutOfRange = 2500 * 12;
                        satelliteInRange = false;
                    }
                    else timeUntilOutOfRange -= 2500;
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
                    return sb + "\n" + "TimeUntilNextAttackLaunch".Translate(timeUnitlNextAttackLaunch / 2500f);
                }

                return sb + "\n" + "TimeUntilSatelliteInRange".Translate(timeUntilSatelliteInRange / 2500f);
            }
            else
            {
                sb.Append("\n" + "TimeUntilOutOfRange".Translate(timeUntilOutOfRange / 2500f));
            }
            
            return sb.ToString();
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (Gizmo g in base.GetGizmos())
            {
                yield return g;
            }

            if (connectedToSatellite)
            {
                yield return new Command_Action()
                {
                    action = () => Find.WindowStack.Add(terminal),
                    icon = Textures.BeginInfestation,
                    defaultDesc = "Open the terminal",
                    disabled = !power.PowerOn,
                    disabledReason = "NoPower".Translate()
                };
            }
            else
            {
                string researched = "SatelliteOutOfRange";
                if (ResearchProjectDefOf.HackEngineControls.IsFinished) researched = "AttackSpamProtection";
                yield return new Command_Action()
                {
                    action = Connect,
                    icon = Textures.ConnectToSatellite,
                    defaultDesc = "Connect to satellite",
                    disabled = !power.PowerOn || !satelliteInRange,
                    disabledReason = !power.PowerOn ? "NoPower".Translate() : researched.Translate()
                };
            }
        }
        
        private void Connect()
        {
            terminal.TerminalStatus = TerminalStatus.NOT_READY_TO_ATTACK;
            Messages.Message("Connected to satellite! You can now launch an attack.", this, MessageTypeDefOf.PositiveEvent);
            connectedToSatellite = true;
        }
        
        private void Disconnect()
        {
            terminal.Close();
            connectedToSatellite = false;
            satelliteInRange = false;
            terminal.TerminalStatus = TerminalStatus.COOLDOWN;
        }

        public void RegisterPosition(Map map, IntVec3 attackingPos)
        {
            if(currentMap != null && currentMap != map && !currentMap.IsPlayerHome) Current.Game.DeinitAndRemoveMap(currentMap);
            
            currentMap = map;
            pos = attackingPos;
            terminal.CoordinatesSet = true;
            
            CameraJumper.TryJump(this);
            Find.WindowStack.Add(terminal);
        }
        
        public static void ResetPosition()
        {
            currentMap = null;
            pos = IntVec3.Zero;
        }

        public void CancelAttack()
        {
            ResetPosition();
            terminal.fillPercent = 0f;
            terminal.TerminalStatus = TerminalStatus.NOT_READY_TO_ATTACK;
            terminal.CoordinatesSet = false;
            
            power.PowerOutput = 1600;
            
            Messages.Message("The attack has been canceled!", MessageTypeDefOf.NeutralEvent);
        }
        
        public void BeginTargeting()
        {
            Find.WorldSelector.ClearSelection();
            CameraJumper.TryJump(CameraJumper.GetWorldTarget(this));
            
            Find.WorldTargeter.BeginTargeting_NewTemp(
                ChooseWorldTarget, true, null, false,
                null,
                delegate(GlobalTargetInfo info)
                {
                    if (!Find.WorldObjects.AnyMapParentAt(info.Tile))
                    {
                        return "Target must be a settlement or bandit camp!";
                    }
                    
                    return null;
                }, delegate(GlobalTargetInfo info)
                {
                    if (Find.WorldObjects.AnyMapParentAt(info.Tile))
                    {
                        return true;
                    }

                    return false;
                });
        }
        private void LaunchAttack(PsychicWaveStrength strength)
        {
            
            int price = Convert.ToInt32(strength);

            if (!TradeUtility.ColonyHasEnoughSilver(Map, price))
            {
                Messages.Message("You need " + price + " Silver near a trade beacon to launch the attack!", MessageTypeDefOf.NegativeEvent);
                CancelAttack();
                return;
            }
            
            if (currentMap.ParentFaction != Faction.OfPlayer && currentMap.ParentFaction.AllyOrNeutralTo(Faction.OfPlayer))
            {
                currentMap.ParentFaction.TrySetRelationKind(Faction.OfPlayer, FactionRelationKind.Hostile, true, null, Find.WorldObjects.SettlementAt(currentMap.Tile));
            }
            
            TradeUtility.LaunchSilver(Map, price);
            
            int hives = 0;
            switch (strength)
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
                spawnCenter = pos,
                target = currentMap,
                forced = true,
                faction = Faction.OfPlayer,
                points = hives
            };

            IncidentDefOf.InfestationWithGivenPosition.Worker.TryExecute(parms);
            Messages.Message("The attack has been launched!", pos.GetThingList(currentMap).RandomElement(), MessageTypeDefOf.PositiveEvent);
            terminal.CoordinatesSet = false;
            Disconnect();
        }
        
        private bool ChooseWorldTarget(GlobalTargetInfo target)
        {
            this.target = target;
            LongEventHandler.QueueLongEvent(new Action(EnterMap), "GeneratingMapForNewEncounter", false, null, true);
            return true;
        }
        
        private void EnterMap()
        {
            Map map = Current.Game.FindMap(target.Tile);
   
            if (map == null)
            {
                MapParent parent = Find.WorldObjects.MapParentAt(target.Tile);

                map = MapGenerator.GenerateMap(Find.World.info.initialMapSize, parent, parent.MapGeneratorDef,
                    parent.ExtraGenStepDefs, null);
            }

            CameraJumper.TryJump(map.Center, map);
            
         //   Designator_InfestationTarget des = new Designator_InfestationTarget(this);

            Find.TickManager.Pause();
           //Find.DesignatorManager.Select(des);
        }

        public static bool IsCurrentlyAttacked(Map map) => map == currentMap;
        public bool IsOperational => power.PowerOn && power != null;
        public Map GetMap => currentMap;
        
    }*/

}