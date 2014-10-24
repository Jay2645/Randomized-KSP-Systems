using System.Collections.Generic;
using UnityEngine;

namespace RandomizedSystems
{
	public class SolarData
	{
		/// <summary>
		/// A dictionary containing all solar systems we have generated.
		/// </summary>
		public static Dictionary<string,SolarData> solarSystems = new Dictionary<string, SolarData> ();
		/// <summary>
		/// The seed for this solar system.
		/// </summary>
		public string seed = "";
		/// <summary>
		/// The sun of this solar system.
		/// </summary>
		public CelestialBody sun = null;
		/// <summary>
		/// The PlanetData corresponding with the sun.
		/// </summary>
		public PlanetData sunData = null;

		/// <summary>
		/// Gets the name of this solar system.
		/// </summary>
		/// <value>The name of this solar system.</value>
		public string name
		{
			get
			{
				if (sun == null)
				{
					return "Kerbol";
				}
				else
				{
					return sun.name;
				}
			}
		}

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

		/// <summary>
		/// Gets the total planet count.
		/// </summary>
		/// <value>The planet count.</value>
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

		/// <summary>
		/// Initializes a new instance of the <see cref="RandomizedSystems.SolarData"/> class.
		/// </summary>
		/// <param name="seed">The seed to use to generate the solar system.</param>
		public SolarData (string seed)
		{
			// Set seed
			this.seed = seed;
			// Make the system
			CreateSystem ();
			// Add to lookup
			solarSystems.Add (seed, this);
		}

		/// <summary>
		/// Creates a solar system.
		/// </summary>
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
			CacheAllPlanets (sun, sunData.planetID);
		}

		private void CacheAllPlanets (CelestialBody currentPlanet, int parentID)
		{
			int count = 0;
			foreach (CelestialBody child in currentPlanet.orbitingBodies)
			{
				count++;
				int childID = solarSystem.Count;
				PlanetData planet = new PlanetData (child, this, childID);
				solarSystem.Add (planet);
				solarSystem [parentID].childBodies.Add (child);
				solarSystem [parentID].childDataIDs.Add (childID);
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
				if (!planet.IsSun () && !planet.IsMoon ())
				{
					// Name planets
					NamePlanet (ref planet, systemName, systemNameIndex);
					systemNameIndex++;
					solarSystem [i] = planet;
				}
			}
			for (int i = 1; i < solarSystem.Count; i++)
			{
				PlanetData moon = solarSystem [i];
				if (moon.IsMoon ())
				{
					NameMoon (ref moon, moon.referenceBodyData);
					solarSystem [i] = moon;
					break;
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

		private void SortSystem ()
		{
			PlanetData[] allPlanets = solarSystem.ToArray ();
			Quicksort (ref allPlanets, 1, allPlanets.Length - 1);
			solarSystem = new List<PlanetData> (allPlanets);
		}

		private void NamePlanet (ref PlanetData planet, string systemName, int systemNameIndex)
		{
			planet.name = systemName + " " + ((char)systemNameIndex);
		}

		private void NameMoon (ref PlanetData moon, PlanetData planet)
		{
			solarSystem [planet.planetID].moonCount++;
			NameMoon (ref moon, planet.name, solarSystem [planet.planetID].moonCount);
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

		/// <summary>
		/// Gets the planet by its ID.
		/// </summary>
		/// <returns>A planet, based on its ID.</returns>
		/// <param name="planetID">The planet ID to use.</param>
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

		public PlanetData GetPlanetByCelestialBody (CelestialBody body)
		{
			if (body == null)
			{
				return null;
			}
			foreach (PlanetData planetData in solarSystem)
			{
				if (planetData.planet == body)
				{
					return planetData;
				}
			}
			return null;
		}

		/// <summary>
		/// Adds a moon or other child body orbiting a planet.
		/// </summary>
		/// <param name="planetID">The ID of the parent planet.</param>
		/// <param name="child">The child to put into orbit.</param>
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

		public void AddChildToPlanet (PlanetData parent, CelestialBody child)
		{
			AddChildToPlanet (parent.planetID, child);
		}

		/// <summary>
		/// Adjusts a planet's sphere of influence.
		/// </summary>
		/// <param name="planetID">The planet ID to adjust.</param>
		/// <param name="newSOI">The new Sphere of Influence.</param>
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

		/// <summary>
		/// Adjusts a planet's gravity.
		/// </summary>
		/// <param name="planetID">The planet ID to adjust.</param>
		/// <param name="newGravity">The new gravity.</param>
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

		/// <summary>
		/// Creates a solar system from a seed.
		/// </summary>
		/// <returns>The newly-created system.</returns>
		/// <param name="seed">The seed to use.</param>
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
			while (currentBody.referenceBody.name != currentBody.name)
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
				while (elements[i].semiMajorAxis < pivot.semiMajorAxis)
				{
					i++;
				}

				while (elements[j].semiMajorAxis > pivot.semiMajorAxis)
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

