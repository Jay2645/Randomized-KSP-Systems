using System;

namespace RandomizedSystems
{
	/// <summary>
	/// Randomizes values pertaining to a planet, then updates them.
	/// </summary>
	public abstract class PlanetRandomizer
	{
		protected PlanetRandomizer ()
		{
			/* This method intentionally left blank */
		}

		public abstract void UpdatePlanet ();
	}
}

