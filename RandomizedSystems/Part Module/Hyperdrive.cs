using UnityEngine;

namespace RandomizedSystems
{
	public class Hyperdrive : PartModule
	{
		/// <summary>
		/// The seed.
		/// </summary>
		public static int seed = 0;
		private Rect windowPosition;
		private string seedString = "0";
		/*public override void OnActive ()
		{
			if(seedString == "0")
			{
				Events ["JumpToKerbol"].active = false;
			}
		}*/
		[KSPEvent(guiActive = true, guiName = "Start Warp Drive")]
		/// <summary>
		/// Starts the hyperspace jump.
		/// </summary>
		public void StartHyperspaceJump ()
		{
			CelestialBody reference = FlightGlobals.currentMainBody;
			if (reference.referenceBody.name != reference.name)
			{
				ScreenMessages.PostScreenMessage ("Warp Drive cannot be activated. Please enter orbit around the nearest star.", 5.0f, ScreenMessageStyle.UPPER_CENTER);
				return;
			}
			windowPosition = new Rect (100, 100, 0, 0);
			RenderingManager.AddToPostDrawQueue (0, OnDraw);
			Events ["JumpToKerbol"].active = true;
		}

		[KSPEvent(guiActive = true, guiName = "Return to Kerbol", active = false)]
		/// <summary>
		/// Jumps to kerbol.
		/// </summary>
		public void JumpToKerbol ()
		{
			string tempString = seedString;
			seedString = "0";
			Warp ();
			seedString = tempString;
			Events ["JumpToKerbol"].active = false;
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
					ScreenMessages.PostScreenMessage ("Invalid coordinates.", 3.0f, ScreenMessageStyle.UPPER_CENTER);
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
			SolarData system = SolarData.CreateSystem (seedString);
			Debug.LogWarning ("Created system " + system.name + " from string " + seedString + ". Seed value: " + seed);
			ScreenMessages.PostScreenMessage ("Warp Drive initialized. Traveling to the " + system.name + " system, at coordinates " + seedString + ".", 3.0f, ScreenMessageStyle.UPPER_CENTER);
		}
	}
}

