using System.Collections.Generic;
//using System.Reflection;
using System.Linq;

using UnityEngine;
using Verse;

namespace UniversalFermenter
{
	public static class Utils
	{

		public static string IngredientFilterSummary(ThingFilter thingFilter)
		{
			/*FieldInfo fi = thingFilter.GetType().GetField("categories", BindingFlags.NonPublic | BindingFlags.Instance);
			var summaryList = (List<string>) fi.GetValue(thingFilter);
			fi = thingFilter.GetType().GetField("categories", BindingFlags.NonPublic | BindingFlags.Instance);
			var thingDefs = (List<string>) fi.GetValue(thingFilter);*/
			
			//if (thingDefs != null)
			//{/
			/*List<ThingDef> thingDefsList = (List<ThingDef>) thingDefs;
			if (thingDefsList == null)
			{
				Log.Message("thingDefList null: " + thingDefs.GetType().Name);
			}
			else
			{
				Log.Message("yes: " + thingDefs.GetType().Name);
			}*/
			/*}
			else
			{
				Log.Message("thingDefs null");
			}*/
			/*if (summaryList == null)
				summaryList = new List<string>();
			if (thingDefs == null)
				thingDefs = new List<string>();*/
			
			/*foreach (string thingDef in thingDefs)
			{
				summaryList.Add(thingDef);
			}*/
			
			return thingFilter.Summary;
		}

		public static string VowelTrim(string str, int limit)
		{
			int vowelsToRemove = str.Length - limit;
			for (int i = str.Length - 1; i > 0; i--)
			{
				if (vowelsToRemove <= 0)
					break;			

				if (IsVowel(str[i]))
				{
					if (str[i - 1] == ' ')
					{
						continue;
					}
					else
					{
						str = str.Remove(i, 1);						
						vowelsToRemove--;
					}
				}
			}

			if (str.Length > limit)
			{
				str = str.Remove(limit-2) + "..";
			}

			return str;
		}

		public static bool IsVowel(char c)
		{
			var vowels = new HashSet<char> { 'a', 'e', 'i', 'o', 'u' };
			return vowels.Contains(c);
		}

		// Try to get a texture of a thingDef; If not found, use LaunchReport icon
		public static Texture2D GetIcon(ThingDef thingDef)
		{
			Texture2D icon = ContentFinder<Texture2D>.Get(thingDef.graphicData.texPath, false);
			if (icon == null)
			{
				// Use the first texture in the folder
				icon = ContentFinder<Texture2D>.GetAllInFolder(thingDef.graphicData.texPath).ToList()[0];
				if (icon == null)
				{
					icon = ContentFinder<Texture2D>.Get("UI/Commands/LaunchReport", true);
					Log.Warning("Universal Fermenter:: No texture at " + thingDef.graphicData.texPath + ".");
				}
			}
			return icon;
		}
	}
}