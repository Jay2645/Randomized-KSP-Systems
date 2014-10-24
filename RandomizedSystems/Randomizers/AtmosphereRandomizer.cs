using UnityEngine;

namespace RandomizedSystems.Randomizers
{
	public class AtmosphereRandomizer : PlanetRandomizer
	{
		public AtmosphereRandomizer (CelestialBody body, PlanetData bodyData)
		{
			SetBody (body, bodyData);
		}

		/// <summary>
		/// Does the planet have an atmosphere?
		/// </summary>
		protected bool hasAtmosphere;
		/// <summary>
		/// Does the planet have oxygen (for jet engines)?
		/// </summary>
		protected bool hasOxygen;
		/// <summary>
		/// What is the atmosphere height? In KM, not m.
		/// </summary>
		protected double atmosphereHeight;
		/// <summary>
		/// Multiplier for atmospheric pressure.
		/// </summary>
		protected float atmospherePressureMult;
		/// <summary>
		/// A multiplier for the temperature. At higher settings, parts explode.
		/// </summary>
		protected float tempMultiplier;
		/// <summary>
		/// Colors the planet's atmosphere at low altitudes.
		/// </summary>
		protected Color ambientColor;

		public override void Cache ()
		{
			hasAtmosphere = planet.atmosphere;
			hasOxygen = planet.atmosphereContainsOxygen;
			atmosphereHeight = planet.atmosphereScaleHeight;
			atmospherePressureMult = planet.pressureMultiplier;
			tempMultiplier = planet.atmoshpereTemperatureMultiplier;
			ambientColor = planet.atmosphericAmbientColor;
		}

		public override void Randomize ()
		{
			float value = WarpRNG.GetValue ();
			// Atmosphere has a 75% chance of being generated if we are a planet
			// Atmosphere has a 10% chance of being generated if we are a moon
			if (value >= 0.25f && AstroUtils.IsSun (planetData.referenceBody) || value <= 0.1f)
			{
				hasAtmosphere = true;
			}
			if (hasAtmosphere)
			{
				value = WarpRNG.GetValue ();
				if (value >= 0.9f)
				{
					hasOxygen = true;
				}
				atmosphereHeight = WarpRNG.GenerateFloat (0.5f, 15.0f);
				atmospherePressureMult = WarpRNG.GenerateFloat (0.1f, 15.0f);
				ambientColor = new Color (WarpRNG.GetValue () * 0.25f, WarpRNG.GetValue () * 0.25f, WarpRNG.GetValue () * 0.25f);
			}
			// Temperature measured by distance from sun
			if (!IsSun ())
			{
				double orbitHeight = planetData.semiMajorAxis / AstroUtils.MAX_SEMI_MAJOR_AXIS;
				double inverseMult = 1.0 - orbitHeight;
				tempMultiplier = 5.0f * (float)inverseMult;
			}
		}

		public override void Apply ()
		{
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
		}
	}
}

