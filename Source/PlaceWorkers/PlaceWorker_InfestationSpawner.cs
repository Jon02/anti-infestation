using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace AntiInfestation
{
    public class PlaceWorker_InfestationSpawner : PlaceWorker
    {
        public override AcceptanceReport AllowsPlacing(BuildableDef checkingDef, IntVec3 loc, Rot4 rot, Map map, Thing thingToIgnore = null,
            Thing thing = null)
        {

            if (map.listerBuildings.ColonistsHaveBuilding(checkingDef as ThingDef))
            {
                return "CannotPlaceBecauseBuildingExistsOnMap".Translate(checkingDef.defName);
            }
            
            foreach (var blueprints in map.blueprintGrid.InnerArray)
            {
                if (blueprints.NullOrEmpty())
                {
                    continue;
                }

                foreach (var blueprint in blueprints)
                {
                    if (blueprint.def == checkingDef.blueprintDef)
                    {
                        return "CannotPlaceBecauseBlueprintExistsOnMap".Translate(checkingDef.defName);
                    }
                }
            }
            
            return true;
        }
        
    }
}