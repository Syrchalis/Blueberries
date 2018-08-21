// Notes:
//   * parent.Map is null when the building (parent) is minified (uninstalled).

using System.Collections.Generic;
using System.Text;

//using System.Linq; // Used for texture loading in GetIcon()

using UnityEngine;
using RimWorld;
using Verse;

namespace UniversalFermenter
{

	public class CompUniversalFermenter : ThingComp
	{

		private int ingredientCount;
		private float progressInt;
		private Material barFilledCachedMat;
		private int nextResourceInd;
		private int currentResourceInd;
		private List<string> ingredientLabels = new List<string>();

		protected float ruinedPercent;

		public string defaultTexPath;		
		public CompRefuelable refuelComp;
		public CompPowerTrader powerTradeComp;
		public CompFlickable flickComp;

		public const string RuinedSignal = "RuinedByTemperature";

		// Properties

		public CompProperties_UniversalFermenter Props
		{
			get { return (CompProperties_UniversalFermenter)props; }
		}

		private int ResourceListSize
		{
			get
			{
				return Props.products.Count;
			}
		}

		public UniversalFermenterProduct Product
		{
			get
			{
				return Props.products[currentResourceInd];
			}
		}

		public UniversalFermenterProduct NextProduct
		{
			get
			{
				return Props.products[nextResourceInd];
			}
		}

		public bool Ruined
		{
			get
			{
				return ruinedPercent >= 1f;
			}
		}
		
		public string SummaryAddedIngredients
		{
			get
			{
				int substractLength;
				int maxSummaryLength;
				int lineLength = 60;
				string summary = "";
				for (int i = 0; i < ingredientLabels.Count; i++)
				{
					if (i == 0)
						summary += ingredientLabels[i];
					else
						summary += ", " + ingredientLabels[i];
				}

				substractLength = ("Contains " + Product.maxCapacity.ToString() + "/" + Product.maxCapacity.ToString() + " ").Length;
				maxSummaryLength = lineLength - substractLength;
				return Utils.VowelTrim(summary, maxSummaryLength);
				/*if (summary.Length > maxSummaryLength)
					return summary.Remove(maxSummaryLength - 3) + " ..";
				else
					return summary;*/
			}
		}

		public string SummaryNextIngredientFilter
		{
			get
			{
				/*string summary = "";
				foreach (ThingDef thingDef in NextIngredientFilter.AllowedThingDefs)
				{
					if (summary == "")
						summary += thingDef.label;
					else
						summary += ", " + thingDef.label;
				}*/
				//summary = NextIngredientFilter.ToString();
				return Utils.IngredientFilterSummary(NextProduct.ingredientFilter);
			}
		}

		public float Progress
		{
			get
			{
				return progressInt;
			}
			set
			{
				if (value == progressInt)
				{
					return;
				}
				progressInt = value;
				barFilledCachedMat = null;
			}
		}

		private Material BarFilledMat
		{
			get
			{
				if (barFilledCachedMat == null)
				{
					barFilledCachedMat = SolidColorMaterials.SimpleSolidColorMaterial(Color.Lerp(Static_Bar.ZeroProgressColor, Static_Bar.FermentedColor, Progress), false);
				}
				return barFilledCachedMat;
			}
		}

		private bool Empty
		{
			get
			{
				return ingredientCount <= 0;
			}
		}

		public bool Fermented
		{
			get
			{
				return !Empty && Progress >= 1f;
			}
		}

		public int SpaceLeftForIngredient
		{
			get
			{
				if (Fermented)
				{
					return 0;
				}
				return Product.maxCapacity - ingredientCount;
			}
		}
		
		private void NextResource()
		{
			nextResourceInd++;
			if (nextResourceInd >= ResourceListSize)
				nextResourceInd = 0;
			if (Empty)
			{
				currentResourceInd = nextResourceInd;
			}
		}

		private float CurrentTempProgressSpeedFactor
		{
			get
			{				
				float ambientTemperature = parent.AmbientTemperature;
				// Temperature out of a safe range
				if (ambientTemperature < Product.temperatureSafe.min)
				{
					return Product.speedLessThanSafe;
				}
				else if (ambientTemperature > Product.temperatureSafe.max)
				{
					return Product.speedMoreThanSafe;
				}
				// Temperature out of an ideal range but still within a safe range
				if (ambientTemperature < Product.temperatureIdeal.min)
				{
					return GenMath.LerpDouble(Product.temperatureSafe.min, Product.temperatureIdeal.min, Product.speedLessThanSafe, 1f, ambientTemperature);
				}
				else if (ambientTemperature > Product.temperatureIdeal.max)
				{
					return GenMath.LerpDouble(Product.temperatureIdeal.max, Product.temperatureSafe.max, 1f, Product.speedMoreThanSafe, ambientTemperature);
				}
				// Temperature within an ideal range
				return 1f;
			}
		}

