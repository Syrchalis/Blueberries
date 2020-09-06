using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace SyrBlueberries
{
    //Adds the recipe to administer blueberries to all fleshy pawns
    [StaticConstructorOnStartup]
    public static class AdministerBlueberries
    {
        static AdministerBlueberries()
        {
            BlueberriesRecipeUsers();
        }

        public static void BlueberriesRecipeUsers()
        {
            InfectionDefOf.AdministerBlueberries.recipeUsers = new List<ThingDef>();
            foreach (ThingDef item in DefDatabase<ThingDef>.AllDefs.Where((ThingDef d) => d.category == ThingCategory.Pawn && d.race.IsFlesh))
            {
                InfectionDefOf.AdministerBlueberries.recipeUsers.Add(item);
            }
        }
    }
}
