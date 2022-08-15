using System;
using System.Collections.Generic;
using AntiInfestation.Buildings;
using RimWorld;
using UnityEngine;
using Verse;
using Random = UnityEngine.Random;

namespace AntiInfestation
{
    public class Designator_InfestationTarget : Designator_Area
    {
        private IntVec3 pos;
        private Map map;
        private Building_InfestationSpawner spawner;

        public Designator_InfestationTarget(Building_InfestationSpawner spawner)
        {
            this.spawner = spawner;
        }
        
        public override AcceptanceReport CanDesignateCell(IntVec3 loc)
        {
            return (AcceptanceReport) loc.InBounds(Map);
        }
        
        public override int DraggableDimensions => 0;
        public override bool DragDrawMeasurements => false;

        public override void DesignateSingleCell(IntVec3 c)
        {
            pos = c;
            map = Current.Game.CurrentMap;
        }
        
        public override void SelectedUpdate()
        {
            base.SelectedUpdate();
            GenDraw.DrawFieldEdges(new List<IntVec3>() { UI.MouseCell()}, Color.green);
        }

        protected override void FinalizeDesignationSucceeded()
        {
            base.FinalizeDesignationSucceeded();
            
            if (map.ParentFaction.IsPlayer)
            {
                Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation("AttackOwnFaction".Translate(), () =>
                {
                    spawner.RegisterPosition(map, pos);
                }, false, null));

                return;
            }
            
            if (map.ParentFaction.AllyOrNeutralTo(Faction.OfPlayer))
            {
                Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation("AttackAllyOrNeutral".Translate(), () =>
                {
                    spawner.RegisterPosition(map, pos);
                }, false, null));

                return;
            } 
            
            spawner.RegisterPosition(map, pos);
            Find.DesignatorManager.Deselect();
        }
        
    }
}