		public float SunRespectSpeedFactor
		{
			get
			{
				if (parent.Map == null)
				{
					return 0f;
				}
				if (Product.sunRespect.Span == 0)
				{
					return 1f;
				}
				float skyGlow = parent.Map.skyManager.CurSkyGlow * (1 - RoofedFactor);
				return GenMath.LerpDouble(Static_Weather.SunGlowRange.TrueMin, Static_Weather.SunGlowRange.TrueMax,
				                          Product.sunRespect.min, Product.sunRespect.max,
				                          skyGlow);
			}
		}
		
		public float RainRespectSpeedFactor
		{
			get
			{
				if (parent.Map == null)
				{
					return 0f;
				}
				if (Product.rainRespect.Span == 0)
				{
					return 1f;
				}
				// When snowing, the game also increases RainRate.
				// Therefore, non-zero SnowRate puts RainRespect to a state as if it was not raining.
				if (parent.Map.weatherManager.SnowRate != 0)
				{
					return Product.rainRespect.min;
				}
				float rainRate = parent.Map.weatherManager.RainRate * (1 - RoofedFactor);
				return GenMath.LerpDouble(Static_Weather.RainRateRange.TrueMin, Static_Weather.RainRateRange.TrueMax,
				                          Product.rainRespect.min, Product.rainRespect.max,
				                          rainRate);
			}
		}

		public float SnowRespectSpeedFactor
		{
			get
			{
				if (parent.Map == null)
				{
					return 0f;
				}
				if (Product.snowRespect.Span == 0)
				{
					return 1f;
				}
				float snowRate = parent.Map.weatherManager.SnowRate * (1 - RoofedFactor);
				return GenMath.LerpDouble(Static_Weather.SnowRateRange.TrueMin, Static_Weather.SnowRateRange.TrueMax,
				                          Product.snowRespect.min, Product.snowRespect.max,
				                          snowRate);
			}
		}

		public float WindRespectSpeedFactor
		{
			get
			{
				if (parent.Map == null)
				{
					return 0f;
				}
				if (Product.windRespect.Span == 0)
				{
					return 1f;
				}
				if (RoofedFactor != 0)
				{
					return Product.windRespect.min;
				}
				return GenMath.LerpDouble(Static_Weather.WindSpeedRange.TrueMin, Static_Weather.WindSpeedRange.TrueMax,
				                          Product.windRespect.min, Product.windRespect.max,
				                          parent.Map.windManager.WindSpeed);
			}
		}

		public float RoofedFactor  // How much of the building is under a roof
		{
			get
			{
				if (parent.Map == null)
				{
					return 0f;
				}
				int allTiles = 0;
				int roofedTiles = 0;
				foreach (IntVec3 current in parent.OccupiedRect())
				{
					allTiles++;
					if (parent.Map.roofGrid.Roofed(current))
					{
						roofedTiles++;
					}
				}
				return (float)roofedTiles / (float)allTiles;
				//return (float)(num - num2) / (float)num;
			}
		}

		public float SpeedFactor
		{
			get
			{
				// Always >= 0
				return Mathf.Max(CurrentTempProgressSpeedFactor * SunRespectSpeedFactor
				                                                * RainRespectSpeedFactor
				                                                * SnowRespectSpeedFactor
				                                                * WindRespectSpeedFactor,
				                 0f);
			}
		}
		
		private float CurrentProgressPerTick
		{
			get
			{
				return (1f / Product.baseFermentationDuration) * SpeedFactor;
			}
		}

		private int EstimatedTicksLeft
		{
			get
			{
				if (CurrentProgressPerTick == 0)
				{
					return -1;
				}
				else
				{
					return Mathf.Max(Mathf.RoundToInt((1f - Progress) / CurrentProgressPerTick), 0);
				}				
			}
		}

		public bool Fueled
		{
			get
			{
				return (refuelComp == null || refuelComp.HasFuel);
			}
		}

