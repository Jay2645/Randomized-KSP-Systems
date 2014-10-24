using UnityEngine;
using System;
using RandomizedSystems.Randomizers;

namespace RandomizedSystems
{
	/// <summary>
	/// Calculates astronomical formulas and the like.
	/// </summary>
	public static class AstroUtils
	{
		public const double KERBIN_SOI = 84159286.0;
		public const double KERBIN_MASS = 5.2915793e22;
		public const double MAX_SEMI_MAJOR_AXIS = 90118820000;
		public const double KERBIN_GRAVITY = 3531600000000.0;
		public const double KERBAL_ASTRONOMICAL_UNIT = 13599840256;
		public const double KERBIN_RADIUS = 600000;
		public const double GRAV_CONSTANT = 6.673e-11;
		public const double MUN_SOI = 2429559.1;

		public static double piSquared
		{
			get
			{
				return Math.PI * Math.PI;
			}
		}

		/// <summary>
		/// Calculates gravity, in terms of a multiplier of Kerbin gravity.
		/// </summary>
		/// <returns>The gravity, in terms of multiples of Kerbin gravity.</returns>
		/// <param name="mass">Mass, in relation to Kerbin.</param>
		/// <param name="radius">Radius, in relation to Kerbin.</param>
		public static double CalculateGravity (double mass, double radius)
		{
			double radiusSquared = radius * radius;
			return mass / radiusSquared;
		}

		/// <summary>
		/// Calcluates orbital period using Kepler's Third Law.
		/// T^2 = (4 pi^2 a^3)/G
		/// T: Orbital period
		/// m: Solar mass
		/// a: semi-major axis
		/// G: Newtonian gravitational constant
		/// </summary>
		/// <returns>The orbital period from the semi-major axis.</returns>
		public static double CalculatePeriodFromSemiMajorAxis (double semiMajorAxis)
		{
			double periodSquared = (4 * piSquared * (Math.Pow ((semiMajorAxis / KERBAL_ASTRONOMICAL_UNIT * 2.0f), 3))) / GRAV_CONSTANT;
			double output = Math.Abs (Math.Sqrt (periodSquared));
			return output;
		}

		/// <summary>
		/// Calculates the hill sphere, which is the maxmium radius that a child body can orbit.
		/// </summary>
		/// <returns>The hill sphere.</returns>
		/// <param name="body">The parent body to calculate the hill sphere for.</param>
		public static double CalculateHillSphere (PlanetData body)
		{
			if (body.IsSun ())
			{
				return MAX_SEMI_MAJOR_AXIS;
			}
			// Things are expressed in relation to Kerbin because the "real" way produced semi-major axes which felt too large.
			// Since KSP is scaled down, this formula may need to be scaled down as well
			/*double eccentricity = 1.0 - body.eccentricity;
			double normalizedSemiMajorAxis = body.semiMajorAxis * eccentricity;*/
			/*double normalizedSemiMajorAxis = body.semiMajorAxis / KERBAL_ASTRONOMICAL_UNIT;
			double massRatio = (body.planet.Mass / KERBIN_MASS) / (3 * (body.referenceBody.Mass / KERBIN_MASS));
			return normalizedSemiMajorAxis * Math.Pow (massRatio, (1 / 3));*/
			return body.sphereOfInfluence;
		}

		/// <summary>
		/// Calculates the volume of a perfect sphere, given a radius.
		/// </summary>
		/// <returns>The volume of a sphere with the given radius.</returns>
		/// <param name="radius">The radius.</param>
		public static double CalculateVolume (double radius)
		{
			return (radius * radius * radius) * ((4 / 3) * Math.PI);
		}

		/// <summary>
		/// Calculates the mass for a given radius and density.
		/// </summary>
		/// <returns>The mass.</returns>
		/// <param name="density">Density.</param>
		/// <param name="radius">Radius.</param>
		public static double CalculateMass (double density, double radius)
		{
			return CalculateVolume (radius) * density;
		}

