using UnityEngine;
using RandomizedSystems.Persistence;
using System;

namespace RandomizedSystems
{
	public class Hyperdrive : PartModule
	{
		public static bool hasInit = false;

		public override void OnStart (StartState state)
		{
			if (!hasInit && state != StartState.Editor && state != StartState.None)
			{
				HyperdriveWarper.SetSeed (AstroUtils.KERBIN_SYSTEM_COORDS);
				// This caches the Kerbin system
				HyperdriveWarper.Warp (false, Warp);
				hasInit = true;
			}
		}

		[KSPEvent(guiActive = true, guiName = "Start Warp Drive")]
		/// <summary>
		/// Starts the hyperspace jump.
		/// </summary>
		public void StartHyperspaceJump ()
		{
			// Can only warp around the sun
			CelestialBody reference = FlightGlobals.currentMainBody;
			if (reference.referenceBody.name != reference.name)
			{
				ScreenMessages.PostScreenMessage ("Warp Drive cannot be activated. Please enter orbit around the nearest star.", 5.0f, ScreenMessageStyle.UPPER_CENTER);
				return;
			}
			HyperdriveWarper.OpenWindow ();
		}

		[KSPEvent(guiActive = true, guiName = "Return to Kerbol", active = false)]
		/// <summary>
		/// Jumps to kerbol.
		/// </summary>
		public void JumpToKerbol ()
		{
			CelestialBody reference = FlightGlobals.currentMainBody;
			if (reference.referenceBody.name != reference.name)
			{
				ScreenMessages.PostScreenMessage ("Warp Drive cannot be activated. Please enter orbit around the nearest star.", 5.0f, ScreenMessageStyle.UPPER_CENTER);
				return;
			}
			HyperdriveWarper.SetSeed (AstroUtils.KERBIN_SYSTEM_COORDS);
			HyperdriveWarper.Warp (true, Warp);
		}

		private void Warp ()
		{
			if (HyperdriveWarper.seedString == AstroUtils.KERBIN_SYSTEM_COORDS)
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

