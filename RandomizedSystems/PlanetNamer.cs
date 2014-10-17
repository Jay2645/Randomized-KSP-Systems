using UnityEngine;

namespace RandomizedSystems
{
	public static class Randomizer
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
			"Pla",
			"Gal",
			"Rea",
			"Olym"
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
			"vin"
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

	public class Hyperdrive : PartModule
	{
		public static int seed = 0;
		private Rect windowPosition = new Rect ();
		private string seedString = "";

		[KSPEvent(guiActive = true, guiName = "Start Warp Drive")]
		public void StartHyperspaceJump ()
		{
			CelestialBody reference = FlightGlobals.currentMainBody;
			if (reference.referenceBody.name != reference.name)
			{
				ScreenMessages.PostScreenMessage ("Warp Drive cannot be activated. Please enter orbit around the nearest star.", 5.0f, ScreenMessageStyle.UPPER_CENTER);
				return;
			}
			RenderingManager.AddToPostDrawQueue (0, OnDraw);
		}

		private void OnDraw ()
		{
			if (this.vessel == FlightGlobals.ActiveVessel)
			{
				windowPosition = GUILayout.Window (10, windowPosition, OnWindow, "Enter Hyperspace Coordinates");
			}
		}

		private void OnWindow (int windowID)
		{
			GUILayout.BeginHorizontal (GUILayout.Width (250.0f));
			seedString = GUILayout.TextField (seedString);
			if (GUILayout.Button ("Start Warp Drive"))
			{
				if (seedString != "")
				{
					Warp ();
				}
				else
				{
					ScreenMessages.PostScreenMessage ("Invalid coordinates", 5.0f, ScreenMessageStyle.UPPER_CENTER);
				}
				RenderingManager.RemoveFromPostDrawQueue (0, OnDraw);
			}
			GUILayout.EndHorizontal ();

			GUI.DragWindow ();
		}

		private void Warp ()
		{
			seed = 0;
			foreach (char c in seedString)
			{
				seed += (int)c;
			}
			ScreenMessages.PostScreenMessage ("Initializing Warp Drive", 5.0f, ScreenMessageStyle.UPPER_CENTER);
			CelestialBody reference = FlightGlobals.currentMainBody;
			PlanetData sunData = new PlanetData (reference);
			sunData.RandomizeValues (true);
			sunData.ApplyChanges (true);
		}
	}
}

