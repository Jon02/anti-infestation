using RimWorld;

namespace AntiInfestation
{
    [DefOf]
    public class ThoughtDefOf
    {
        static ThoughtDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(ThoughtDefOf));  
        }

        public static ThoughtDef MoodDecreaseDueToWaves;
    }
}