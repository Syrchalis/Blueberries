using Verse;
using System.Collections.Generic;

namespace UniversalFermenter
{

	public class CompProperties_UniversalFermenter : CompProperties
	{

		public List<UniversalFermenterProduct> products = new List<UniversalFermenterProduct>();		

		public CompProperties_UniversalFermenter()
		{
			compClass = typeof(CompUniversalFermenter);
		}

		public override void ResolveReferences(ThingDef parentDef)
		{
			base.ResolveReferences(parentDef);
			for (int i = 0; i < this.products.Count; i++)
			{				
				this.products[i].ResolveReferences();
			}
		}
	}
}
