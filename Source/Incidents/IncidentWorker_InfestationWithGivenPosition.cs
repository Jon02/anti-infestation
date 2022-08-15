using System;
using RimWorld;
using UnityEngine;
using Verse;
using Random = UnityEngine.Random;

namespace AntiInfestation
{
    public class IncidentWorker_InfestationWithGivenPosition : IncidentWorker_Infestation
    {

        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            Map map = (Map) parms.target;
            int hiveCount = (int) parms.points;
            
            Thing thing = GenSpawn.Spawn(ThingMaker.MakeThing(ThingDefOf.TunnelHiveSpawner, null), parms.spawnCenter,
                map, WipeMode.FullRefund);

            for (int i = 0; i < hiveCount - 1; i++)
            {
                var loc = CompSpawnerHives.FindChildHiveLocation(thing.Position, map, ThingDefOf.Hive, ThingDefOf.Hive.GetCompProperties<CompProperties_SpawnerHives>(), true, true);
                if (loc.IsValid)
                {
                    thing = GenSpawn.Spawn(ThingMaker.MakeThing(ThingDefOf.TunnelHiveSpawner, null), loc, map, WipeMode.FullRefund);
                }
            }

            Find.TickManager.slower.SignalForceNormalSpeed();
            
            return true;
        }
        
        
    }
}