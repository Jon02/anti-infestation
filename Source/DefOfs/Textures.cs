using UnityEngine;
using Verse;

namespace AntiInfestation
{
    [StaticConstructorOnStartup]
    public static class Textures
    {
        public static readonly Texture2D BeginInfestation =
            ContentFinder<Texture2D>.Get("UI/Gizmos/BeginInfestation");

        public static readonly Texture2D ConnectToSatellite =
            ContentFinder<Texture2D>.Get("UI/Gizmos/ConnectToSatellite");
    }
}