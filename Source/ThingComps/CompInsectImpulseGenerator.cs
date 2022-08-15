using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace AntiInfestation
{
    public class CompInsectImpulseGenerator : ThingComp
    {
        private CompProperties_InsectImpulseGenerator Props => (CompProperties_InsectImpulseGenerator) this.props;
        private IEnumerable<IntVec3> workingZone;

        private Room deviceRoom;
        
        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            workingZone = GenRadial.RadialCellsAround(parent.Position, Props.radius, true);
        }
        
        public override void CompTickRare()
        {
            base.CompTickRare();
            if (parent.GetComp<CompPowerTrader>().PowerOn == false) return;
            if (!Props.isWorkingThroughWalls) deviceRoom = parent.Position.GetRoom(parent.Map);
 
            
            foreach (Pawn pawn in parent.Map.mapPawns.PawnsInFaction(Faction.OfInsects))
            {
                if (workingZone.Contains(pawn.Position))
                {
                    if (!Props.isWorkingThroughWalls && pawn.Position.GetRoom(pawn.Map) != deviceRoom)
                    {
                        continue;
                    }
                    
                    HealthUtility.AdjustSeverity(pawn, HediffDefOf.PsychicWave,
                        Props.severityPerSecond); 
                    
                }
            }
        }
        
        public override void PostDrawExtraSelectionOverlays()
        {
            base.PostDrawExtraSelectionOverlays();
            GenDraw.DrawFieldEdges(workingZone.ToList(), Color.green);
        }
    }
}