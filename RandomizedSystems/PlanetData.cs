using UnityEngine;
using System.Collections.Generic;
using System;
using RandomizedSystems.Randomizers;

namespace RandomizedSystems
{
	public class PlanetData
	{
		public SolarData solarSystem;
		public CelestialBody planet;
		public int planetID = -1;
		public int moonCount = 0;
		/// <summary>
		/// This keeps track of the Planet IDs of our child bodies, for easy lookup.
		/// </summary>
		public List<int> childDataIDs = new List<int> ();
		#region Randomizers
		protected List<PlanetRandomizer> allRandomizers = new List<PlanetRandomizer> ();
		// We always want to reference the randomizer in our list
		public AtmosphereRandomizer atmosphereRandomizer
		{
			get
			{
				if (atmoRandomizerIndex == -1)
				{
					return null;
				}
				return (AtmosphereRandomizer)allRandomizers [atmoRandomizerIndex];
			}
			protected set
			{
				atmoRandomizerIndex = allRandomizers.Count;
				allRandomizers.Add (value);
			}
		}

		private int atmoRandomizerIndex = -1;

		public GeneralRandomizer generalRandomizer
		{
			get
			{
				if (generalRandomizerIndex == -1)
				{
					return null;
				}
				return (GeneralRandomizer)allRandomizers [generalRandomizerIndex];
			}
			protected set
			{
				generalRandomizerIndex = allRandomizers.Count;
				allRandomizers.Add (value);
			}
		}

		private int generalRandomizerIndex = -1;

		public GeologicalRandomizer geologicalRandomizer
		{
			get
			{
				if (geoRandomizerIndex == -1)
				{
					return null;
				}
				return (GeologicalRandomizer)allRandomizers [geoRandomizerIndex];
			}
			protected set
			{
				geoRandomizerIndex = allRandomizers.Count;
				allRandomizers.Add (value);
			}
		}

		private int geoRandomizerIndex = -1;

		public OrbitRandomizer orbitRandomizer
		{
			get
			{
				if (orbitRandomizerIndex == -1)
				{
					return null;
				}
				return (OrbitRandomizer)allRandomizers [orbitRandomizerIndex];
			}
			protected set
			{
				orbitRandomizerIndex = allRandomizers.Count;
				allRandomizers.Add (value);
			}
		}

		private int orbitRandomizerIndex = -1;
		#endregion
		#region Randomizer Accessors
		// These are here for ease of use
		public CelestialBody referenceBody
		{
			get
			{
				if (orbitRandomizer == null)
				{
					return null;
				}
				orbitRandomizer.CreateOrbit ();
				return orbitRandomizer.referenceBody;
			}
		}

		public PlanetData referenceBodyData
		{
			get
			{
				if (orbitRandomizer == null)
				{
					return null;
				}
				orbitRandomizer.CreateOrbit ();
				return orbitRandomizer.referenceBodyData;
			}
		}

		public double sphereOfInfluence
		{
			get
			{
				if (orbitRandomizer == null)
				{
					return 0;
				}
				// This will only randomize if we haven't already
				orbitRandomizer.CreateOrbit ();
				return orbitRandomizer.sphereOfInfluence;
			}
			set
			{
				orbitRandomizer.sphereOfInfluence = value;
			}
		}

		public double semiMajorAxis
		{
			get
			{
				if (orbitRandomizer == null)
				{
					return 0;
				}
				orbitRandomizer.Randomize ();
				return orbitRandomizer.orbitData.semiMajorAxis;
			}
		}

		public double gravityMultiplier
		{
			get
			{
				if (orbitRandomizer == null)
				{
					return 0;
				}
				orbitRandomizer.Randomize ();
				return orbitRandomizer.gravityMultiplier;
			}
		}

		public double eccentricity
		{
			get
			{
				if (orbitRandomizer == null)
				{
					return 0;
				}
				orbitRandomizer.CreateOrbit ();
				return orbitRandomizer.orbitData.eccentricity;
			}
		}

		public string name
		{
			get
			{
				if (generalRandomizer == null)
				{
					return string.Empty;
				}
				return generalRandomizer.GetName (true);
			}
			set
			{
				generalRandomizer.name = value;
			}
		}

		public List<CelestialBody> childBodies
		{
			get
			{
				if (orbitRandomizer == null)
				{
					return null;
				}
				orbitRandomizer.CreateOrbit ();
				return orbitRandomizer.childBodies;
			}
			set
			{
				orbitRandomizer.childBodies = value;
			}
		}

		public double gravity
		{
			get
			{
				if (orbitRandomizer == null)
				{
					return 0;
				}
				orbitRandomizer.CreateOrbit ();
				return orbitRandomizer.gravity;
			}
			set
			{
				orbitRandomizer.gravity = value;
			}
		}
		#endregion
		public PlanetData (CelestialBody planet, SolarData system, int id)
		{
			this.planetID = id;
			this.solarSystem = system;
			this.planet = planet;

			// From here we add our randomizers
			orbitRandomizer = new OrbitRandomizer (planet, this);
			geologicalRandomizer = new GeologicalRandomizer (planet, this);
			generalRandomizer = new GeneralRandomizer (planet, this);
			atmosphereRandomizer = new AtmosphereRandomizer (planet, this);

			foreach (PlanetRandomizer randomizer in allRandomizers)
			{
				randomizer.Cache ();
			}
		}

		public void RandomizeValues ()
		{
			foreach (PlanetRandomizer randomizer in allRandomizers)
			{
				randomizer.Randomize ();
			}
		}

		public void ApplyChanges ()
		{
			string output = "Planet: " + name;
			if (IsSun ())
			{
				output = "Star: " + name;
			}
			Debugger.LogWarning (output, "PlanetData.ApplyChanges()");
			foreach (PlanetRandomizer randomizer in allRandomizers)
			{
				randomizer.Apply ();
			}
		}

		public bool IsSun ()
		{
			// The sun orbits itself
			return AstroUtils.IsSun (planet);
		}

		public bool IsMoon ()
		{
			// If our reference body is *not* the sun, we are a moon
			return referenceBody.name != solarSystem.sun.name;
		}
	}
}

