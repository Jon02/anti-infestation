using RimWorld;
using Verse;

namespace AntiInfestation
{
    [DefOf]
    public class ResearchProjectDefOf
    {
        static ResearchProjectDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(ResearchProjectDefOf));
        }

        public static ResearchProjectDef HackEngineControls;
    }
}