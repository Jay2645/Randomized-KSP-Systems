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
		private string[] romanNumerals = new string[] {
			"0",
			"I",
			"II",
			"III",
			"IV",
			"V",
			"VI",
			"VII",
			"VIII",
			"IX",
			"X"
		};

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
			SortSystem ();
			string systemName = solarSystem [0].name;
			int systemNameIndex = 98; // "b" in ASCII
			for (int i = 1; i < solarSystem.Count; i++)
			{
				PlanetData planet = solarSystem [i];
				if (planet.referenceBody.name == sun.name)
				{
					NamePlanet (ref planet, systemName, systemNameIndex);
					systemNameIndex++;
					solarSystem [i] = planet;
				}
			}
		}

		private void ApplySystem ()
		{
			for (int i = 0; i < solarSystem.Count; i++)
			{
				solarSystem [i].ApplyChanges ();
			}
		}

		public void SortSystem ()
		{
			PlanetData[] allPlanets = solarSystem.ToArray ();
			Quicksort (ref allPlanets, 0, allPlanets.Length - 1);
			solarSystem = new List<PlanetData> (allPlanets);
		}

		private void NamePlanet (ref PlanetData planet, string systemName, int systemNameIndex)
		{
			planet.name = systemName + " " + ((char)systemNameIndex);
			int moonCount = 0;
			foreach (CelestialBody moon in planet.childBodies)
			{
				for (int i = 0; i < solarSystem.Count; i++)
				{
					PlanetData moonData = solarSystem [i];
					if (moonData.planet == moon)
					{
						moonCount++;
						NameMoon (ref moonData, systemName + " " + ((char)systemNameIndex), moonCount);
						solarSystem [i] = moonData;
						break;
					}
				}
			}
		}

		private void NameMoon (ref PlanetData moon, string planetName, int count)
		{
			string name = planetName + " ";
			while (count >= 10)
			{
				name += "X";
				count -= 10;
			}
			name += romanNumerals [count];
			moon.name = name;
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

		public void AdjustPlanetSOI (int planetID, double newSOI)
		{
			if (planetID < solarSystem.Count)
			{
				solarSystem [planetID].sphereOfInfluence = newSOI;
			}
			else
			{
				Debug.LogError (planetID + " does not match up with any planet!");
			}
		}

		public void AdjustPlanetGravity (int planetID, double newGravity)
		{
			if (planetID < solarSystem.Count)
			{
				solarSystem [planetID].gravity = newGravity;
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

		private static void Quicksort (ref PlanetData[] elements, int left, int right)
		{
			int i = left, j = right;
			PlanetData pivot = elements [(left + right) / 2];

			while (i <= j)
			{
				while (elements[i].orbitData.semiMajorAxis < pivot.orbitData.semiMajorAxis)
				{
					i++;
				}

				while (elements[j].orbitData.semiMajorAxis > pivot.orbitData.semiMajorAxis)
				{
					j--;
				}

				if (i <= j)
				{
					// Swap
					PlanetData tmp = elements [i];
					elements [i] = elements [j];
					elements [j] = tmp;

					i++;
					j--;
				}
			}

			// Recursive calls
			if (left < j)
			{
				Quicksort (ref elements, left, j);
			}

			if (i < right)
			{
				Quicksort (ref elements, i, right);
			}
		}
	}
}

