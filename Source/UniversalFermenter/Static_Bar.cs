using UnityEngine;
using Verse;

namespace UniversalFermenter
{
	[StaticConstructorOnStartup]
	public static class Static_Bar
	{
		public static readonly Vector2 Size = new Vector2(0.55f, 0.1f);
		public static readonly Color ZeroProgressColor = new Color(0.4f, 0.27f, 0.22f);
		public static readonly Color FermentedColor = new Color(0.9f, 0.85f, 0.2f);
		public static readonly Material UnfilledMat = SolidColorMaterials.SimpleSolidColorMaterial(new Color(0.3f, 0.3f, 0.3f), false);
	}
}