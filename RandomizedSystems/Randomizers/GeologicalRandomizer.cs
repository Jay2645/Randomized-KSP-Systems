using UnityEngine;
using System.Collections.Generic;

namespace RandomizedSystems.Randomizers
{
	/// <summary>
	/// The geological randomizer affects anything which involves the actual surface of the body.
	/// This includes things like the size of the body and its rotation rate -- anything which doesn't affect other bodies.
	/// This also includes things like whether we are a gas giant or not and where our oceans are.
	/// </summary>
	public class GeologicalRandomizer : PlanetRandomizer
	{
		public GeologicalRandomizer (CelestialBody body, PlanetData bodyData)
		{
			SetBody (body, bodyData);
		}

		protected bool isGasGiant = false;
		protected Vector3 scale;
		protected double rotationPeriod;
		protected double radius;
		protected double mass;
		protected double density;

		public override void Cache ()
		{
			if (ScaledSpace.Instance != null)
			{
				List<Transform> planets = ScaledSpace.Instance.scaledSpaceTransforms;
				foreach (Transform planetTfm in planets)
				{
					if (planetTfm.name == planet.name)
					{
						scale = planetTfm.localScale;
						break;
					}
				}
			}
			rotationPeriod = planet.rotationPeriod;
			radius = planet.Radius;
			mass = planet.Mass;
			density = AstroUtils.CalculateDensity (mass, radius);
		}

		public override void Randomize ()
		{
			RandomizeScale ();
			RandomizeRotation ();
		}

		private void DeterminePlanetType ()
		{
			// 5% chance of becoming a gas giant
			if (WarpRNG.GetValue () <= 0.05f)
			{
				isGasGiant = true;
			}
		}

		private void RandomizeDensity ()
		{

		}

		private void RandomizeScale ()
		{
			// Kerbin is scaled to 0.1 by default (Jool is 1)
			float gravityMult = (float)planetData.gravityMultiplier;
			scale = new Vector3 (gravityMult * 0.1f, gravityMult * 0.1f, gravityMult * 0.1f);
		}

		private void RandomizeRotation ()
		{
			float value = WarpRNG.GenerateFloat (0.0f, 30.0f);
			rotationPeriod = value * 3600;
			if (WarpRNG.GetValue () < 0.10f)
			{
				rotationPeriod *= 30;
			}
		}

		public override void Apply ()
		{
			if (!IsSun ())
			{
				if (ScaledSpace.Instance != null)
				{
					List<Transform> planets = ScaledSpace.Instance.scaledSpaceTransforms;
					foreach (Transform planetTfm in planets)
					{
						if (planetTfm.name == planet.name)
						{
							Debugger.Log ("Scale: " + scale);
							//planetTfm.localScale = scale;
							planetTfm.name = planetData.generalRandomizer.GetName (false);
							break;
						}
					}
				}
			}
			Debugger.Log ("Rotation period: " + rotationPeriod + " seconds per rotation (" +
				(rotationPeriod / 60) + " minutes, " + ((rotationPeriod / 60) / 60) + " hours)");
			planet.rotationPeriod = rotationPeriod;
		}
	}
}

