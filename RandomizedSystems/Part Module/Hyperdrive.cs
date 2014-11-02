using UnityEngine;
using RandomizedSystems.WarpDrivers;

namespace RandomizedSystems
{
	public class Hyperdrive : PartModule
	{
		public override void OnActive ()
		{
			ResetKerbolPrompt ();
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
			WarpDrive.SetNextWarpAction (new WarpDrive.OnWarpDelegate (WarpMessage), new WarpDrive.OnWarpDelegate (ResetKerbolPrompt));
			WarpDrive.OpenWindow ();
		}

		[KSPEvent(guiActive = true, guiName = "Return to Kerbol", active = false)]
		/// <summary>
		/// Jumps to kerbol.
		/// </summary>
		public void JumpToKerbol ()
		{
			WarpDrive.SetNextWarpAction (new WarpDrive.OnWarpDelegate (WarpMessage), new WarpDrive.OnWarpDelegate (ResetKerbolPrompt));
			WarpDrive.JumpToKerbol (true);
		}

		private void WarpMessage ()
		{
			ScreenMessages.PostScreenMessage ("Warp Drive initialized. Traveling to the " + 
				WarpDrive.currentSystem.name + " system, at coordinates " + 
				WarpDrive.seedString + ".", 
			                                  3.0f, 
			                                  ScreenMessageStyle.UPPER_CENTER);
		}

		private void ResetKerbolPrompt ()
		{
			if (WarpDrive.seedString == AstroUtils.KERBIN_SYSTEM_COORDS)
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

