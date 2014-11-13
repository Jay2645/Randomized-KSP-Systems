using System;
using RandomizedSystems.Systems;

namespace RandomizedSystems.Randomizers
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

		protected CelestialBody planet;
		protected PlanetData planetData;

		protected SolarData solarSystem
		{
			get
			{
				return planetData.solarSystem;
			}
		}

		protected void SetBody (CelestialBody body, PlanetData bodyData)
		{
			this.planet = body;
			this.planetData = bodyData;
		}

		/// <summary>
		/// Caches all planet values.
		/// </summary>
		public abstract void Cache ();

		/// <summary>
		/// Randomizes all planet values.
		/// </summary>
		public abstract void Randomize ();

		/// <summary>
		/// Applies the random values.
		/// </summary>
		public abstract void Apply ();

		public bool IsSun ()
		{
			// The sun orbits itself
			return planetData.IsSun ();
		}

		public bool IsMoon ()
		{
			return planetData.IsMoon ();
		}
	}
}

