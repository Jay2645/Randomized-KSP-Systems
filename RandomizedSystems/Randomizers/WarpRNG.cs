using UnityEngine;
using System;
using RandomizedSystems.WarpDrivers;

namespace RandomizedSystems.Randomizers
{
	/// <summary>
	/// Generates pseudo-random values based off of a seed.
	/// </summary>
	public static class WarpRNG
	{
		private static int index = 0;
		//private static long seed = 0;
		private static System.Random rng = new System.Random (0);

		/// <summary>
		/// Initializes the randomizer from a string
		/// </summary>
		/// <param name="seedstring">seedstring</param>
		public static void ReSeed (string seedstring)
		{
			rng = new System.Random (seedstring.GetHashCode ());
		}

		/// <summary>
		/// Generates an int between min (inclusive) and max (exclusive)
		/// </summary>
		/// <returns>A pseudorandom int.</returns>
		/// <param name="min">Minimum.</param>
		/// <param name="max">Max.</param>
		public static int GenerateInt (int min, int max)
		{
			return rng.Next (min, max);
		}

		/// <summary>
		/// Generates a float between min and max, exclusive.
		/// </summary>
		/// <returns>A pseudorandom float.</returns>
		/// <param name="min">Minimum.</param>
		/// <param name="max">Max.</param>
		public static float GenerateFloat (float min, float max)
		{
			return (float)GenerateDouble (min, max);
		}

		/// <summary>
		/// Generates a double between min and max, exclusive.
		/// </summary>
		/// <returns>A pseudorandom double.</returns>
		/// <param name="min">Minimum.</param>
		/// <param name="max">Max.</param>
		public static double GenerateDouble (double min, double max)
		{
			return min + rng.NextDouble () * (max - min);
		}

		/// <summary>
		/// Generates a float between 0 and 1, inclusive.
		/// </summary>
		/// <returns>A pseudorandom value.</returns>
		public static float GetValue ()
		{
			return (float)rng.NextDouble ();
		}

		/// <summary>
		/// Generates a double between 0 and 1, inclusive.
		/// </summary>
		/// <returns>A pseudorandom value.</returns>
		public static double GetValueDouble ()
		{
			return rng.NextDouble ();
		}

		/// <summary>
		/// Seeds the RNG. - OBSOLETE!!!
		/// </summary>
		[Obsolete("Slow. Use ReSeed() instead.")]
		private static void Seed ()
		{
			if (string.IsNullOrEmpty (WarpDrive.seedString))
			{
				// Can't seed
				return;
			}
			// Reset the index if we exceed the string length
			if (index >= WarpDrive.seedString.Length)
			{
				index = 0;
			}
			// We convert the next character in the seed string to an int
			int nextValue = (int)WarpDrive.seedString [index];
			if (int.MaxValue - nextValue <= WarpDrive.seed)
			{
				// Very unlikely to ever happen, but better safe than sorry
				WarpDrive.seed = 0;
			}
			// Add the char's value to the seed
			WarpDrive.seed += nextValue;
			// Seed the RNG
			UnityEngine.Random.seed = WarpDrive.seed;
			// Increment the index
			index++;
		}

		/// <summary>
		/// Generates a random number that follows a normal distribution, using a Box–Muller transform.
		/// </summary>
		/// <returns>A random number following a normal distribution.</returns>
		public static double GenerateNormalRandom ()
		{
			double valueOne = GetValueDouble ();
			double valueTwo = GetValueDouble ();
			return Math.Sqrt (-2 * Math.Log (valueOne)) * Math.Cos (2 * Math.PI * valueTwo);
		}
	}
}

