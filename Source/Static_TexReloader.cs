using System;
using System.Reflection;

using Verse;

namespace Kubouch
{
	public static class TexReloader
	{
		public static void Reload(Thing t, string texPath)
		{			
			Graphic graphic = GraphicDatabase.Get(t.def.graphicData.graphicClass, texPath, ShaderDatabase.ShaderFromType(t.def.graphicData.shaderType), t.def.graphicData.drawSize, t.DrawColor, t.DrawColorTwo);
			typeof(Thing).GetField("graphicInt", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(t, graphic);
			if (t.Map != null)
			{
				t.DirtyMapMesh(t.Map);
			}
		}
	}
}