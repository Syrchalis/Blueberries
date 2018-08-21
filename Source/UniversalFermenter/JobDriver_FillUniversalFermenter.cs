using System.Collections.Generic;

using RimWorld;
using Verse;
using Verse.AI;

namespace UniversalFermenter
{

	public class JobDriver_FillUniversalFermenter : JobDriver
	{

		private const TargetIndex FermenterInd = TargetIndex.A;
		private const TargetIndex IngredientInd = TargetIndex.B;
		private const int Duration = 200;


		protected Thing Fermenter
		{
			get
			{
				return this.job.GetTarget(TargetIndex.A).Thing;
			}
		}

		protected Thing Ingredient
		{
			get
			{
				return this.job.GetTarget(TargetIndex.B).Thing;
			}
		}

		public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
			return this.pawn.Reserve(this.Fermenter, this.job, 1, -1, null, errorOnFailed) && this.pawn.Reserve(this.Ingredient, this.job, 1, -1, null, errorOnFailed);
		}

		protected override IEnumerable<Toil> MakeNewToils()
		{
			CompUniversalFermenter comp = Fermenter.TryGetComp<CompUniversalFermenter>();
						
			// Verify fermenter and ingredient validity
			this.FailOn(() => comp.SpaceLeftForIngredient <= 0);
			this.FailOnDespawnedNullOrForbidden(FermenterInd);
			this.FailOnBurningImmobile(FermenterInd);
			this.FailOnDestroyedNullOrForbidden(IngredientInd);

			// Reserve resources
			// Creating the toil before yielding allows for CheckForGetOpportunityDuplicate
			Toil ingrToil = Toils_Reserve.Reserve(IngredientInd);
			yield return ingrToil;

			// Reserve fermenter
			yield return Toils_Reserve.Reserve(FermenterInd);

			// Go to the ingredient
			yield return Toils_Goto.GotoThing(IngredientInd, PathEndMode.ClosestTouch)
			  .FailOnSomeonePhysicallyInteracting(IngredientInd)
			  .FailOnDestroyedNullOrForbidden(IngredientInd);

			// Haul the ingredients
			yield return Toils_Haul.StartCarryThing(IngredientInd, false, true).FailOnDestroyedNullOrForbidden(IngredientInd);
			yield return Toils_Haul.CheckForGetOpportunityDuplicate(ingrToil, IngredientInd, TargetIndex.None, true);

			// Carry ingredients to the fermenter
			yield return Toils_Haul.CarryHauledThingToCell(FermenterInd);

			// Add delay for adding ingredients to the fermenter
			yield return Toils_General.Wait(Duration).FailOnDestroyedNullOrForbidden(FermenterInd).WithProgressBarToilDelay(FermenterInd);

			// Use ingredients
			// The UniversalFermenter automatically destroys held ingredients
			Toil add = new Toil();
			add.initAction = () =>
			{
				if (!comp.AddIngredient(Ingredient))
				{
					// The ingredient is not allowed, end the job
					EndJobWith(JobCondition.Incompletable);
					Log.Message("JobCondition.Incompletable");
				}
			};
			add.defaultCompleteMode = ToilCompleteMode.Instant;
			yield return add;

			// End the current job
			yield break;
		}
	}
}
