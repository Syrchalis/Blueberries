using System;
using System.Reflection;

using Verse;

namespace UniversalFermenter
{
	public static class TexReloader
	{
		public static void Reload(Thing t, string texPath)
		{			
			Graphic graphic = GraphicDatabase.Get(t.def.graphicData.graphicClass, texPath, ShaderDatabase.LoadShader(t.def.graphicData.shaderType.shaderPath), t.def.graphicData.drawSize, t.DrawColor, t.DrawColorTwo);
			typeof(Thing).GetField("graphicInt", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(t, graphic);
			if (t.Map != null)
			{
				t.DirtyMapMesh(t.Map);
			}
		}
	}
}