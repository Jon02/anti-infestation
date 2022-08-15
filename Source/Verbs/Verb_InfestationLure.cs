using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;
using Random = UnityEngine.Random;

namespace AntiInfestation
{
    public class Verb_InfestationLure : Verb_CastBase
    {
        
        protected override bool TryCastShot()
        {

            if (caster.Map.listerBuildings.ColonistsHaveBuildingWithPowerOn(BuildingDefOf.InfestationPreventer))
            {
                Messages.Message("There is an infestation preventer on the map, so the attack could not be launched!", MessageTypeDefOf.NegativeEvent);
                return false;
            }
            
            IncidentParms parms = new IncidentParms
            {
                spawnCenter = currentTarget.Cell,
                target = caster.Map,
                forced = true,
                points = Random.Range(3, 5)
            };
            
            IncidentDef incident = IncidentDefOf.InfestationWithGivenPosition;
            incident.Worker.TryExecute(parms);
            
            CompReloadable reloadable = ReloadableCompSource;
            if(reloadable != null) reloadable.UsedOnce();
            return true;
        }

    }
}