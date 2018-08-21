using Harmony;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace UniversalFermenter
{
    public class Comp_Bluciferum : CompDrug
    {
        public override void PostIngested(Pawn ingester)
        {
            base.PostIngested(ingester);
            if (ingester.health.hediffSet.HasHediff(InfectionDefOf.MuscleParasites))
            {
                HediffWithComps hediff = (HediffWithComps)ingester.health.hediffSet.GetFirstHediffOfDef(InfectionDefOf.MuscleParasites);
                hediff.comps.First(h => h is HediffComp_TendDuration).CompTended(1, 0);
            }
            if (ingester.health.hediffSet.HasHediff(InfectionDefOf.GutWorms))
            {
                HediffWithComps hediff = (HediffWithComps)ingester.health.hediffSet.GetFirstHediffOfDef(InfectionDefOf.GutWorms);
                hediff.comps.First(h => h is HediffComp_TendDuration).CompTended(1, 0);
            }
            if (ingester.health.hediffSet.HasHediff(InfectionDefOf.FibrousMechanites))
            {
                HediffWithComps hediff = (HediffWithComps)ingester.health.hediffSet.GetFirstHediffOfDef(InfectionDefOf.FibrousMechanites);
                HediffComp hediffComp = hediff.comps.First(h => h is HediffComp_Disappears);
                Traverse field = Traverse.Create(hediffComp).Field("ticksToDisappear");
                int ticksToDisappear = field.GetValue<int>();
                field.SetValue(ticksToDisappear - 90000);
            }
            if (ingester.health.hediffSet.HasHediff(InfectionDefOf.SensoryMechanites))
            {
                HediffWithComps hediff = (HediffWithComps)ingester.health.hediffSet.GetFirstHediffOfDef(InfectionDefOf.SensoryMechanites);
                HediffComp hediffComp = hediff.comps.First(h => h is HediffComp_Disappears);
                Traverse field = Traverse.Create(hediffComp).Field("ticksToDisappear");
                int ticksToDisappear = field.GetValue<int>();
                field.SetValue(ticksToDisappear - 90000);
            }

        }
    }
}
