using UnityEngine;
using RandomizedSystems.Persistence;

namespace RandomizedSystems
{
	public class Hyperdrive : PartModule
	{
		private Rect windowPosition;
		public static int seed = 0;
		public static string seedString = AstroUtils.KERBIN_SYSTEM_COORDS;
		private string lastSeed = string.Empty;
		public static bool hasInit = false;

		public override void OnStart (StartState state)
		{
			if (!hasInit && state != StartState.Editor && state != StartState.None)
			{
				seedString = AstroUtils.KERBIN_SYSTEM_COORDS;
				Warp (false);
				hasInit = true;
			}
		}

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
			lastSeed = seedString;
			RenderingManager.AddToPostDrawQueue (0, OnDraw);
		}

		[KSPEvent(guiActive = true, guiName = "Return to Kerbol", active = false)]
		/// <summary>
		/// Jumps to kerbol.
		/// </summary>
		public void JumpToKerbol ()
		{
			seedString = AstroUtils.KERBIN_SYSTEM_COORDS;
			Warp (true);
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
			GUILayout.BeginVertical (GUILayout.Width (250.0f));
			seedString = GUILayout.TextField (seedString);
			if (GUILayout.Button ("Start Warp Drive") || Input.GetKeyDown (KeyCode.Return) || Input.GetKeyDown (KeyCode.KeypadEnter))
			{
				Warp (true);
				RenderingManager.RemoveFromPostDrawQueue (0, OnDraw);
			}
			GUILayout.EndVertical ();

			GUI.DragWindow ();
		}

		private void Warp (bool showMessage)
		{
			SolarData system = null;
			seedString = seedString.Replace ("\n", string.Empty);
			if (string.IsNullOrEmpty (seedString))
			{
				ScreenMessages.PostScreenMessage ("Invalid coordinates.", 3.0f, ScreenMessageStyle.UPPER_CENTER);
			}
			try
			{
				system = SolarData.CreateSystem (seedString);
				PersistenceGenerator.CreatePersistenceFile (lastSeed, seedString);
				SeedTracker.Jump ();
			}
			catch (System.Exception e)
			{
				// Catch all exceptions so users know if something goes wrong
				ScreenMessages.PostScreenMessage ("Warp Drive failed due to " + e.GetType () + ".");
				Debugger.LogException ("Unable to jump to system!", e);
				return;
			}
			Debugger.LogWarning ("Created system " + system.name + " from string " + seedString + ".");
			if (showMessage)
			{
				ScreenMessages.PostScreenMessage ("Warp Drive initialized. Traveling to the " + system.name + " system, at coordinates " + seedString + ".", 3.0f, ScreenMessageStyle.UPPER_CENTER);
				if (seedString == AstroUtils.KERBIN_SYSTEM_COORDS)
				{
					Events ["JumpToKerbol"].active = false;
				}
				else
				{
					Events ["JumpToKerbol"].active = true;
				}
			}
		}
	}
}

