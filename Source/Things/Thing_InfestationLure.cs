using System.Collections.Generic;
using System.Text;
using RimWorld;
using Verse;

namespace AntiInfestation
{
    public class Thing_InfestationLure : Apparel
    {
        private int timeUntilFullyCharged = 60000; //one whole day to recharge
        private CompReloadable reloadable;

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            reloadable = GetComp<CompReloadable>();
        }
        

        public override void TickLong()
        {
            base.TickLong();
            if (!reloadable.CanBeUsed && reloadable != null && !Position.Roofed(Map))
            {
                timeUntilFullyCharged -= 2000;
                if (timeUntilFullyCharged == 0)
                {
                    Thing ammo = ThingMaker.MakeThing(reloadable.AmmoDef, null);
                    ammo.stackCount = 1;
                    
                    Messages.Message("Infestation Wave Creater successfully recharged!", ammo, MessageTypeDefOf.PositiveEvent, false);
                    reloadable.ReloadFrom(ammo);
                    reloadable.ReloadFrom(ammo);
                    timeUntilFullyCharged = 60000;
                }
            }
        }
        
        public override string GetInspectString()
        {
            StringBuilder sb = new StringBuilder(base.GetInspectString());

            if (!reloadable.CanBeUsed)
            {
                sb.Append("\nFully recharged in: " + (timeUntilFullyCharged / 60000f) * 24 + "h");
            }

            return sb.ToString();
        }
    }
}