		public bool Powered
		{
			get
			{
				return (powerTradeComp == null || powerTradeComp.PowerOn);
			}
		}

		public bool FlickedOn
		{
			get
			{
				return (flickComp == null || flickComp.SwitchIsOn);
			}
		}

		// Methods

		public override void Initialize(CompProperties props)
		{
			base.Initialize(props);
			refuelComp = parent.GetComp<CompRefuelable>();
			powerTradeComp = parent.GetComp<CompPowerTrader>();
			flickComp = parent.GetComp<CompFlickable>();
			defaultTexPath = parent.def.graphicData.texPath;			
		}

		public override void PostExposeData()
		{
			Scribe_Values.Look(ref ruinedPercent, "ruinedPercent", 0f, false);
			Scribe_Values.Look(ref ingredientCount, "UF_UniversalFermenter_IngredientCount", 0);
			Scribe_Values.Look(ref progressInt, "UF_UniversalFermenter_Progress", 0f);
			Scribe_Values.Look(ref nextResourceInd, "UF_nextResourceInd", 0);
			Scribe_Values.Look(ref currentResourceInd, "UF_currentResourceInd", 0);
			Scribe_Values.Look(ref defaultTexPath, "defaultTexPath");
			Scribe_Collections.Look(ref ingredientLabels, "UF_ingredientLabels");
		}

		public override void PostDraw()
		{
			base.PostDraw();
			if (!Empty)
			{
				Vector3 drawPos = parent.DrawPos;
				drawPos.y += 0.0483870953f;
				drawPos.z += 0.25f;
				GenDraw.DrawFillableBar(new GenDraw.FillableBarRequest
				{
					center = drawPos,
					size = Static_Bar.Size,
					fillPercent = ingredientCount / (float)Product.maxCapacity,					
					filledMat = BarFilledMat,
					unfilledMat = Static_Bar.UnfilledMat,
					margin = 0.1f,
					rotation = Rot4.North
				});
			}
		}

		public bool AddIngredient(Thing ingredient)
		{
			if (!Product.ingredientFilter.Allows(ingredient))
			{
				return false;
			}
			if (!ingredientLabels.Contains(ingredient.def.label))
				ingredientLabels.Add(ingredient.def.label);
			AddIngredient(ingredient.stackCount);
			ingredient.Destroy(DestroyMode.Vanish);
			return true;
		}

		public void AddIngredient(int count)
		{
			ruinedPercent = 0f;
			if (Fermented)
			{
				Log.Warning("Universal Fermenter:: Tried to add ingredient to a fermenter full of product. Colonists should take the product first.");
				return;
			}
			int num = Mathf.Min(count, Product.maxCapacity - ingredientCount);
			if (num <= 0)
			{
				return;
			}
			Progress = GenMath.WeightedAverage(0f, num, Progress, ingredientCount);
			if (Empty)
			{
				GraphicChange(false);
			}
			ingredientCount += num;
		}

		public Thing TakeOutProduct()
		{
			if (!Fermented)
			{
				Log.Warning("Universal Fermenter:: Tried to get product but it's not yet fermented.");
				return null;
			}
			Thing thing = ThingMaker.MakeThing(Product.thingDef, null);
			thing.stackCount = Mathf.RoundToInt(ingredientCount * Product.efficiency);
			Reset();
			return thing;
		}

		public void Reset()
		{
			ingredientCount = 0;
			//ruinedPercent = 0f;			
			Progress = 0f;
			currentResourceInd = nextResourceInd;
			ingredientLabels.Clear();
			GraphicChange(true);
		}

		public void GraphicChange(bool toEmpty)
		{
			string suffix = Product.graphSuffix;
			if (suffix != null)
			{
				string texPath = defaultTexPath;
				if (toEmpty == false)
				{
					texPath += Product.graphSuffix;
				}
				TexReloader.Reload(parent, texPath);
			}
		}

		public override void CompTick()
		{
			base.CompTick();
			DoTicks(1);
		}

		public override void CompTickRare()
		{
			base.CompTickRare();
			DoTicks(250);
		}