		/// <summary>
		/// Calculate density for a given mass and radius.
		/// </summary>
		/// <returns>The density of a sphere.</returns>
		/// <param name="mass">The mass of the sphere.</param>
		/// <param name="radius">The radius of the sphere.</param>
		public static double CalculateDensity (double mass, double radius)
		{
			return mass / CalculateVolume (radius);
		}

		/// <summary>
		/// Calculates the radius for a given mass and density.
		/// </summary>
		/// <returns>The radius of a sphere with the given mass and density.</returns>
		/// <param name="mass">The mass.</param>
		/// <param name="density">The density.</param>
		public static double CalculateRadius (double mass, double density)
		{
			double volume = mass / density;
			double radiusCubed = (3 * volume) / (4 * Math.PI);
			return Math.Pow (radiusCubed, (1 / 3));
		}

		public static double CalculateSOIFromMass (PlanetData body)
		{
			if (body.IsSun ())
			{
				return MAX_SEMI_MAJOR_AXIS;
			}
			/*double semiMajorAxis = body.semiMajorAxis / KERBAL_ASTRONOMICAL_UNIT;
			double mass = body.planet.Mass / KERBIN_MASS;
			double parentMass = body.referenceBody.Mass / KERBIN_MASS;
			double massRatio = mass / parentMass;
			double twoFifths = 2.0 / 5.0;
			return semiMajorAxis * Math.Pow (massRatio, twoFifths) * KERBAL_ASTRONOMICAL_UNIT;*/
			double gravityMult = body.gravityMultiplier;
			if (body.IsMoon ())
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
			double sphereOfInfluence = gravityMult * difference;
			// No stock moon has a larger SOI than 15% of Kerbin's
			// If we generate absurdly large or invalid values, we throw them out
			// This is the "old" formula and may not be perfect
			if (double.IsNaN (sphereOfInfluence) || body.IsMoon () && sphereOfInfluence > 0.15)
			{
				Debugger.LogWarning ("Tossing SOI for " + body.name + ": " + sphereOfInfluence + ". Gravity: " + gravityMult);
				sphereOfInfluence = body.planet.Radius * 1.5;
				if (body.IsMoon ())
				{
					// Sphere of Influence is modified by the Mun's SOI and the gravity of our body
					sphereOfInfluence += (MUN_SOI * WarpRNG.GenerateFloat (0.0f, 1.5f));
					if (sphereOfInfluence * 2 > body.referenceBodyData.sphereOfInfluence)
					{
						// Our parent body must have at least double our influence
						float sphereMult = WarpRNG.GenerateFloat (0.1f, 0.5f);
						// There is still a minimum of our radius * 1.5, however
						sphereOfInfluence = body.referenceBodyData.sphereOfInfluence * sphereMult;
						if (sphereOfInfluence < body.planet.Radius * 1.5)
						{
							sphereOfInfluence = body.planet.Radius * 1.5;
							// Parent body must now have at minimum double that value as its SOI
							double parentSOI = sphereOfInfluence * WarpRNG.GenerateFloat (2.0f, 3.0f);
							body.solarSystem.AdjustPlanetSOI (body.referenceBodyData.planetID, parentSOI);
							// Gravity must also reflect this
							// Remove the parent's radius from the SOI calculations
							parentSOI -= body.referenceBodyData.planet.Radius * 1.5;
							// New gravity multiplier is based on how much stronger this is than Kerbin's SOI
							double parentGravityMult = parentSOI / KERBIN_SOI;
							body.solarSystem.AdjustPlanetGravity (body.referenceBodyData.planetID, KERBIN_GRAVITY * parentGravityMult);
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

		public static bool IsSun (CelestialBody check)
		{
			if (check == null)
			{
				Debugger.LogError ("Sun check was null!");
				return false;
			}
			return check.name == check.referenceBody.name;
		}
	}
}

