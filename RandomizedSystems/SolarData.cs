using System.Collections.Generic;
using UnityEngine;

namespace RandomizedSystems
{
	public class SolarData
	{
		public static Dictionary<string,SolarData> solarSystems = new Dictionary<string, SolarData> ();
		public string seed = "";
		public CelestialBody sun = null;
		public PlanetData sunData = null;

		public int planetCount
		{
			get
			{
				return solarSystem.Count;
			}
		}

		private List<PlanetData> solarSystem = null;
		private const string KERBIN_SYSTEM_COORDS = "0";

		/// <summary>
		/// Creates the Kerbin system. Should be called before creating any other system.
		/// </summary>
		public SolarData ()
		{
			// Special case: Kerbin
			this.seed = KERBIN_SYSTEM_COORDS;
			MakeNewSystem ();
			solarSystems.Add (KERBIN_SYSTEM_COORDS, this);
		}

		public SolarData (string seed)
		{
			// Set seed
			this.seed = seed;
			// Make the system
			CreateSystem ();
			// Add to lookup
			solarSystems.Add (seed, this);
		}

		public void CreateSystem ()
		{
			// Make all the planets
			MakeNewSystem ();
			// Randomize everything
			RandomizeSystem ();
			// Apply changes
			ApplySystem ();
		}

		private void MakeNewSystem ()
		{
			solarSystem = new List<PlanetData> ();
			sun = FindSun ();
			sunData = new PlanetData (sun, this, solarSystem.Count);
			solarSystem.Add (sunData);
			CacheAllPlanets (sun, 0);
		}

		private void CacheAllPlanets (CelestialBody currentPlanet, int parentID)
		{
			foreach (CelestialBody child in currentPlanet.orbitingBodies)
			{
				int childID = solarSystem.Count;
				PlanetData planet = new PlanetData (child, this, childID);
				solarSystem.Add (planet);
				solarSystem [parentID].childBodies.Add (child);
				CacheAllPlanets (child, childID);
			}
		}

		private void RandomizeSystem ()
		{
			for (int i = 0; i < solarSystem.Count; i++)
			{
				solarSystem [i].childBodies = new List<CelestialBody> ();
			}
			for (int i = 0; i < solarSystem.Count; i++)
			{
				solarSystem [i].RandomizeValues ();
			}
		}

		private void ApplySystem ()
		{
			for (int i = 0; i < solarSystem.Count; i++)
			{
				solarSystem [i].ApplyChanges ();
			}
		}

		public PlanetData GetPlanetByID (int planetID)
		{
			if (planetID < solarSystem.Count)
			{
				return solarSystem [planetID];
			}
			else
			{
				Debug.LogError (planetID + " does not match up with any planet!");
				return null;
			}
		}

		public void AddChildToPlanet (int planetID, CelestialBody child)
		{
			if (planetID < solarSystem.Count)
			{
				solarSystem [planetID].childBodies.Add (child);
			}
			else
			{
				Debug.LogError (planetID + " does not match up with any planet!");
			}
		}

		public static SolarData CreateSystem (string seed)
		{
			SolarData solarSystem = null;
			if (!solarSystems.ContainsKey (KERBIN_SYSTEM_COORDS))
			{
				solarSystem = new SolarData ();
			}
			if (solarSystems.ContainsKey (seed))
			{
				Debug.LogWarning ("Loading pre-generated system.");
				solarSystem = solarSystems [seed];
				solarSystem.ApplySystem ();
			}
			else
			{
				solarSystem = new SolarData (seed);
			}
			return solarSystem;
		}

		private static CelestialBody FindSun ()
		{
			CelestialBody currentBody = FlightGlobals.currentMainBody;
			while (currentBody.referenceBody != null && currentBody.referenceBody.name != currentBody.name)
			{
				currentBody = currentBody.referenceBody;
			}
			return currentBody;
		}
	}
}

