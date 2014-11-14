using System.Collections.Generic;
using UnityEngine;
using RandomizedSystems.Vessels;

namespace RandomizedSystems.Systems
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

		public static SolarData currentSystem
		{
			get
			{
				string seed = WarpDrivers.WarpDrive.seedString;
				if (solarSystems.ContainsKey (seed))
				{
					return solarSystems [seed];
				}
				return null;
			}
		}

		public bool debug = false;

		public FlightState flightState
		{
			get
			{
				return HighLogic.CurrentGame.flightState;
			}
			set
			{
				HighLogic.CurrentGame.flightState = value;
			}
		}

		private const string KERBOL_NAME = "Sun";

		/// <summary>
		/// Gets the name of this solar system.
		/// </summary>
		/// <value>The name of this solar system.</value>
		public string name
		{
			get
			{
				if (sun == null || sun.name.ToLower () == "sun")
				{
					return "Kerbol";
				}
				else
				{
					return sun.name;
				}
			}
		}

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

		/// <summary>
		/// Creates the Kerbin system. Should be called before creating any other system.
		/// </summary>
		public SolarData ()
		{
			// Special case: Kerbin
			this.seed = AstroUtils.KERBIN_SYSTEM_COORDS;
			MakeNewSystem ();
			foreach (PlanetData planet in solarSystem)
			{
				planet.name = planet.planet.bodyName;
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="RandomizedSystems.SolarData"/> class.
		/// </summary>
		/// <param name="seed">The seed to use to generate the solar system.</param>
		public SolarData (string seed, bool debug = false)
		{
			// Set seed
			this.seed = seed;
			this.debug = debug;
			// Make the system
			CreateSystem ();
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
			// Add to lookup
			solarSystems [seed] = this;
			sunData = new PlanetData (sun, seed, solarSystem.Count);
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
				PlanetData planet = new PlanetData (child, seed, childID);
				solarSystem.Add (planet);
				solarSystem [parentID].childBodies.Add (child);
				solarSystem [parentID].childDataIDs.Add (childID);
				CacheAllPlanets (child, childID);
			}
		}

		private void RandomizeSystem ()
		{
			if (seed == AstroUtils.KERBIN_SYSTEM_COORDS)
			{
				return;
			}
			SystemNamer.moonCount.Clear ();
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
			foreach (PlanetData data in solarSystem)
			{
				data.planet.orbitingBodies.Clear ();
			}
			SystemNamer.NamePlanets (this);
			for (int i = 0; i < solarSystem.Count; i++)
			{
				solarSystem [i].ApplyChanges ();
			}
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
				Debugger.LogError (planetID + " does not match up with any planet!", "SolarData.GetPlanetByID()");
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
				for (int i = 0; i < solarSystem.Count; i++)
				{
					PlanetData planet = solarSystem [i];
					if (planet.childBodies.Contains (child))
					{
						planet.childBodies.Remove (child);
					}
					solarSystem [i] = planet;
				}
				solarSystem [planetID].childBodies.Add (child);
			}
			else
			{
				Debugger.LogError (planetID + " does not match up with any planet!", "SolarData.AddChildToPlanet()");
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
				Debugger.LogError (planetID + " does not match up with any planet!", "SolarData.AdjustPlanetSOI()");
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
				Debugger.LogError (planetID + " does not match up with any planet!", "SolarData.AdjustPlanetGravity()");
			}
		}

		public void NamePlanet (int planetID, string planetName)
		{
			if (planetID < solarSystem.Count)
			{
				solarSystem [planetID].name = name;
			}
			else
			{
				Debugger.LogError (planetID + " does not match up with any planet!", "SolarData.AdjustPlanetGravity()");
			}
		}

		/// <summary>
		/// Creates a solar system from a seed.
		/// </summary>
		/// <returns>The newly-created system.</returns>
		/// <param name="seed">The seed to use.</param>
		public static SolarData CreateSystem (string seed, bool debug = false)
		{
			SolarData solarSystem = null;
			if (solarSystems.ContainsKey (seed))
			{
				solarSystem = solarSystems [seed];
				solarSystem.debug = debug;
				solarSystem.ApplySystem ();
			}
			else if (seed == AstroUtils.KERBIN_SYSTEM_COORDS && 
				FindSun ().name == KERBOL_NAME)
			{
				solarSystem = new SolarData ();
			}
			else
			{
				solarSystem = new SolarData (seed, debug);
			}
			return solarSystem;
		}

		private static CelestialBody FindSun ()
		{
			CelestialBody currentBody = FlightGlobals.getMainBody ();
			while (currentBody.referenceBody.name != currentBody.name)
			{
				currentBody = currentBody.referenceBody;
			}
			return currentBody;
		}

		public bool IsKerbol ()
		{
			return seed == AstroUtils.KERBIN_SYSTEM_COORDS;
		}
	}
}

