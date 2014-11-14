using System.Collections.Generic;
using RandomizedSystems.Randomizers;

namespace RandomizedSystems.Systems
{
	public static class SystemNamer
	{
		/// <summary>
		/// Different prefixes for star names.
		/// </summary>
		private static string[] prefixes = new string[] {
			"Ker",
			"Jo",
			"Ear",
			"Ju",
			"Jeb",
			"Plu",
			"Nep",
			"Bes",
			"Tat",
			"Coru",
			"Dego",
			"Ho",
			"Geo",
			"Mu",
			"Usta",
			"Pla",
			"Gal",
			"Rea",
			"Olym",
			"Mor",
			"Mar",
			"Jup",
			"Sa",
			"Sat",
			"Kry",
			"Ee",
			"Su",
			"Spar",
			"He",
			"Xa",
			"Sak",
			"So",
			"Ha",
			"Kor",
			"Ath",
			"Chand",
			"Rig",
			"Ven",
			"Den",
			"C",
			"Holo",
			"Korr",
			"Ran",
		};
		/// <summary>
		/// Different suffixes for star names.
		/// </summary>
		private static string[] suffixes = new string[] {
			"bin",
			"ol",
			"th",
			"to",
			"ne",
			"in",
			"ant",
			"bah",
			"sis",
			"n",
			"os",
			"ch",
			"dor",
			"vin",
			"s",
			"ury",
			"us",
			"it",
			"er",
			"urn",
			"une",
			"pau",
			"far",
			"ton",
			"lou",
			"vin",
			"tax",
			"dar",
			"aar",
			"l",
			"la",
			"nth",
			"il",
			"ifrey",
			"nus",
			"neb",
			"ron",
			"ii",
			"iban",
			"trax",
			"turus",
			"clos",
			"daa",
		};
		private static string[] romanNumerals = new string[] {
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
		public static Dictionary<CelestialBody, int> moonCount = new Dictionary<CelestialBody, int> ();
		/// <summary>
		/// This is a list of which bodies orbit which other bodies.
		/// For example, the sun would have all the planets, each planet would have its moons, etc.
		/// All these are sorted by the length semi-major axis.
		/// </summary>
		private static Dictionary<int, PlanetData[]> planetaryBodies = new Dictionary<int, PlanetData[]> ();

		/// <summary>
		/// Generates a random name, composed of prefix + suffix.
		/// </summary>
		/// <returns>A name.</returns>
		public static string GenerateName ()
		{
			string prefix = prefixes [WarpRNG.GenerateInt (0, prefixes.Length)];
			string suffix = suffixes [WarpRNG.GenerateInt (0, suffixes.Length)];
			return prefix + suffix;
		}

		public static void RegisterPlanet (PlanetData data)
		{
			if (data.IsSun ())
			{
				return;
			}
			PlanetData referenceBody = data.referenceBodyData;
			PlanetData[] currentPlanetsOrbitingBody = new PlanetData[0];
			if (planetaryBodies.ContainsKey (referenceBody.planetID))
			{
				currentPlanetsOrbitingBody = planetaryBodies [referenceBody.planetID];
			}
			List<PlanetData> planetsOrbitingBody = new List<PlanetData> (currentPlanetsOrbitingBody);
			planetsOrbitingBody.Add (data);
			planetaryBodies [referenceBody.planetID] = planetsOrbitingBody.ToArray ();
		}

		/// <summary>
		/// Names all the planets. This makes sure planets are named before the moons.
		/// </summary>
		/// <param name="system">System.</param>
		public static void NamePlanets (SolarData system)
		{
			if (!planetaryBodies.ContainsKey (system.sunData.planetID))
			{
				return;
			}
			SortPlanetsByDistance ();
			PlanetData[] planets = planetaryBodies [system.sunData.planetID];
			for (int i = 0; i < planets.Length; i++)
			{
				NameBody (planets [i], true);
			}
			planetaryBodies [system.sunData.planetID] = planets;
		}

		public static void NameBody (PlanetData planet, bool skipPlanetCheck = false)
		{
			if (planet.IsSun () || planet.solarSystem.IsKerbol ())
			{
				return;
			}
			if (!skipPlanetCheck && planetaryBodies.ContainsKey (planet.solarSystem.sunData.planetID))
			{
				// Haven't named all the planets yet, need to do that first
				NamePlanets (planet.solarSystem);
			}
			if (!planetaryBodies.ContainsKey (planet.referenceBodyData.planetID))
			{
				// This would be removed if we are a planet which has already been named
				return;
			}
			string referenceName = planet.referenceBodyData.name;
			if (string.IsNullOrEmpty (referenceName))
			{
				Debugger.LogWarning ("Reference name is empty! " +
					"Planet: " + planet.planetID + ", " +
					"reference body: " + planet.referenceBodyData.name + ", " +
					"ID " + planet.referenceBodyData.planetID);
				PlanetData referenceBody = planet.solarSystem.GetPlanetByID (planet.referenceBodyData.planetID);
				referenceName = referenceBody.planet.bodyName;
			}
			// Get list of all "sibling" bodies (bodies orbiting the same reference body)
			PlanetData[] siblingBodies = planetaryBodies [planet.referenceBodyData.planetID];
			int index = 0;
			for (int i = 0; i < siblingBodies.Length; i++)
			{
				if (siblingBodies [i].planetID == planet.planetID)
				{
					index = i;
					break;
				}
			}
			string name = referenceName + " ";
			if (planet.IsMoon ())
			{
				while (index >= romanNumerals.Length)
				{
					name += romanNumerals [romanNumerals.Length - 1];
					index -= romanNumerals.Length;
				}
				name += romanNumerals [index];
			}
			else
			{
				int systemNameIndex = 98; // "b" in ASCII
				// Change the letter to match the distance from the sun
				systemNameIndex += index;
				name += ((char)systemNameIndex);
			}
			planet.solarSystem.NamePlanet (planet.planetID, name);
			planet.name = name;
			planet.planet.bodyName = name;
		}

		private static void SortPlanetsByDistance ()
		{
			Dictionary<int, PlanetData[]> sortedBodies = new Dictionary<int, PlanetData[]> ();
			foreach (KeyValuePair<int, PlanetData[]> kvp in planetaryBodies)
			{
				PlanetData[] system = kvp.Value;
				PlanetData temp = null;
				for (int i = 0; i < system.Length; i++)
				{
					double semiMajorAxis = system [i].semiMajorAxis;
					for (int j = i + 1; j < system.Length; j++)
					{
						double otherSemiMajorAxis = system [j].semiMajorAxis;
						if (semiMajorAxis > otherSemiMajorAxis)
						{
							temp = system [i];
							system [i] = system [j];
							system [j] = temp;
						}
					}
				}
				sortedBodies.Add (kvp.Key, system);
			}
		}
	}
}

