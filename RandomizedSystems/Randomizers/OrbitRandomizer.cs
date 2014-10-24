using UnityEngine;
using System.Collections.Generic;

namespace RandomizedSystems.Randomizers
{
	public class OrbitRandomizer : PlanetRandomizer
	{
		public OrbitRandomizer (CelestialBody body, PlanetData bodyData)
		{
			SetBody (body, bodyData);
		}

		public struct OrbitData
		{
			public double inclination;
			public double eccentricity;
			public double semiMajorAxis;
			public double longitudeAscendingNode;
			public double argumentOfPeriapsis;
			public double meanAnomalyAtEpoch;
			public double epoch;
			public double period;
			public CelestialBody referenceBody;
			public bool randomized;
		}

		/// <summary>
		/// The size of our sphere of influence.
		/// </summary>
		public double sphereOfInfluence;
		public double gravity;

		/// <summary>
		/// Expresses gravity in relation to Kerbin's stock gravity.
		/// </summary>
		/// <value>How much more (or less) this planet's gravity is compared to Kerbin.</value>
		public double gravityMultiplier
		{
			get
			{
				return gravity / AstroUtils.KERBIN_GRAVITY;
			}
		}

		public CelestialBody referenceBody;

		public PlanetData referenceBodyData
		{
			get;
			protected set;
		}

		public List<CelestialBody> childBodies = new List<CelestialBody> ();
		public OrbitData orbitData;
		protected Orbit orbit;
		protected OrbitDriver orbitDriver;

		public override void Cache ()
		{
			if (!IsSun ())
			{
				orbit = planet.GetOrbit ();
				orbitData = OrbitDataFromOrbit (orbit);
				orbitDriver = planet.orbitDriver;
				referenceBody = orbit.referenceBody;
				referenceBodyData = solarSystem.GetPlanetByCelestialBody (referenceBody);
			}
			else
			{
				referenceBody = solarSystem.sun;
				referenceBodyData = solarSystem.sunData;
			}
			gravity = planet.gravParameter;
			sphereOfInfluence = planet.sphereOfInfluence;
		}

		public override void Randomize ()
		{
			CreateOrbit ();
		}

