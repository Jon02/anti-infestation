using RimWorld;
using Verse;

namespace AntiInfestation
{
    [DefOf]
    public static class HediffDefOf
    {
        static HediffDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(HediffDefOf));
        }

        public static HediffDef PsychicWave;

    }
}