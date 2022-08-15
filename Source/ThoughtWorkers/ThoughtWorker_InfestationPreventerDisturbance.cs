using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace AntiInfestation.ThoughtWorkers
{
    public class ThoughtWorker_InfestationPreventerDisturbance : ThoughtWorker
    {
        protected override ThoughtState CurrentStateInternal(Pawn p)
        {
            if (!p.Spawned) return false;
            List<Thing> things = p.Map.listerThings.ThingsOfDef(BuildingDefOf.InfestationPreventer);

            if (things.NullOrEmpty()) return false;

            CompPowerTrader power = things.First().TryGetComp<CompPowerTrader>();

            if (power == null || !power.PowerOn)
            {
                return false;
            }
            
            var moodEffect = -Util.StrToIntSafe(LoadedModManager.GetMod<Main>().settings.moodDebuff, 10);
            if (moodEffect == 0) return false; //Dont display thought when 0
            
            def.stages[0].baseMoodEffect = moodEffect;
            
            return true;
        }
    }
}