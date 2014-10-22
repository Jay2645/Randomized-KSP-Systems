using UnityEngine;
using System.Collections.Generic;
using System;

namespace RandomizedSystems
{
	public class PlanetData
	{
		public SolarData solarSystem;
		public CelestialBody referenceBody;
		public PlanetData referenceBodyData;
		public CelestialBody planet;
		public string name;
		public Orbit orbit;
		private OrbitDriver orbitDriver;
		public OrbitData orbitData;
		public bool hasAtmosphere = true;
		public bool hasOxygen = true;
		public double gravity = 0;
		public double rotationPeriod;
		public double sphereOfInfluence;
		public float tempMultiplier = 1.0f;
		public double atmosphereHeight = 5;
		public float atmospherePressureMult = 1.0f;
		public Color ambientColor = Color.gray;
		public int planetID = -1;
		public List<CelestialBody> childBodies = new List<CelestialBody> ();
		public List<int> childDataIDs = new List<int> ();
		private const double KERBIN_GRAVITY = 3531600000000.0;
		private const double KERBAL_ASTRONOMICAL_UNIT = 13599840256;
		private const double KERBIN_SOI = 84159286.0;
		private const double MUN_SOI = 2429559.1;
		private const double KERBIN_RADIUS = 600000;
		private const double MAX_SEMI_MAJOR_AXIS = 90118820000;

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
		}

		public PlanetData (CelestialBody planet, SolarData system, int id)
		{
			this.planetID = id;
			this.solarSystem = system;
			this.planet = planet;
			GetValues ();
		}

		private void GetValues ()
		{
			// General
			name = planet.name;
			gravity = planet.gravParameter;
			rotationPeriod = planet.rotationPeriod;
			tempMultiplier = planet.atmoshpereTemperatureMultiplier;
			sphereOfInfluence = planet.sphereOfInfluence;

			// Orbit
			if (!IsSun ())
			{
				orbit = planet.GetOrbit ();
				orbitData = OrbitDataFromOrbit (orbit);
				orbitDriver = planet.orbitDriver;
			}

			// Atmosphere
			hasAtmosphere = planet.atmosphere;
			hasOxygen = planet.atmosphereContainsOxygen;
			atmosphereHeight = planet.atmosphereScaleHeight;
			ambientColor = planet.atmosphericAmbientColor;
			atmospherePressureMult = planet.pressureMultiplier;
		}

		public void RandomizeValues ()
		{
			name = Randomizer.GenerateName ();
			CreateOrbit ();
			CreateAtmosphere ();
			CreateCharacteristics ();
		}

		public void ApplyChanges ()
		{
			Debug.LogWarning ("Planet: " + name);
			planet.bodyName = name;
			if (IsSun ())
			{
				Debug.LogWarning ("Star");
			}
			else
			{
				Debug.Log ("Gravity: " + (gravity / KERBIN_GRAVITY) + " times Kerbin gravity.");
				planet.gravParameter = gravity;
			}
			Debug.Log ("Atmosphere: " + hasAtmosphere);
			if (hasAtmosphere)
			{
				Debug.Log ("Oxygen: " + hasOxygen);
				Debug.Log ("Atmosphere height: " + (atmosphereHeight * Mathf.Log (1000000.0f)) + " kilometers.");
				Debug.Log ("Pressure multiplier: " + atmospherePressureMult);
				Debug.Log ("Temperature multiplier: " + tempMultiplier);
				Debug.Log ("Ambient color: " + ambientColor);
			}
			planet.atmosphere = hasAtmosphere;
			planet.atmosphereContainsOxygen = hasOxygen;
			planet.atmoshpereTemperatureMultiplier = tempMultiplier;
			planet.atmosphereScaleHeight = atmosphereHeight;
			planet.pressureMultiplier = atmospherePressureMult;
			planet.atmosphericAmbientColor = ambientColor;
			if (!IsSun ())
			{
				Debug.Log ("Sphere of influence: " + sphereOfInfluence + " meters (" + (sphereOfInfluence / KERBIN_SOI) + " times Kerbin SOI)");
				planet.sphereOfInfluence = sphereOfInfluence;
			}
			Debug.Log ("Rotation period: " + rotationPeriod + " seconds per rotation (" +
				(rotationPeriod / 60) + " minutes, " + ((rotationPeriod / 60) / 60) + " hours)");
			planet.rotationPeriod = rotationPeriod;
			planet.orbitingBodies = childBodies;
			if (orbitDriver != null)
			{
				orbit = CreateOrbit (orbitData, orbit);
				orbitDriver.orbit = orbit;
				orbitDriver.UpdateOrbit ();
			}
		}