		public void CreateOrbit ()
		{
			if (orbitData.randomized)
			{
				// Already randomized data
				return;
			}
			//orbitData = new OrbitData ();
			orbitData.randomized = true;
			if (IsSun ())
			{
				// Special case
				orbitData.referenceBody = solarSystem.sun;
				orbitData.semiMajorAxis = 0;
				return;
			}
			float value = WarpRNG.GetValue ();
			#region Reference Body
			referenceBodyData = null;
			referenceBody = null;
			// Planet is in a solar orbit if any of these are true:
			// 1. RNG rolls a value above at or below 0.25 (25% chance)
			// 2. There is only one planet in the solar system (should never happen).
			// 3. We already have a moon orbiting us (no moons orbiting other moons)
			if (value <= 0.25f || solarSystem.planetCount <= 1 || childBodies.Count > 0)
			{
				referenceBody = solarSystem.sun;
				referenceBodyData = solarSystem.sunData;
			}
			else
			{
				// We will be a moon
				List<int> attemptedInts = new List<int> ();
				int attempts = 0;

				// Toss out a candidate if any of the following is true:
				// 1. The reference body is null or us (causes KSP to crash)
				// 2. The reference body is a moon
				// 3. The reference body is smaller than us
				// Move us to solar orbit after 100 attempts.
				while ((referenceBody == null || referenceBody == planet || referenceBodyData.referenceBody != solarSystem.sun || referenceBody.Radius < planet.Radius))
				{
					attempts++;
					// Keep track of already-attempted planets
					// Might change this to pull a list of all planets from the solar system and poll that
					int index = WarpRNG.GenerateInt (0, solarSystem.planetCount);
					if (attemptedInts.Contains (index))
					{
						continue;
					}
					attemptedInts.Add (index);
					// Get the planet dictated by the random int
					referenceBodyData = solarSystem.GetPlanetByID (index);
					referenceBody = referenceBodyData.planet;
					if (attempts >= 100)
					{
						referenceBody = solarSystem.sun;
						referenceBodyData = solarSystem.sunData;
						break;
					}
					// Loop will do a logic check to make sure the chosen planet is valid
					// Will continue iterating until we have found a valid planet
				}
			}
			// Notify the solar system and the planet itself that our reference body has a new body orbiting it
			solarSystem.AddChildToPlanet (referenceBodyData, planet);
			// Update orbital data
			orbitData.referenceBody = referenceBody;
			#endregion
			#region Gravity
			float gravityMult = 0.0f;
			if (IsMoon ())
			{
				// Moons in KSP for the most part have SOIs which are greater than their real-life counterparts
				// SOI -> Gravity is not a 1:1 ratio; instead a moon's SOI is usually 7-8 times more powerful than its gravity
				// To skew the gravity data for moons, we use the formula y = (0.0788628 * x^2)-(0.788279 * x)+1.58089
				// Note that values below 7.25 generate negative multipliers
				float randomGravity = WarpRNG.GenerateFloat (7.25f, 9f);
				gravityMult = (0.0788628f * randomGravity * randomGravity) - (0.788279f * randomGravity) + 1.58089f;
			}
			else
			{
				gravityMult = WarpRNG.GenerateFloat (0.15f, 2.0f);
				value = WarpRNG.GetValue ();
				// There is a chance that a planet is a gas giant like Jool
				if (value <= 0.05f)
				{
					gravityMult *= 20.0f;
				}
			}
			// All gravity values are relative to Kerbin
			gravity = gravityMult * AstroUtils.KERBIN_GRAVITY;
			#endregion
			#region Inclination
			// Inclination starts directly at orbital plane
			int inclination = 0;
			// Get new random value
			value = WarpRNG.GetValue ();
			if (value >= 0.975f)
			{
				// 2.5% chance of orbit being between 0 and 180 degrees
				inclination = WarpRNG.GenerateInt (0, 180);
			}
			else if (value >= 0.95f)
			{
				// 2.5% chance of orbit being between 0 and 60 degrees
				inclination = WarpRNG.GenerateInt (0, 60);
			}
			else if (value >= 0.925f)
			{
				// 2.5% chance of orbit being between 0 and 45 degrees
				inclination = WarpRNG.GenerateInt (0, 45);
			}
			else if (value >= 0.9f)
			{
				// 2.5% chance or orbit being between 0 and 25 degrees
				inclination = WarpRNG.GenerateInt (0, 25);
			}
			else if (value >= 0.6f)
			{
				// 30% chance of orbit being between 0 and 10 degrees
				inclination = WarpRNG.GenerateInt (0, 10);
			}
			else if (value > 0.1f)
			{
				// 50% chance of orbit being between 0 and 5 degrees
				inclination = WarpRNG.GenerateInt (0, 5);
			}
			else
			{
				// 10% chance of a 0 inclination orbit
				inclination = 0;
			}
			orbitData.inclination = inclination;
			#endregion
			#region Eccentricity
			// Eccentricity must be a value between 0 and 0.99
			double eccentricity = WarpRNG.GetValue ();
			if (eccentricity == 1)
			{
				eccentricity = 0.99;
			}
			// For extreme values of eccentricity, tone it down a bit so planets don't buzz the sun so much
			if (eccentricity > 0.95)
			{
				eccentricity *= 0.5f;
			}
			else
			{
				// Below 0.25 eccentricity is ignored
				if (eccentricity <= 0.25)
				{
					// Values above 0.25 are toned down by 10% to keep orbits circlular
					eccentricity -= (eccentricity * 0.1f);
				}
				if (eccentricity <= 0.5)
				{
					// Values above 0.8 after being toned down are toned down by 25%
					eccentricity -= (eccentricity * 0.25f);
				}
				if (eccentricity <= 0.8)
				{
					// If values are *still* above 0.8, cut in half
					eccentricity *= 0.5f;
				}
				else
				{
					// Square resulting eccentricity to make orbits slightly more circular
					eccentricity *= eccentricity;
				}
				if (eccentricity < 0)
				{
					// Should never happen
					eccentricity = 0;
				}
			}
			orbitData.eccentricity = eccentricity;
			#endregion
			#region Sphere of Influence
			sphereOfInfluence = AstroUtils.CalculateSOIFromMass (planetData);
			if (sphereOfInfluence > AstroUtils.KERBIN_SOI * 30)
			{
				// This is where Jool's SOI caps out -- we don't want to go any larger
				sphereOfInfluence = AstroUtils.KERBIN_SOI * 30;
			}
			#endregion
			#region Semi-Major Axis
			double semiMajorAxis = AstroUtils.MAX_SEMI_MAJOR_AXIS;
			if (referenceBodyData.IsSun ())
			{
				// Special case: parent is sun
				// Find Semi-Major Axis in KAU (Kerbin Astronomical Units)
				// Min is 0.2 (~1.5 solar radii), max is 6.0 (Eeloo orbit)
				float kerbinSemiMajorAxisMultiplier = WarpRNG.GenerateFloat (0.02f, 6.0f);
				semiMajorAxis = kerbinSemiMajorAxisMultiplier * AstroUtils.KERBAL_ASTRONOMICAL_UNIT;
			}
			else
			{
				// Planet is moon
				value = WarpRNG.GetValue ();
				// Floor resulting value at 1%, to be used later
				if (value < 0.0001f)
				{
					value = 0.0001f;
				}
				// Semi-Major Axis can be anywhere within the hill sphere of parent body
				double hillSphere = AstroUtils.CalculateHillSphere (referenceBodyData);
				double tempMajorAxis = hillSphere * value;
				double parentAtmosphereHeight = planet.Radius + referenceBody.Radius + (referenceBody.atmosphereScaleHeight * 1000.0 * Mathf.Log (1000000.0f));
				while (tempMajorAxis < parentAtmosphereHeight)
				{
					// Inside planet's atmosphere
					value += WarpRNG.GenerateFloat (0.001f, 0.1f);
					tempMajorAxis = semiMajorAxis * value;
					foreach (int id in referenceBodyData.childDataIDs)
					{
						// This ensures we do not crash into other planets
						PlanetData childData = solarSystem.GetPlanetByID (id);
						double moonAxis = childData.semiMajorAxis;
						double moonMin = moonAxis - childData.planet.Radius;
						double moonMax = moonAxis + childData.planet.Radius;
						while (tempMajorAxis + planet.Radius >= moonMin && tempMajorAxis <= moonMax)
						{
							value += WarpRNG.GenerateFloat (0.001f, 0.1f);
							tempMajorAxis = semiMajorAxis * value;
						}
					}
				}
				semiMajorAxis = tempMajorAxis;
			}
			// Remove eccentricity from the semi-major axis
			if (orbitData.eccentricity != 1.0f)
			{
				semiMajorAxis /= (1.0 - orbitData.eccentricity);
			}
			orbitData.semiMajorAxis = semiMajorAxis;
			#endregion
			#region Longitude Ascending Node
			int lan = WarpRNG.GenerateInt (0, 360);
			orbitData.longitudeAscendingNode = lan;
			#endregion
			#region Argument Of Periapsis
			int argumentOfPeriapsis = WarpRNG.GenerateInt (0, 360);
			orbitData.argumentOfPeriapsis = argumentOfPeriapsis;
			#endregion
			#region Mean Anomaly at Epoch
			float meanAnomalyAtEpoch = WarpRNG.GenerateFloat (0.0f, Mathf.PI * 2.0f);
			if (orbitData.semiMajorAxis < 0)
			{
				meanAnomalyAtEpoch /= Mathf.PI;
				meanAnomalyAtEpoch -= 1.0f;
				meanAnomalyAtEpoch *= 5.0f;
			}
			orbitData.meanAnomalyAtEpoch = meanAnomalyAtEpoch;
			#endregion
			#region Period
			orbitData.period = AstroUtils.CalculatePeriodFromSemiMajorAxis (orbitData.semiMajorAxis);
			#endregion
		}

