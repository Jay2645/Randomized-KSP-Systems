using UnityEngine;

namespace RandomizedSystems.Randomizers
{
	public static class WarpRNG
	{
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

		public static int GenerateInt (int min, int max)
		{
			Seed ();
			return Random.Range (min, max);
		}

		public static float GenerateFloat (float min, float max)
		{
			Seed ();
			return Random.Range (min, max);
		}

		public static float GetValue ()
		{
			Seed ();
			return Random.value;
		}

		public static string GenerateName ()
		{
			Seed ();
			string prefix = prefixes [Random.Range (0, prefixes.Length)];
			string suffix = suffixes [Random.Range (0, suffixes.Length)];
			return prefix + suffix;
		}

		private static void Seed ()
		{
			Random.seed = Hyperdrive.seed;
			Hyperdrive.seed++;
		}
	}
}