		private void CreateOrbit ()
		{
			float value = Randomizer.GetValue ();
			orbitData = new OrbitData ();
			#region Reference Body
			if (orbit == null)
			{
				// Special case: Sun
				referenceBody = solarSystem.sun;
				orbitData.semiMajorAxis = 0;
				return;
			}
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
					int index = Randomizer.GenerateInt (0, solarSystem.planetCount);
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
			solarSystem.AddChildToPlanet (referenceBodyData.planetID, planet);
			// Update orbital data
			orbitData.referenceBody = referenceBody;
			#endregion
			#region Inclination
			// Inclination starts directly at orbital plane
			int inclination = 0;
			// Get new random value
			value = Randomizer.GetValue ();
			if (value >= 0.975f)
			{
				// 2.5% chance of orbit being between 0 and 180 degrees
				inclination = Randomizer.GenerateInt (0, 180);
			}
			else if (value >= 0.95f)
			{
				// 2.5% chance of orbit being between 0 and 60 degrees
				inclination = Randomizer.GenerateInt (0, 60);
			}
			else if (value >= 0.925f)
			{
				// 2.5% chance of orbit being between 0 and 45 degrees
				inclination = Randomizer.GenerateInt (0, 45);
			}
			else if (value >= 0.9f)
			{
				// 2.5% chance or orbit being between 0 and 25 degrees
				inclination = Randomizer.GenerateInt (0, 25);
			}
			else if (value >= 0.6f)
			{
				// 30% chance of orbit being between 0 and 10 degrees
				inclination = Randomizer.GenerateInt (0, 10);
			}
			else if (value > 0.1f)
			{
				// 50% chance of orbit being between 0 and 5 degrees
				inclination = Randomizer.GenerateInt (0, 5);
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
			double eccentricity = Randomizer.GetValue ();
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
			#region Gravity
			float gravityMult = 0.0f;
			if (IsMoon ())
			{
				// Moons in KSP for the most part have SOIs which are greater than their real-life counterparts
				// SOI -> Gravity is not a 1:1 ratio; instead a moon's SOI is usually 7-8 times more powerful than its gravity
				// To skew the gravity data for moons, we use the formula y = (0.0788628 * x^2)-(0.788279 * x)+1.58089
				// Note that values below 7.25 generate negative multipliers
				float randomGravity = Randomizer.GenerateFloat (7.25f, 9f);
				gravityMult = (0.0788628f * randomGravity * randomGravity) - (0.788279f * randomGravity) + 1.58089f;
			}
			else
			{
				gravityMult = Randomizer.GenerateFloat (0.15f, 2.0f);
				value = Randomizer.GetValue ();
				// There is a chance that a planet is a gas giant like Jool
				if (value <= 0.05f)
				{
					gravityMult *= 20.0f;
				}
			}
			gravity = gravityMult * KERBIN_GRAVITY;
			sphereOfInfluence = CalculateSOIFromGravity (gravityMult);
			#endregion
			#region Altitude
			value = Randomizer.GetValue ();
			// Floor resulting value at 1%, to be used later
			if (value < 0.0001f)
			{
				value = 0.0001f;
			}
			// Max Semi-Major Axis is based on sphere of influence of parent body
			double semiMajorAxis = referenceBodyData.sphereOfInfluence;
			if (referenceBodyData.IsSun ())
			{
				// Special case: parent is sun
				semiMajorAxis = MAX_SEMI_MAJOR_AXIS;
				// Determine second random value
				float secondValue = Randomizer.GetValue ();
				// Orbit can be anywhere between the max semi-major axis of the sun and 0.01% of the semi-major axis
				semiMajorAxis *= value * secondValue;
			}
			else
			{
				// Planet is moon
				// Semi-Major Axis can be anywhere within the sphere of influence of parent body
				double tempMajorAxis = semiMajorAxis * value;
				double parentAtmosphereHeight = planet.Radius + referenceBody.Radius + (referenceBody.atmosphereScaleHeight * 1000.0 * Mathf.Log (1000000.0f));
				while (tempMajorAxis < parentAtmosphereHeight)
				{
					// Inside planet's atmosphere
					value += Randomizer.GenerateFloat (0.001f, 0.1f);
					tempMajorAxis = semiMajorAxis * value;
					foreach (int id in referenceBodyData.childDataIDs)
					{
						// This ensures we do not crash into other planets
						PlanetData childData = solarSystem.GetPlanetByID (id);
						double moonAxis = childData.orbitData.semiMajorAxis;
						double moonMin = moonAxis - childData.planet.Radius;
						double moonMax = moonAxis + childData.planet.Radius;
						while (tempMajorAxis + planet.Radius >= moonMin && tempMajorAxis <= moonMax)
						{
							value += Randomizer.GenerateFloat (0.001f, 0.1f);
							tempMajorAxis = semiMajorAxis * value;
						}
					}
				}
				semiMajorAxis = tempMajorAxis;
			}
			// Remove eccentricity from the semi-major axis
			if (eccentricity != 1.0f)
			{
				semiMajorAxis /= (1.0 - eccentricity);
			}
			orbitData.semiMajorAxis = semiMajorAxis;
			#endregion
			#region Longitude Ascending Node
			int lan = Randomizer.GenerateInt (0, 360);
			orbitData.longitudeAscendingNode = lan;
			#endregion
			#region Argument Of Periapsis
			int argumentOfPeriapsis = Randomizer.GenerateInt (0, 360);
			orbitData.argumentOfPeriapsis = argumentOfPeriapsis;
			#endregion
			#region Mean Anomaly at Epoch
			float meanAnomalyAtEpoch = Randomizer.GenerateFloat (0.0f, Mathf.PI * 2.0f);
			if (semiMajorAxis < 0)
			{
				meanAnomalyAtEpoch /= Mathf.PI;
				meanAnomalyAtEpoch -= 1.0f;
				meanAnomalyAtEpoch *= 5.0f;
			}
			orbitData.meanAnomalyAtEpoch = meanAnomalyAtEpoch;
			#endregion
			#region Period
			orbitData.period = CalculateOrbitalPeriodFromSemimajorAxis (semiMajorAxis);
			#endregion
		}

		private void CreateAtmosphere ()
		{
			// Randomize atmosphere
			float value = Randomizer.GetValue ();
			// Atmosphere has a 75% chance of being generated if we are a planet
			// Atmosphere has a 10% chance of being generated if we are a moon
			if (value >= 0.25f && IsSun (referenceBody) || value <= 0.1f)
			{
				hasAtmosphere = true;
			}
			if (hasAtmosphere)
			{
				value = Randomizer.GetValue ();
				if (value >= 0.9f)
				{
					hasOxygen = true;
				}
				atmosphereHeight = Randomizer.GenerateInt (1, 10);
				atmospherePressureMult = Randomizer.GenerateFloat (0.1f, 15.0f);
				ambientColor = new Color (Randomizer.GetValue () * 0.25f, Randomizer.GetValue () * 0.25f, Randomizer.GetValue () * 0.25f);
			}
		}

		private void CreateCharacteristics ()
		{
			float value = Randomizer.GenerateFloat (0.0f, 30.0f);
			rotationPeriod = value * 3600;
			if (Randomizer.GetValue () < 0.10f)
			{
				rotationPeriod *= 30;
			}
			// Temperature measured by distance from sun
			if (orbit != null)
			{
				double orbitHeight = orbitData.semiMajorAxis / MAX_SEMI_MAJOR_AXIS;
				double inverseMult = 1.0 - orbitHeight;
				tempMultiplier = 5.0f * (float)inverseMult;
			}
		}

		public bool IsSun ()
		{
			// The sun orbits itself
			return planet.referenceBody.name == planet.name;
		}

		public bool IsMoon ()
		{
			// If our reference body is *not* the sun, we are a moon
			return referenceBody.name != solarSystem.sun.name;
		}

		private static bool IsSun (CelestialBody potentialSun)
		{
			return potentialSun.referenceBody.name == potentialSun.name;
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
			if (Math.Sign (eccentricity - 1) == Math.Sign (semiMajorAxis))
			{
				semiMajorAxis = -semiMajorAxis;
			}
			if (Math.Sign (semiMajorAxis) >= 0)
			{
				while (meanAnomalyAtEpoch < 0)
				{
					meanAnomalyAtEpoch += Math.PI * 2;
				}
				while (meanAnomalyAtEpoch > Math.PI * 2)
				{
					meanAnomalyAtEpoch -= Math.PI * 2;
				}
			}
			orbit.referenceBody = referenceBody;
			orbit.inclination = inclination;
			orbit.eccentricity = eccentricity;
			orbit.semiMajorAxis = semiMajorAxis;
			orbit.LAN = longitudeAscendingNode;
			orbit.argumentOfPeriapsis = argumentOfPeriapsis;
			orbit.meanAnomalyAtEpoch = meanAnomalyAtEpoch;
			orbit.epoch = epoch;
			orbit.period = period;
			return orbit;
		}

		private double CalculateSOIFromGravity (double gravityMult)
		{
			if (IsSun ())
			{
				return MAX_SEMI_MAJOR_AXIS;
			}
			double sphereOfInfluence = 0;
			if (IsMoon ())
			{
				// All moons have their gravity adjusted according to the following quadratic:
				// y = (4 * sqrt(7925156250 * x + 3082419716)+499779)/100000
				double moonModifier = (double)((4.0f * Mathf.Sqrt ((7925156250.0f * (float)gravityMult) + 3082419716.0f) + 499779.0f) / 100000.0f);
				// This "unskews" the gravity multiplier and allows moons to use the same gravity formula as planets
				gravityMult *= moonModifier;
			}
			// Kerbal Space Program doesn't use a straight 1:1 ratio for Gravity -> SOI
			// Instead, it uses something along the following quadratic: y = 7.42334 - 9.01415 x + 2.59081 x^2
			// Note that the range from 1.25 to 2.15 generates negative values
			if (gravityMult > 1.25 && gravityMult < 2.15)
			{
				gravityMult += 1.0f;
			}
			double difference = (2.59081 * gravityMult * gravityMult) - (9.01415 * gravityMult) + 7.42334;
			sphereOfInfluence = gravityMult * difference;
			// No stock moon has a larger SOI than 15% of Kerbin's
			// If we generate absurdly large or invalid values, we throw them out
			// This is the "old" formula and may not be perfect
			if (double.IsNaN (sphereOfInfluence) || IsMoon () && sphereOfInfluence > 0.15)
			{
				Debug.LogWarning ("Tossing SOI for " + planet.name + ": " + sphereOfInfluence + ". Gravity: " + gravityMult);
				sphereOfInfluence = planet.Radius * 1.5;
				if (IsMoon ())
				{
					// Sphere of Influence is modified by the Mun's SOI and the gravity of our body
					sphereOfInfluence += (MUN_SOI * Randomizer.GenerateFloat (0.0f, 1.5f));
					if (sphereOfInfluence * 2 > referenceBodyData.sphereOfInfluence)
					{
						// Our parent body must have at least double our influence
						float sphereMult = Randomizer.GenerateFloat (0.1f, 0.5f);
						// There is still a minimum of our radius * 1.5, however
						sphereOfInfluence = referenceBodyData.sphereOfInfluence * sphereMult;
						if (sphereOfInfluence < planet.Radius * 1.5)
						{
							sphereOfInfluence = planet.Radius * 1.5;
							// Parent body must now have at minimum double that value as its SOI
							double parentSOI = sphereOfInfluence * Randomizer.GenerateFloat (2.0f, 3.0f);
							solarSystem.AdjustPlanetSOI (referenceBodyData.planetID, parentSOI);
							// Gravity must also reflect this
							// Remove the parent's radius from the SOI calculations
							parentSOI -= referenceBodyData.planet.Radius * 1.5;
							// New gravity multiplier is based on how much stronger this is than Kerbin's SOI
							double parentGravityMult = parentSOI / KERBIN_SOI;
							solarSystem.AdjustPlanetGravity (referenceBodyData.planetID, KERBIN_GRAVITY * parentGravityMult);
						}
					}
				}
				else
				{
					// Planet
					// Sphere of Influence is modified by Kerbin's SOI and the gravity of our body
					sphereOfInfluence += (KERBIN_SOI * gravityMult);
				}
			}
			else
			{
				if (sphereOfInfluence > 80)
				{
					// Jool would have an absurdly large SOI in this formula, but it's capped at 80
					sphereOfInfluence = 80;
				}
				// Sphere of Influence is presented as a multiplier of Kerbin's SOI
				sphereOfInfluence *= KERBIN_SOI;
			}
			return sphereOfInfluence;
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

		private static double CalculateOrbitalPeriodFromSemimajorAxis (double semimajorAxis)
		{
			double kerbalAU = semimajorAxis / KERBAL_ASTRONOMICAL_UNIT;
			// This formula produces a rough equivalent of the relationship between orbital periods and years on Earth
			// Errors get higher as AU increases
			double period = -0.114435 + (0.77734 * kerbalAU) + (0.337095 * (kerbalAU * kerbalAU));
			if (period < 0)
			{
				period *= -1;
			}
			// Time is in years, so a conversion to seconds is required
			// Given time is 1 Kerbin year
			period *= 9203545;
			return period;
		}
	}
}

