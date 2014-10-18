using UnityEngine;
using System.Collections.Generic;
using System;

namespace RandomizedSystems
{
	public class PlanetData
	{
		public SolarData solarSystem;
		public CelestialBody referenceBody;
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
			if (planet.referenceBody.name != planet.name)
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
			if (planet.referenceBody.name != planet.name)
			{
				// Gravity expressed in terms of how many times Kerbin's gravity a planet is
				float gravityMult = Randomizer.GenerateFloat (0.0f, 5.0f);
				gravity = KERBIN_GRAVITY * gravityMult;
				sphereOfInfluence = planet.Radius * 1.5;
				if (orbitData.referenceBody.name != solarSystem.sun.name)
				{
					// Moon
					sphereOfInfluence += (MUN_SOI * gravityMult);
				}
				else
				{
					// Planet
					sphereOfInfluence += (KERBIN_SOI * gravityMult);
				}
			}
			float value = Randomizer.GetValue ();
			if (orbit != null)
			{
				orbitData = new OrbitData ();
				#region Reference Body
				PlanetData referenceData = null;
				referenceBody = planet.referenceBody;
				if (value > 0.75f || solarSystem.planetCount <= 1 || childBodies.Count > 0 || planet.isHomeWorld)
				{
					referenceBody = solarSystem.sun;
					referenceData = solarSystem.sunData;
				}
				else
				{
					referenceBody = planet;
					List<int> attemptedInts = new List<int> ();
					int attempts = 0;
					// Toss out a candidate if any of the following is true:
					// 1. The reference body is us (causes KSP to crash)
					// 2. The reference body is a moon
					// 3. The reference body is smaller than us
					// Move us to the sun after 100 attempts.
					while ((referenceBody == planet || referenceData.referenceBody != solarSystem.sun || referenceBody.Radius < planet.Radius))
					{
						attempts++;
						int index = Randomizer.GenerateInt (0, solarSystem.planetCount);
						if (attemptedInts.Contains (index))
						{
							continue;
						}
						attemptedInts.Add (index);
						referenceData = solarSystem.GetPlanetByID (index);
						referenceBody = referenceData.planet;
						if (attempts >= 100)
						{
							referenceBody = solarSystem.sun;
							referenceData = solarSystem.sunData;
							break;
						}
					}
				}
				solarSystem.AddChildToPlanet (referenceData.planetID, planet);
				orbitData.referenceBody = referenceBody;
				#endregion
				#region Inclination
				int inclination = 0;
				if (value >= 0.95f)
				{
					inclination = Randomizer.GenerateInt (0, 180);
				}
				else if (value >= 0.925f)
				{
					inclination = Randomizer.GenerateInt (0, 60);
				}
				else if (value >= 0.875f)
				{
					inclination = Randomizer.GenerateInt (0, 35);
				}
				else if (value >= 0.825f)
				{
					inclination = Randomizer.GenerateInt (0, 25);
				}
				else if (value >= 0.5f)
				{
					inclination = Randomizer.GenerateInt (0, 10);
				}
				else
				{
					inclination = Randomizer.GenerateInt (0, 5);
				}
				orbitData.inclination = inclination;
				#endregion
				#region Eccentricity
				double eccentricity = Randomizer.GetValue ();
				if (eccentricity == 1)
				{
					eccentricity = 0.99;
				}
				if (eccentricity > 0.95)
				{
					eccentricity *= 0.5f;
				}
				else
				{
					if (eccentricity <= 0.25)
					{
						eccentricity *= 0.1f;
					}
					if (eccentricity <= 0.5)
					{
						eccentricity *= 0.25f;
					}
					if (eccentricity <= 0.8)
					{
						eccentricity *= 0.5f;
					}
					else
					{
						eccentricity *= eccentricity;
					}
				}
				orbitData.eccentricity = eccentricity;
				#endregion
				#region Altitude
				double semiMajorAxis = referenceData.sphereOfInfluence;
				value = Randomizer.GetValue ();
				if (value < 0.01f)
				{
					value = 0.01f;
				}
				if (referenceBody.name == solarSystem.sun.name)
				{
					semiMajorAxis = MAX_SEMI_MAJOR_AXIS;
					float secondValue = Randomizer.GetValue ();
					if (secondValue < 0.01f)
					{
						secondValue = 0.01f;
					}
					semiMajorAxis *= value * secondValue;
				}
				else
				{
					semiMajorAxis *= value;
					while (semiMajorAxis < referenceBody.Radius + referenceBody.atmosphereScaleHeight * 1000.0 * Mathf.Log(1000000.0f))
					{
						// Inside planet's atmosphere
						// This check might need to be moved to after we have already adjusted planets' atmosphere
						semiMajorAxis *= 2.0f;
					}
				}
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
			else
			{
				referenceBody = solarSystem.sun;
				orbitData = new OrbitData ();
				orbitData.semiMajorAxis = 0;
			}

			// Randomize atmosphere
			value = Randomizer.GetValue ();
			if (value >= 0.4f)
			{
				hasAtmosphere = true;
			}
			if (hasAtmosphere)
			{
				value = Randomizer.GetValue ();
				if (value >= 0.75f)
				{
					hasOxygen = true;
				}
				atmosphereHeight = Randomizer.GenerateInt (1, 10);
				atmospherePressureMult = Randomizer.GenerateFloat (0.1f, 15.0f);
				ambientColor = new Color (Randomizer.GetValue () * 0.5f, Randomizer.GetValue () * 0.5f, Randomizer.GetValue () * 0.5f);
			}

			value = Randomizer.GenerateFloat (0.0f, 30.0f);
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

		public void ApplyChanges ()
		{
			planet.bodyName = name;
			if (orbitDriver != null)
			{
				orbit = CreateOrbit (orbitData, orbit);
				orbitDriver.orbit = orbit;
				orbitDriver.UpdateOrbit ();
			}
			planet.atmosphere = hasAtmosphere;
			planet.atmosphereContainsOxygen = hasOxygen;
			planet.gravParameter = gravity;
			planet.atmoshpereTemperatureMultiplier = tempMultiplier;
			planet.atmosphereScaleHeight = atmosphereHeight;
			planet.pressureMultiplier = atmospherePressureMult;
			planet.atmosphericAmbientColor = ambientColor;
			planet.orbitingBodies = childBodies;
			planet.rotationPeriod = rotationPeriod;
			if (planet.referenceBody.name != planet.name)
			{
				planet.sphereOfInfluence = sphereOfInfluence;
			}
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

		private OrbitData OrbitDataFromOrbit (Orbit orbit)
		{
			OrbitData data = new OrbitData ();
			data.argumentOfPeriapsis = orbit.argumentOfPeriapsis;
			data.eccentricity = orbit.eccentricity;
			data.epoch = orbit.epoch;
			data.inclination = orbit.inclination;
			data.longitudeAscendingNode = orbit.LAN;
			data.meanAnomalyAtEpoch = orbit.meanAnomalyAtEpoch;
			data.referenceBody = orbit.referenceBody;
			data.semiMajorAxis = orbit.semiMajorAxis;
			return data;
		}

		private static double CalculateOrbitalPeriodFromSemimajorAxis (double semimajorAxis)
		{
			double kerbalAU = semimajorAxis / KERBAL_ASTRONOMICAL_UNIT;
			// This formula produces a rough equivalent of the relationship between orbital periods and years on Earth
			// Errors get higher as AU increases
			double period = -0.114435 + (0.77734 * kerbalAU) + (0.337095 * (kerbalAU * kerbalAU));
			// Time is in years, so a conversion to seconds is required
			// Given time is 1 Kerbin year
			period *= 9203545;
			return period;
		}
	}
}

