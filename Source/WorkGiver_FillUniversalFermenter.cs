using System;

using RimWorld;
using Verse;
using Verse.AI;

namespace Kubouch
{

	public class WorkGiver_FillUniversalFermenter : WorkGiver_Scanner
	{
		
		private static string TemperatureTrans;
		private static string NoIngredientTrans;

		public override ThingRequest PotentialWorkThingRequest
		{
			get
			{
				return ThingRequest.ForGroup(ThingRequestGroup.BuildingArtificial);
			}
		}

		public override PathEndMode PathEndMode
		{
			get
			{
				return PathEndMode.Touch;
			}
		}
		
		public static void Reset()
		{
			TemperatureTrans = "BadTemperature".Translate().ToLower();
			NoIngredientTrans = "UF_NoIngredient".Translate();
		}
		
		public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
		{
			CompUniversalFermenter comp = t.TryGetComp<CompUniversalFermenter>();

			if (comp == null || comp.Fermented || comp.SpaceLeftForIngredient <= 0)
			{
				return false;
			}
			float ambientTemperature = comp.parent.AmbientTemperature;			
			if (ambientTemperature < comp.Product.temperatureSafe.min + 2f || ambientTemperature > comp.Product.temperatureSafe.max - 2f)
			{
				JobFailReason.Is(TemperatureTrans);
				return false;
			}
			if (t.IsForbidden(pawn) || !pawn.CanReserveAndReach(t, PathEndMode.Touch, pawn.NormalMaxDanger(), 1, -1, null, forced))
			{
				return false;
			}
			if (pawn.Map.designationManager.DesignationOn(t, DesignationDefOf.Deconstruct) != null)
			{
				return false;
			}
			if (FindIngredient(pawn, t) == null)
			{				
				JobFailReason.Is(NoIngredientTrans);
				return false;
			}
			return !t.IsBurning();
		}


		public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
		{
			Thing t2 = FindIngredient(pawn, t);
			return new Job(DefDatabase<JobDef>.GetNamed("UF_FillUniversalFermenter"), t, t2)
			{
				count = t.TryGetComp<CompUniversalFermenter>().SpaceLeftForIngredient
			};			
		}

		private Thing FindIngredient(Pawn pawn, Thing fermenter)
		{
			ThingFilter filter = fermenter.TryGetComp<CompUniversalFermenter>().Product.ingredientFilter;
			Predicate<Thing> predicate = (Thing x) => !x.IsForbidden(pawn) && pawn.CanReserve(x) && filter.Allows(x);
			Predicate<Thing> validator = predicate;
			return GenClosest.ClosestThingReachable(pawn.Position, pawn.Map, filter.BestThingRequest, PathEndMode.ClosestTouch, TraverseParms.For(pawn, Danger.Deadly, TraverseMode.ByPawn, false), 9999f, validator);
		}
	}
}