		private void DoTicks(int ticks)
		{			
			if (!Empty && Fueled && Powered && FlickedOn)
			{
				Progress = Mathf.Min(Progress + (float)ticks * CurrentProgressPerTick, 1f);
			}
			if (!Ruined)
			{				
				if (!Empty)
				{
					float ambientTemperature = parent.AmbientTemperature;
					if (ambientTemperature > Product.temperatureSafe.max)
					{
						ruinedPercent += (ambientTemperature - Product.temperatureSafe.max) * Product.progressPerDegreePerTick * (float)ticks;
					}
					else if (ambientTemperature < Product.temperatureSafe.min)
					{
						ruinedPercent -= (ambientTemperature - Product.temperatureSafe.min) * Product.progressPerDegreePerTick * (float)ticks;
					}
				}
				if (ruinedPercent >= 1f)
				{
					ruinedPercent = 1f;
					parent.BroadcastCompSignal("RuinedByTemperature");
					Reset();					
				}
				else if (ruinedPercent < 0f)
				{
					ruinedPercent = 0f;
				}
			}
		}

		public override void PreAbsorbStack(Thing otherStack, int count)
		{
			float t = (float)count / (float)(parent.stackCount + count);
			CompUniversalFermenter comp = ((ThingWithComps)otherStack).GetComp<CompUniversalFermenter>();
			ruinedPercent = Mathf.Lerp(ruinedPercent, comp.ruinedPercent, t);
			//ruinedPercent = Mathf.Lerp(ruinedPercent, ruinedPercent, t);
		}

		public override bool AllowStackWith(Thing other)
		{
			CompUniversalFermenter comp = ((ThingWithComps)other).GetComp<CompUniversalFermenter>();
			return Ruined == comp.Ruined;
			//return Ruined == Ruined;
		}

		public override void PostSplitOff(Thing piece)
		{
			CompUniversalFermenter comp = ((ThingWithComps)piece).GetComp<CompUniversalFermenter>();
			comp.ruinedPercent = ruinedPercent;
			//ruinedPercent = ruinedPercent;
		}

		public override IEnumerable<Gizmo> CompGetGizmosExtra()
		{
			// Add a dev button for finishing the fermenting			
			if (Prefs.DevMode && !Empty)
			{
				Command_Action DevFinish = new Command_Action()
				{
					defaultLabel = "DEBUG: Finish",
					activateSound = SoundDef.Named("Click"),
					action = () => { Progress = 1f; },
				};
				yield return DevFinish;
			}

			// Dev button for printing speed factors (speed factors: sun, rain, snow, wind, roofed)
			if (Prefs.DevMode)
			{
				string line = parent.ToString() + ": " +
				              "sun: " + SunRespectSpeedFactor.ToString("0.00") +
				              ", rain: " + RainRespectSpeedFactor.ToString("0.00") +
				              ", snow: " + SnowRespectSpeedFactor.ToString("0.00") +
				              ", wind: " + WindRespectSpeedFactor.ToString("0.00") +
				              ", roofed: " + RoofedFactor.ToString("0.00");
				Command_Action DispSpeeds = new Command_Action()
				{
					defaultLabel = "DEBUG: Display Speed Factors",
					defaultDesc = "Display the current sun, rain, snow and wind speed factors and how much of the building is covered by roof.",
					activateSound = SoundDef.Named("Click"),
					action = () => { Log.Message(line); }
				};
				yield return DispSpeeds;
			}

			// Default buttons
			foreach (Gizmo c in base.CompGetGizmosExtra())
			{
				yield return c;
			}

			// Switching products button (no button if only 1 resource)
			if (ResourceListSize > 1)
			{
				Command_Action CycleProducts = new Command_Action()
				{
					defaultLabel = NextProduct.thingDef.label,
					defaultDesc = "Produce " + NextProduct.thingDef.label + " from " + SummaryNextIngredientFilter + ".",
					activateSound = SoundDef.Named("Click"),
					icon = Utils.GetIcon(NextProduct.thingDef),
					action = () => { NextResource(); },
				};
				yield return CycleProducts;
			}
		}	