		public override void Apply ()
		{
			if (!IsSun ())
			{
				Debug.Log ("Sphere of influence: " + sphereOfInfluence + " meters (" + (sphereOfInfluence / AstroUtils.KERBIN_SOI) + " times Kerbin SOI)");
				planet.sphereOfInfluence = sphereOfInfluence;
			}
			Debug.Log ("Gravity: " + (gravity / AstroUtils.KERBIN_GRAVITY) + " times Kerbin gravity.");
			planet.gravParameter = gravity;
			if (orbitDriver != null)
			{
				orbit = CreateOrbit (orbitData, orbit);
				orbitDriver.orbit = orbit;
				orbitDriver.UpdateOrbit ();
			}
		}

		private static OrbitData OrbitDataFromOrbit (Orbit orbit)
		{
			OrbitData data = new OrbitData ();
			data.inclination = orbit.inclination;
			data.eccentricity = orbit.eccentricity;
			data.semiMajorAxis = orbit.semiMajorAxis;
			data.longitudeAscendingNode = orbit.LAN;
			data.argumentOfPeriapsis = orbit.argumentOfPeriapsis;
			data.meanAnomalyAtEpoch = orbit.meanAnomalyAtEpoch;
			data.epoch = orbit.epoch;
			data.period = orbit.period;
			data.referenceBody = orbit.referenceBody;
			return data;
		}

