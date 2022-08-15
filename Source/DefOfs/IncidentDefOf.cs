using RimWorld;

namespace AntiInfestation
{
    [DefOf]
    public class IncidentDefOf
    {
        static IncidentDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(IncidentDefOf));
        }

        public static IncidentDef InfestationWithGivenPosition;
    }
}