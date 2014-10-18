using UnityEngine;
using System.Collections.Generic;
using System;

namespace RandomizedSystems
{
	public class PlanetData
	{
		public SolarData solarSystem;
		public CelestialBody planet;
		public string name;
		public Orbit orbit;
		private OrbitDriver orbitDriver;
		public bool hasAtmosphere = true;
		public bool hasOxygen = true;
		public double gravity = 0;
		public float tempMultiplier = 1.0f;
		public double atmosphereHeight = 5;
		public float atmospherePressureMult = 1.0f;
		public Color ambientColor = Color.gray;
		public int planetID = -1;
		public List<CelestialBody> childBodies = new List<CelestialBody> ();
		private const double KERBIN_GRAVITY = 3531600000000.0;

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
			tempMultiplier = planet.atmoshpereTemperatureMultiplier;

			// Orbit
			if (planet.referenceBody.name != planet.name)
			{
				orbit = planet.GetOrbit ();
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
			float value = Randomizer.GetValue ();
			if (orbit != null)
			{
				#region Reference Body
				PlanetData referenceData = null;
				CelestialBody referenceBody = planet.referenceBody;
				if (value >= 0.5f || solarSystem.planetCount <= 1 || childBodies.Count > 0)
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
					while ((referenceBody == planet || referenceBody.referenceBody != solarSystem.sun || referenceBody.Radius < planet.Radius))
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
				#endregion
				#region Inclination
				int inclination = 0;
				if (value >= 0.9f)
				{
					inclination = Randomizer.GenerateInt (0, 360);
				}
				else if (value >= 0.8f)
				{
					inclination = Randomizer.GenerateInt (0, 180);
				}
				else if (value >= 0.75f)
				{
					inclination = Randomizer.GenerateInt (0, 90);
				}
				else if (value >= 0.6f)
				{
					inclination = Randomizer.GenerateInt (0, 45);
				}
				else if (value >= 0.25f)
				{
					inclination = Randomizer.GenerateInt (0, 10);
				}
				#endregion
				#region Eccentricity
				double eccentricity = Randomizer.GetValue ();
				if (eccentricity == 1)
				{
					eccentricity = 0.99;
				}
				if (eccentricity <= 0.25)
				{
					eccentricity *= 0.1f;
				}
				else if (eccentricity <= 0.5)
				{
					eccentricity *= 0.25f;
				}
				else if (eccentricity <= 0.8)
				{
					eccentricity *= 0.5f;
				}
				else
				{
					eccentricity *= eccentricity;
				}
				#endregion
				#region Altitude
				double periapsis = 1;
				value = Randomizer.GetValue ();
				if (value < 0.01f)
				{
					value = 0.01f;
				}
				float secondValue = Randomizer.GetValue ();
				if (secondValue < 0.01f)
				{
					secondValue = 0.01f;
				}
				periapsis = 250000000000 * value * secondValue;
				if (referenceBody != solarSystem.sun)
				{
					// Not orbiting sun
					periapsis *= 0.0001;
					while (periapsis < referenceBody.Radius + referenceBody.atmosphereScaleHeight * 1000.0 * Mathf.Log(1000000.0f))
					{
						// Inside planet's atmosphere
						// This check might need to be moved to after we have already adjusted planets' atmosphere
						periapsis *= 10.0f;
					}
				}
				double semiMajorAxis = periapsis;
				if (eccentricity != 1.0f)
				{
					semiMajorAxis /= (1.0 - eccentricity);
				}
				#endregion
				#region Longitude Ascending Node
				int lan = Randomizer.GenerateInt (0, 360);
				#endregion
				#region Argument Of Periapsis
				int argumentOfPeriapsis = Randomizer.GenerateInt (0, 360);
				#endregion
				#region Mean Anomaly at Epoch
				float meanAnomalyAtEpoch = Randomizer.GenerateFloat (0.0f, Mathf.PI * 2.0f);
				if (semiMajorAxis < 0)
				{
					meanAnomalyAtEpoch /= Mathf.PI;
					meanAnomalyAtEpoch -= 1.0f;
					meanAnomalyAtEpoch *= 5.0f;
				}
				#endregion
				orbit = CreateOrbit (inclination, eccentricity, semiMajorAxis, lan, argumentOfPeriapsis, meanAnomalyAtEpoch, Planetarium.GetUniversalTime (), orbit, referenceBody);
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

			// Gravity expressed in terms of how many times Kerbin's gravity a planet is
			gravity = KERBIN_GRAVITY * Randomizer.GenerateFloat (0.0f, 10.0f);

			// Temperature measured by distance from sun
			if (orbit != null)
			{
				float orbitHeight = (float)orbit.altitude / 250000000000.0f;
				float inverseMult = 1.0f - orbitHeight;
				tempMultiplier = 5.0f * inverseMult;
			}
		}

		public void ApplyChanges ()
		{
			planet.bodyName = name;
			if (orbitDriver != null)
			{
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
		}

		private static Orbit CreateOrbit (double inclination,
		                                  double eccentricity,
		                                  double semiMajorAxis, 
		                                  double longitudeAscendingNode, 
		                                  double argumentOfPeriapsis, 
		                                  double meanAnomalyAtEpoch, 
		                                  double epoch, 
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
			return orbit;
		}
	}
}