		private static Orbit CreateOrbit (OrbitData data, Orbit orbit)
		{
			return CreateOrbit (data.inclination,
			                    data.eccentricity, 
			                    data.semiMajorAxis, 
			                    data.longitudeAscendingNode, 
			                    data.argumentOfPeriapsis, 
			                    data.meanAnomalyAtEpoch, 
			                    data.epoch,
			                    data.period,
			                    orbit,
			                    data.referenceBody);
		}

		private static Orbit CreateOrbit (double inclination,
		                                  double eccentricity,
		                                  double semiMajorAxis, 
		                                  double longitudeAscendingNode, 
		                                  double argumentOfPeriapsis, 
		                                  double meanAnomalyAtEpoch, 
		                                  double epoch, 
		                                  double period,
		                                  Orbit orbit,
		                                  CelestialBody referenceBody)
		{
			if (double.IsNaN (inclination))
			{
				inclination = 0;
				Debug.LogWarning ("Inclination not a number!");
			}
			if (double.IsNaN (eccentricity))
			{
				eccentricity = 0;
				Debug.LogWarning ("Eccentricity not a number!");
			}
			if (double.IsNaN (semiMajorAxis))
			{
				semiMajorAxis = referenceBody.Radius + referenceBody.maxAtmosphereAltitude + 10000;
				Debug.LogWarning ("Semi-Major Axis not a number!");
			}
			if (double.IsNaN (longitudeAscendingNode))
			{
				longitudeAscendingNode = 0;
				Debug.LogWarning ("Longitude Ascending Node not a number!");
			}
			if (double.IsNaN (argumentOfPeriapsis))
			{
				argumentOfPeriapsis = 0;
				Debug.LogWarning ("Argument of Periapsis not a number!");
			}
			if (double.IsNaN (meanAnomalyAtEpoch))
			{
				meanAnomalyAtEpoch = 0;
				Debug.LogWarning ("Mean Anomaly at Epoch not a number!");
			}
			if (double.IsNaN (epoch))
			{
				epoch = Planetarium.GetUniversalTime ();
				Debug.LogWarning ("Epoch not a number!");
			}
			if (Mathf.Sign ((float)eccentricity - 1.0f) == Mathf.Sign ((float)semiMajorAxis))
			{
				semiMajorAxis = -semiMajorAxis;
			}
			if (Mathf.Sign ((float)semiMajorAxis) >= 0)
			{
				while (meanAnomalyAtEpoch < 0)
				{
					meanAnomalyAtEpoch += Mathf.PI * 2;
				}
				while (meanAnomalyAtEpoch > Mathf.PI * 2)
				{
					meanAnomalyAtEpoch -= Mathf.PI * 2;
				}
			}
			if (referenceBody == null)
			{
				Debug.LogError ("Reference body is null!");
				// Cannot proceed with setting orbit
				return orbit;
			}

			Debug.Log ("Reference Body: " + referenceBody);
			orbit.referenceBody = referenceBody;
			Debug.Log ("Inclination: " + inclination);
			orbit.inclination = inclination;
			Debug.Log ("Eccentricity: " + eccentricity);
			orbit.eccentricity = eccentricity;
			Debug.Log ("Semi-Major Axis: " + semiMajorAxis + " (" + (semiMajorAxis / AstroUtils.KERBIN_SOI) + " Kerbin Astronomical Units)");
			orbit.semiMajorAxis = semiMajorAxis;
			Debug.Log ("Longitude of Ascending Node: " + longitudeAscendingNode);
			orbit.LAN = longitudeAscendingNode;
			Debug.Log ("Argument of Periapsis: " + argumentOfPeriapsis);
			orbit.argumentOfPeriapsis = argumentOfPeriapsis;
			Debug.Log ("Mean Anomaly at Epoch: " + meanAnomalyAtEpoch);
			orbit.meanAnomalyAtEpoch = meanAnomalyAtEpoch;
			Debug.Log ("Epoch: " + epoch);
			orbit.epoch = epoch;
			Debug.Log ("Period: " + period + " seconds (" + (period / 9203545) + " years)");
			orbit.period = period;
			return orbit;
		}
	}
}

