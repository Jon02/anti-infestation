using RimWorld;
using Verse;

namespace AntiInfestation
{
    [DefOf]
    public class BuildingDefOf
    {
        static BuildingDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(BuildingDefOf));
        }

        public static ThingDef InfestationSpawner;
        public static ThingDef InfestationPreventer;

    }
}