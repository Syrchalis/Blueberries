using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using System.Reflection;

namespace SyrBlueberries
{
    public class Comp_Bluciferum : CompDrug
    {
        public override void PostIngested(Pawn ingester)
        {
            base.PostIngested(ingester);
            if (ingester.health.hediffSet.HasHediff(InfectionDefOf.MuscleParasites))
            {
                HediffWithComps hediff = (HediffWithComps)ingester.health.hediffSet.GetFirstHediffOfDef(InfectionDefOf.MuscleParasites);
                hediff.comps.First(h => h is HediffComp_TendDuration).CompTended_NewTemp(1.25f, 0);
            }
            if (ingester.health.hediffSet.HasHediff(InfectionDefOf.GutWorms))
            {
                HediffWithComps hediff = (HediffWithComps)ingester.health.hediffSet.GetFirstHediffOfDef(InfectionDefOf.GutWorms);
                hediff.comps.First(h => h is HediffComp_TendDuration).CompTended_NewTemp(1.25f, 0);
            }
            if (ingester.health.hediffSet.HasHediff(InfectionDefOf.FibrousMechanites))
            {
                Hediff hediff = ingester.health.hediffSet.GetFirstHediffOfDef(InfectionDefOf.FibrousMechanites);
                HediffComp hediffComp = hediff.TryGetComp<HediffComp_Disappears>();
                Traverse field = Traverse.Create(hediffComp).Field("ticksToDisappear");
                int ticksToDisappear = field.GetValue<int>();
                field.SetValue(ticksToDisappear - 180000);
            }
            if (ingester.health.hediffSet.HasHediff(InfectionDefOf.SensoryMechanites))
            {
                Hediff hediff = ingester.health.hediffSet.GetFirstHediffOfDef(InfectionDefOf.SensoryMechanites);
                HediffComp hediffComp = hediff.TryGetComp<HediffComp_Disappears>();
                Traverse field = Traverse.Create(hediffComp).Field("ticksToDisappear");
                int ticksToDisappear = field.GetValue<int>();
                field.SetValue(ticksToDisappear - 180000);
            }
            if (ingester.health.hediffSet.HasHediff(HediffDefOf.FoodPoisoning))
            {
                ingester.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.FoodPoisoning).Heal(1);
            }
        }
    }
}
