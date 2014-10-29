using UnityEngine;

namespace RandomizedSystems
{
	public class Hyperdrive : PartModule
	{
		private Rect windowPosition;
		public static int seed = 0;
		public static string seedString = AstroUtils.KERBIN_SYSTEM_COORDS;
		public static bool hasInit = false;

		public override void OnStart (StartState state)
		{
			if (!hasInit && state != StartState.Editor && state != StartState.None)
			{
				if (seedString == AstroUtils.KERBIN_SYSTEM_COORDS)
				{
					Events ["JumpToKerbol"].active = false;
				}
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
			RenderingManager.AddToPostDrawQueue (0, OnDraw);
		}

		[KSPEvent(guiActive = true, guiName = "Return to Kerbol", active = false)]
		/// <summary>
		/// Jumps to kerbol.
		/// </summary>
		public void JumpToKerbol ()
		{
			string tempString = seedString;
			seedString = AstroUtils.KERBIN_SYSTEM_COORDS;
			Warp (true);
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
			GUILayout.BeginVertical (GUILayout.Width (250.0f));
			seedString = GUILayout.TextField (seedString);
			if (GUILayout.Button ("Start Warp Drive"))
			{
				if (seedString != "")
				{
					Warp (true);
				}
				else
				{
					ScreenMessages.PostScreenMessage ("Invalid coordinates.", 3.0f, ScreenMessageStyle.UPPER_CENTER);
				}
				RenderingManager.RemoveFromPostDrawQueue (0, OnDraw);
			}
			GUILayout.EndVertical ();

			GUI.DragWindow ();
		}

		private void Warp (bool showMessage)
		{
			SolarData system = null;
			try
			{
				Randomizers.WarpRNG.ReSeed (seedString);
				system = SolarData.CreateSystem (seedString);
			}
			catch (System.Exception e)
			{
				Debugger.LogException ("Unable to jump to system!", e);
				ScreenMessages.PostScreenMessage ("Warp Drive failed due to " + e.GetType () + "." +
					"\nPlease press Alt+F2 and copy and paste or send a screenshot of the debugger to the Warp Drive developers!" +
					"\nException Message: " + e.Message, 10.0f, ScreenMessageStyle.UPPER_CENTER);
				return;
			}
			Debugger.LogWarning ("Created system " + system.name + " from string " + seedString + ".");
			if (showMessage)
			{
				ScreenMessages.PostScreenMessage ("Warp Drive initialized. Traveling to the " + system.name + " system, at coordinates " + seedString + ".", 3.0f, ScreenMessageStyle.UPPER_CENTER);
			}
			if (seedString != AstroUtils.KERBIN_SYSTEM_COORDS)
			{
				Events ["JumpToKerbol"].active = true;
			}
		}
	}
}

