using System.Collections.Generic;
using System;
using UnityEngine;
using RandomizedSystems.WarpDrivers;
using RandomizedSystems.SaveGames;

namespace RandomizedSystems.Vessels
{
	[KSPAddon(KSPAddon.Startup.SpaceCentre,false)]
	public class VesselManager : MonoBehaviour
	{
		/// <summary>
		/// Keeps track of which VesselManager is active.
		/// This makes sure we don't get tossed in the garbage.
		/// </summary>
		private static VesselManager reference = null;
		/// <summary>
		/// A list of every single GUID in the game and which seed it belongs to.
		/// </summary>
		private static Dictionary<Guid,string> vesselSeeds = new Dictionary<Guid, string> ();
		private static Dictionary<Guid,PersistentVessel> persistentVessels = new Dictionary<Guid, PersistentVessel> ();

		private void Start ()
		{
			OnEnteredSpaceCenter ();
			if (reference != null)
			{
				Destroy (this);
				return;
			}
			reference = this;
			DontDestroyOnLoad (this);
		}

		private static void OnEnteredSpaceCenter ()
		{
			Debugger.Log ("Entered space center!");
			string warpSeed = WarpDrive.seedString;
			if (warpSeed != AstroUtils.KERBIN_SYSTEM_COORDS)
			{
				// Load the Kerbin snapshot
				PersistenceGenerator.LoadSnapshot (AstroUtils.KERBIN_SYSTEM_COORDS);
			}
		}

		/// <summary>
		/// Forces all vessels in the current seed to be unloaded.
		/// Will save a snapshot of the current system beforehand.
		/// </summary>
		/// <param name="warpSeed">The seed string of the current system (the one being unloaded).</param>
		public static void FlushVesselCache (string warpSeed)
		{
			FlushVesselCache (warpSeed, Guid.Empty);
		}

		public static void FlushVesselCache (string warpSeed, Guid unloadIgnoreID)
		{
			// Save a snapshot
			PersistenceGenerator.SaveSnapshot (warpSeed);
			// Cache the current system
			Vessel[] allVessels = GameObject.FindObjectsOfType<Vessel> ();
			List<Vessel> unclearableVessels = new List<Vessel> ();
			for (int i = 0; i < allVessels.Length; i++)
			{
				Vessel vessel = allVessels [i];
				// Clear the vessel unless we are asked to ignore it
				if (vessel.id != unloadIgnoreID || unloadIgnoreID == Guid.Empty)
				{
					vesselSeeds [vessel.id] = warpSeed;
					if (!RemoveVesselFromSystem (vessel))
					{
						Debugger.LogWarning ("Could not unload " + vessel.name);
						unclearableVessels.Add (vessel);
					}
				}
			}
			// Clear the cache
			FlightGlobals.Vessels.Clear ();
			HighLogic.CurrentGame.flightState.protoVessels.Clear ();
			// If we couldn't unload something, ensure it's not duplicated
			foreach (Vessel vessel in unclearableVessels)
			{
				FlightGlobals.Vessels.Add (vessel);
				HighLogic.CurrentGame.flightState.protoVessels.Add (vessel.BackupVessel ());
			}
		}

		public static bool RemoveVesselFromSystem (Vessel toRemove)
		{
			Debugger.LogWarning ("Clearing " + toRemove.vesselName);
			if (toRemove.loaded)
			{
				Debugger.LogError (toRemove.vesselName + " is loaded!");
			}
			else
			{
				Guid id = toRemove.id;
				if (persistentVessels.ContainsKey (id))
				{
					PersistentVessel vessel = persistentVessels [id];
					return vessel.Despawn ();
				}
				else
				{
					Debugger.LogError (toRemove.vesselName + " was never cached as a PersistentVessel!");
				}
			}
			return false;
		}

		public static PersistentVessel GetPersistentVessel (Guid vesselID)
		{
			if (persistentVessels.ContainsKey (vesselID))
			{
				return persistentVessels [vesselID];
			}
			return null;
		}

		public static Vessel LoadVessel (string seed, ProtoVessel proto)
		{
			PersistentVessel persistentVessel = LoadPersistentVessel (seed, proto);
			Vessel vessel = persistentVessel.Spawn ();
			return vessel;
		}

		public static PersistentVessel LoadPersistentVessel (string seed, ProtoVessel proto)
		{
			PersistentVessel persistentVessel = new PersistentVessel (seed, proto);
			Guid id = proto.vesselID;
			if (id == Guid.Empty)
			{
				Debugger.LogWarning ("Vessel " + proto.vesselName + " not cached because GUID was empty!");
			}
			else
			{
				vesselSeeds [id] = seed;
				persistentVessels [id] = persistentVessel;
			}
			return persistentVessel;
		}

		public static void ClearNonSystemVessels ()
		{
			foreach (Vessel v in GameObject.FindObjectsOfType<Vessel>())
			{
				Guid id = v.id;
				if (vesselSeeds.ContainsKey (id))
				{
					string seed = vesselSeeds [id];
					PersistentVessel persistentVessel = persistentVessels [id];
					if (persistentVessel.loaded)
					{
						Debugger.Log ("Vessel: " + v.vesselName + ", seed " + seed + ", ID: " + id.ToString ());
						if (seed != WarpDrive.seedString)
						{
							Debugger.LogWarning ("Vessel is in the wrong seed!");
						}
					}
				}
				else
				{
					Debugger.LogError ("Vessel loaded but not cached: " + v.vesselName + ", ID: " + id.ToString ());
				}
			}
		}
	}
}

