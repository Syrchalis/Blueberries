using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace SyrBlueberries
{
    [DefOf]
    public static class InfectionDefOf
    {
        static InfectionDefOf()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(HediffDefOf));
        }
        public static HediffDef MuscleParasites;
        public static HediffDef GutWorms;
        public static HediffDef FibrousMechanites;
        public static HediffDef SensoryMechanites;
        public static RecipeDef AdministerBlueberries;
    }
}
