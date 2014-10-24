using UnityEngine;
using System.Collections.Generic;

namespace RandomizedSystems.Randomizers
{
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

