using System;
namespace ManicDigger.Mods
{
	/// <summary>
	/// This is a fix for backward compatibility issues of old Mods.
	/// Do not reference this in your Mods. Instead reference the corresponding Core*.cs
	/// </summary>
	public class Default : IMod
	{
		public void PreStart(ModManager m)
		{
			m.RequireMod("Core");
			m.RequireMod("CoreBlocks");
			m.RequireMod("CoreCrafting");
		}
		public void Start(ModManager manager){ }
	}
}
