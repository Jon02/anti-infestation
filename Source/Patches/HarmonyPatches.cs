using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using AntiInfestation.Buildings;
using AntiInfestation.Dialogs;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace AntiInfestation.Patches
{
    [HarmonyPatch(typeof(Settlement), "ShouldRemoveMapNow")]
    public static class HarmonyPatch_DontUnloadMapOnSettlement
    {
        [HarmonyPostfix]
        public static void Postfix(Settlement __instance, ref bool __result)
        {
            Building_InfestationSpawner spawner = Building_InfestationSpawner.GetBuilding() as Building_InfestationSpawner;
            if (spawner == null) return;
            
            if (spawner.IsCurrentlyAttacked(__instance.Map))
            {
                __result = false;
            }
        }
    }
    
    [HarmonyPatch(typeof(Site), "ShouldRemoveMapNow")]
    public static class HarmonyPatch_DontUnloadMapOnSite
    {
        [HarmonyPostfix]
        public static void Postfix(Settlement __instance, ref bool __result)
        {
            Building_InfestationSpawner spawner = Building_InfestationSpawner.GetBuilding() as Building_InfestationSpawner;
            if (spawner == null) return;
            
            if (spawner.IsCurrentlyAttacked(__instance.Map)) //Gets executed every tick!
            {
                __result = false;
            }
        }
    }
    
    [HarmonyPatch(typeof(Tale_SinglePawn))]
    [HarmonyPatch(MethodType.Constructor)]
    [HarmonyPatch(new Type[] { typeof(Pawn)})]
    internal static class HarmonyPatch_ReplacePawnForTale
    {
        [HarmonyPrefix]
        static void Prefix(ref Pawn pawn)
        {
            if (pawn == null)
            {
                Pawn p;
                if ((p = Current.Game.AnyPlayerHomeMap.mapPawns.FreeColonists.RandomElement()) == null)
                {
                    pawn = PawnGenerator.GeneratePawn(PawnKindDefOf.Colonist, Faction.OfAncientsHostile);
                }
                else
                {
                    pawn = p;
                }
            }
        }
    }

    [HarmonyPatch(typeof(SettlementDefeatUtility), "IsDefeated")]
    internal static class HarmonyPatch_IsDefeated
    {
        [HarmonyPostfix]
        static void Postfix(Map map, Faction faction, ref bool __result)
        {
            Building_InfestationSpawner spawner = Building_InfestationSpawner.GetBuilding() as Building_InfestationSpawner;
            
            if (spawner == null) return;
            
            if (map.ParentFaction.AllyOrNeutralTo(Faction.OfPlayer))
            {
                if (spawner.IsCurrentlyAttacked(map))
                {
                    __result = false;
                }
            }
            else
            {
                if (__result && spawner.IsCurrentlyAttacked(map))
                {
                    spawner.ResetPosition();
                }
            }
            
            if (spawner.Terminal.TerminalStatus == TerminalStatus.ATTACKING)
            {
                if (Find.TickManager.TicksGame >= spawner.AttackDuration + Util.HoursInTicks(2))
                {
                    if (map.mapPawns.SpawnedDownedPawns.Count == map.mapPawns.SpawnedPawnsInFaction(Faction.OfInsects).Count || map.mapPawns.PawnsInFaction(Faction.OfInsects).NullOrEmpty())
                    {
                        Messages.Message("The attack has failed!", MessageTypeDefOf.NegativeEvent);
                        __result = false;
                        spawner.ResetPosition();
                    }

                    if (map.mapPawns.PawnsInFaction(faction).NullOrEmpty())
                    {
                        Messages.Message("The attack was successful!", MessageTypeDefOf.PositiveEvent);
                        __result = true;
                        spawner.ResetPosition();   
                    }
                }
            }

            if (__result)
            {
                spawner.Terminal.TerminalStatus = TerminalStatus.COOLDOWN;
            }
        }
    }

    [HarmonyPatch(typeof(IncidentWorker_Infestation), "TryExecuteWorker")]
    internal static class HarmonyPatch_PreventInfestation
    {
        [HarmonyPrefix]
        static bool Prefix(IncidentParms parms, ref bool __result)
        {
            Map map = parms.target as Map;

            if (map == null)
            {
                return true;
            }

            if (!map.listerBuildings.ColonistsHaveBuilding(BuildingDefOf.InfestationPreventer))
            {
                return true;
            }

            Building preventer = map.listerBuildings.AllBuildingsColonistOfDef(BuildingDefOf.InfestationPreventer).First();
            
            if (preventer == null || !preventer.TryGetComp<CompPowerTrader>().PowerOn)
            {
                return true;
            }
            
            IncidentDef replaceIncident = DefDatabase<IncidentDef>.GetNamed("RaidEnemy");
            replaceIncident.Worker.TryExecute(parms);
            
            __result = false;
            return false;
        }
    }
    
}