		// Inspector string eats max. 5 lines - there is room for one more
		public override string CompInspectStringExtra()
		{			
			StringBuilder stringBuilder = new StringBuilder();

			// 1st line: "Temperature: xx C (Overheating/Freezing/Ideal/Safe)" or "Ruined by temperature"
			stringBuilder.AppendLine(StatusInfo());
						
			// 2nd line: "Contains xx/xx ingredient (product)"
			if (!Empty && !Ruined)
			{
				if (Fermented)
				{
					stringBuilder.AppendLine("UF_ContainsProduct".Translate(new object[]
					{
						ingredientCount,
						Product.maxCapacity,
						Product.thingDef.label
					}));
				}
				else
				{
					stringBuilder.AppendLine("UF_ContainsIngredient".Translate(new object[]
					{
						ingredientCount,
						Product.maxCapacity,
						SummaryAddedIngredients
					}));
				}
			}

			// 3rd line: "Finished" or "Progress: xx %" 
			// 4th line: "Non-ideal temp, sun, ... . Ferm. speed: xx %"
			if (!Empty)
			{
				if (Fermented)
				{
					stringBuilder.AppendLine("UF_Finished".Translate());
				}
				else if (parent.Map != null) // parent.Map is null when minified
				{
					stringBuilder.AppendLine("UF_Progress".Translate(new object[]
					{
						Progress.ToStringPercent(),
						TimeLeft()
						//EstimatedTicksLeft.ToStringTicksToPeriod(true, false, true)
					}));
					if (SpeedFactor != 1f)
					{
						// Should be max. 59 chars in the English translation
						if (SpeedFactor < 1f)
						{
							stringBuilder.Append("UF_NonIdealInfluences".Translate(new object[]
							{
								WhatsWrong()
							})).Append(" ").AppendLine("UF_NonIdealSpeedFactor".Translate(new object[]
							{
								SpeedFactor.ToStringPercent()
							}));
						}
						else
						{
							stringBuilder.AppendLine("UF_NonIdealSpeedFactor".Translate(new object[]
							{
								SpeedFactor.ToStringPercent()
							}));
						}
					}
				}
			}

			// 5th line: "Ideal/safe temperature range"
			stringBuilder.AppendLine(string.Concat(new string[]
			{
				"UF_IdealSafeProductionTemperature".Translate(),
				": ",
				Product.temperatureIdeal.min.ToStringTemperature("F0"),
				"~",
				Product.temperatureIdeal.max.ToStringTemperature("F0"),
				" (",
				Product.temperatureSafe.min.ToStringTemperature("F0"),
				"~",
				Product.temperatureSafe.max.ToStringTemperature("F0"),
				")"
			}));

			return stringBuilder.ToString().TrimEndNewlines();
		}

		public string TimeLeft()
		{
			if (EstimatedTicksLeft >= 0)
			{
				return EstimatedTicksLeft.ToStringTicksToPeriod() + " left";
			}
			else
			{
				return "stopped";
			}
		}
				
		public string WhatsWrong()
		{
			if (SpeedFactor < 1f)
			{
				List<string> wrong = new List<string>();
				if (CurrentTempProgressSpeedFactor < 1f)
				{
					wrong.Add("UF_WeatherTemperature".Translate());
				}
				if (SunRespectSpeedFactor < 1f)
				{
					wrong.Add("UF_WeatherSunshine".Translate());
				}
				if (RainRespectSpeedFactor < 1f)
				{
					wrong.Add("UF_WeatherRain".Translate());
				}
				if (SnowRespectSpeedFactor < 1f)
				{
					wrong.Add("UF_WeatherSnow".Translate());
				}
				if (WindRespectSpeedFactor < 1f)
				{
					wrong.Add("UF_WeatherWind".Translate());
				}
				return string.Join(", ", wrong.ToArray());				
			}
			else
			{
				return "nothing";
			}
		}

		public string StatusInfo()
		{
			if (Ruined)
			{
				return "RuinedByTemperature".Translate();
			}

			float ambientTemperature = parent.AmbientTemperature;
			string str = null;
			string tempStr = "Temperature".Translate() + ": " + ambientTemperature.ToStringTemperature("F0");

			if (!Empty)
			{
				if (Product.temperatureSafe.Includes(ambientTemperature))
				{
					if (Product.temperatureIdeal.Includes(ambientTemperature))
					{
						str = "UF_Ideal".Translate();
					}
					else
					{
						str = "UF_Safe".Translate();
					}
				}
				else
				{
					if (ruinedPercent > 0f)
					{
						if (ambientTemperature < Product.temperatureSafe.min)
						{
							str = "Freezing".Translate();
						}
						else
						{
							str = "Overheating".Translate();
						}
						str = str + " " + ruinedPercent.ToStringPercent();
					}
				}
			}

			if (str == null)
			{
				return tempStr;
			}
			else
			{
				return tempStr + " (" + str + ")";
			}
		}
	}
}
