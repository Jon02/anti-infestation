using System;
using Verse;

namespace AntiInfestation
{
    public class CompProperties_InsectImpulseGenerator : CompProperties
    {
        public CompProperties_InsectImpulseGenerator()
        {
            compClass = typeof(CompInsectImpulseGenerator);
        }
        
        public float radius = 8f;
        public float severityPerSecond = 0.05f;
        public bool isWorkingThroughWalls = true;
    }
}