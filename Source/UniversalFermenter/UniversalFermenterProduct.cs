using Verse;

namespace UniversalFermenter
{
	public class UniversalFermenterProduct
	{

		public ThingDef thingDef;
		public ThingFilter ingredientFilter = new ThingFilter();

		public FloatRange temperatureSafe = new FloatRange(-1f, 32f);
		public FloatRange temperatureIdeal = new FloatRange(7f, 32f);
		public float progressPerDegreePerTick = 1E-05f;
		public int baseFermentationDuration = 360000;
		public int maxCapacity = 25;
		public float speedLessThanSafe = 0.1f;
		public float speedMoreThanSafe = 1f;
		public float efficiency = 1f;
		public FloatRange sunRespect = new FloatRange(1f, 1f);
		public FloatRange rainRespect = new FloatRange(1f, 1f);
		public FloatRange snowRespect = new FloatRange(1f, 1f);
		public FloatRange windRespect = new FloatRange(1f, 1f);
		public string graphSuffix = null;

		public void ResolveReferences()
		{			
			this.ingredientFilter.ResolveReferences();			
		}
	}
}