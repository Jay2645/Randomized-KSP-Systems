using UnityEngine;

namespace RandomizedSystems.Randomizers
{
	/// <summary>
	/// Generates pseudo-random values based off of a seed.
	/// </summary>
	public static class WarpRNG
	{
		/// <summary>
		/// Different prefixes for star names.
		/// </summary>
		public static string[] prefixes = new string[] {
			"Ker",
			"Jo",
			"Ear",
			"Ju",
			"Jeb",
			"Plu",
			"Nep",
			"Bes",
			"Tat",
			"Coru",
			"Dego",
			"Ho",
			"Geo",
			"Mu",
			"Usta",
			"Pla",
			"Gal",
			"Rea",
			"Olym",
			"Mor",
			"Mar",
			"Jup",
			"S",
			"Sat"
		};
		/// <summary>
		/// Different suffixes for star names.
		/// </summary>
		public static string[] suffixes = new string[] {
			"bin",
			"ol",
			"th",
			"to",
			"ne",
			"in",
			"ant",
			"bah",
			"sis",
			"n",
			"os",
			"ch",
			"dor",
			"vin",
			"s",
			"ury",
			"us",
			"it",
			"er",
			"urn",
			"une",
			"pau",
			"far",
		};
		private static int index = 0;

		/// <summary>
		/// Generates an int between min (inclusive) and max (exclusive)
		/// </summary>
		/// <returns>A pseudorandom int.</returns>
		/// <param name="min">Minimum.</param>
		/// <param name="max">Max.</param>
		public static int GenerateInt (int min, int max)
		{
			Seed ();
			return Random.Range (min, max);
		}

		/// <summary>
		/// Generates a float between min and max, inclusive.
		/// </summary>
		/// <returns>A pseudorandom float.</returns>
		/// <param name="min">Minimum.</param>
		/// <param name="max">Max.</param>
		public static float GenerateFloat (float min, float max)
		{
			Seed ();
			return Random.Range (min, max);
		}

		/// <summary>
		/// Generates a float between 0 and 1, inclusive.
		/// </summary>
		/// <returns>A pseudorandom value.</returns>
		public static float GetValue ()
		{
			Seed ();
			return Random.value;
		}

		/// <summary>
		/// Generates a random name, composed of prefix + suffix.
		/// </summary>
		/// <returns>A name.</returns>
		public static string GenerateName ()
		{
			Seed ();
			string prefix = prefixes [Random.Range (0, prefixes.Length)];
			string suffix = suffixes [Random.Range (0, suffixes.Length)];
			return prefix + suffix;
		}

		/// <summary>
		/// Seeds the RNG.
		/// </summary>
		private static void Seed ()
		{
			if (string.IsNullOrEmpty (HyperdriveWarper.seedString))
			{
				// Can't seed
				return;
			}
			// Reset the index if we exceed the string length
			if (index >= HyperdriveWarper.seedString.Length)
			{
				index = 0;
			}
			// We convert the next character in the seed string to an int
			int nextValue = (int)HyperdriveWarper.seedString [index];
			if (int.MaxValue - nextValue <= HyperdriveWarper.seed)
			{
				// Very unlikely to ever happen, but better safe than sorry
				HyperdriveWarper.seed = 0;
			}
			// Add the char's value to the seed
			HyperdriveWarper.seed += nextValue;
			// Seed the RNG
			Random.seed = HyperdriveWarper.seed;
			// Increment the index
			index++;
		}
	}
}

