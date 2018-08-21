using System.Collections.Generic;

using RimWorld;
using Verse;
using Verse.AI;

namespace UniversalFermenter
{

	public class JobDriver_TakeProductOutOfUniversalFermenter : JobDriver
	{

		private const TargetIndex FermenterInd = TargetIndex.A;
		private const TargetIndex ProductToHaulInd = TargetIndex.B;
		private const TargetIndex StorageCellInd = TargetIndex.C;
		private const int Duration = 200;

		protected Thing Fermenter
		{
			get
			{
				//return CurJob.GetTarget(TargetIndex.A).Thing;
				return this.job.GetTarget(TargetIndex.A).Thing;
			}
		}

		protected Thing Product
		{
			get
			{
				//return CurJob.GetTarget(TargetIndex.B).Thing;
				return this.job.GetTarget(TargetIndex.B).Thing;
			}
		}

		public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
			return this.pawn.Reserve(this.Fermenter, this.job, 1, -1, null);
		}

		protected override IEnumerable<Toil> MakeNewToils()
		{
			CompUniversalFermenter comp = Fermenter.TryGetComp<CompUniversalFermenter>();
			// Verify fermenter validity
			this.FailOn(() => !comp.Fermented);
			this.FailOnDestroyedNullOrForbidden(FermenterInd);

			// Reserve fermenter
			yield return Toils_Reserve.Reserve(FermenterInd);

			// Go to the fermenter
			yield return Toils_Goto.GotoThing(FermenterInd, PathEndMode.ClosestTouch);

			// Add delay for collecting product from fermenter, if it is ready
			yield return Toils_General.Wait(Duration).FailOnDestroyedNullOrForbidden(FermenterInd).WithProgressBarToilDelay(FermenterInd);

			// Collect product
			Toil collect = new Toil();
			collect.initAction = () =>
			{
				Thing product = comp.TakeOutProduct();
				GenPlace.TryPlaceThing(product, pawn.Position, Map, ThingPlaceMode.Near);
				StoragePriority storagePriority = StoreUtility.CurrentStoragePriorityOf(product);
                IntVec3 c;

				// Try to find a suitable storage spot for the product
				if (StoreUtility.TryFindBestBetterStoreCellFor(product, pawn, Map, storagePriority, pawn.Faction, out c))
				{
					this.job.SetTarget(TargetIndex.B, product);
					this.job.count = product.stackCount;
					this.job.SetTarget(TargetIndex.C, c);
				}
				// If there is no spot to store the product, end this job
				else
				{
					EndJobWith(JobCondition.Incompletable);
				}
			};
			collect.defaultCompleteMode = ToilCompleteMode.Instant;
			yield return collect;

			// Reserve the product
			yield return Toils_Reserve.Reserve(ProductToHaulInd);

			// Reserve the storage cell
			yield return Toils_Reserve.Reserve(StorageCellInd);

			// Go to the product
			yield return Toils_Goto.GotoThing(ProductToHaulInd, PathEndMode.ClosestTouch);

			// Pick up the product
			yield return Toils_Haul.StartCarryThing(ProductToHaulInd);

			// Carry the product to the storage cell, then place it down
			Toil carry = Toils_Haul.CarryHauledThingToCell(StorageCellInd);
			yield return carry;
			yield return Toils_Haul.PlaceHauledThingInCell(StorageCellInd, carry, true);

			// End the current job
			yield break;
		}
	}
}
