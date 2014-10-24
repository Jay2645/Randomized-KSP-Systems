using UnityEngine;
using System.Collections.Generic;

namespace RandomizedSystems.Randomizers
{
	/// <summary>
	/// The general randomizer modifies things which don't fit into any other category.
	/// This includes things like names and descriptions.
	/// </summary>
	public class GeneralRandomizer : PlanetRandomizer
	{
		public GeneralRandomizer (CelestialBody body, PlanetData bodyData)
		{
			SetBody (body, bodyData);
		}

		public string name;
		private bool randomizedName = false;

		public override void Cache ()
		{
			name = planet.name;
		}

		public override void Randomize ()
		{
			RandomizeName ();
		}

		public override void Apply ()
		{
			planet.bodyName = name;
		}

		private void RandomizeName ()
		{
			if (randomizedName || Hyperdrive.seedString == AstroUtils.KERBIN_SYSTEM_COORDS)
			{
				return;
			}
			name = WarpRNG.GenerateName ();
			randomizedName = true;
		}

		public string GetName (bool forceRandomize)
		{
			if (forceRandomize)
			{
				if (!randomizedName)
				{
					Randomize ();
				}
			}
			return name;
		}
	}
}

