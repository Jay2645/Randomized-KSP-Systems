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
					if (RemoveVesselFromSystem (vessel))
					{
						if (persistentVessels.ContainsKey (vessel.id))
						{
							persistentVessels [vessel.id].loaded = false;
						}
					}
					else
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
				return false;
			}
			else
			{
				HighLogic.CurrentGame.DestroyVessel (toRemove);
				toRemove.DestroyVesselComponents ();
				return true;
			}
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
			PersistentVessel persistentVessel = new PersistentVessel (seed, proto);
			Vessel vessel = persistentVessel.Spawn ();
			persistentVessels [vessel.id] = persistentVessel;
			return vessel;
		}
	}
	/*public class VesselManager
	{
		public VesselManager (FlightState flightState)
		{
			this.flightState = flightState;
			foreach (Vessel v in FlightGlobals.Vessels)
			{
				loadedVesselIDs.Add (v.id, v);
			}
		}

		private Dictionary<Guid,Vessel> loadedVesselIDs = new Dictionary<Guid, Vessel> ();
		private FlightState flightState;

		public Vessel LoadVessel (ProtoVessel proto)
		{
			Guid protoID = proto.vesselID;
			if (protoID != Guid.Empty && loadedVesselIDs.ContainsKey (protoID))
			{
				// Already have a vessel with this ID
				return loadedVesselIDs [protoID];
			}
			// Loads the vessel itself and assigns it to vesselRef
			proto.Load (flightState);
			Vessel vessel = proto.vesselRef;
			// Update the Guid with the "actual" Guid
			protoID = vessel.id;
			// We haven't added ourselves to the Guid list yet, so we need to double-check to make sure we're still okay
			if (loadedVesselIDs.ContainsKey (protoID))
			{
				// Looks like we did already exist after all
				RemoveVesselFromSystem (proto.vesselRef);
				return loadedVesselIDs [protoID];
			}
			Debugger.LogWarning ("Loaded vessel " + proto.vesselRef.vesselName + ", ID: " + protoID.ToString ());
			// Add the current ID to the Guid list
			loadedVesselIDs.Add (protoID, vessel);
			return vessel;
		}

		private void ValidateVessels ()
		{
			Debugger.LogWarning ("Validating vessels.");
			// Get all vessels loaded and all vessels we know about
			List<Vessel> loadedVessels = new List<Vessel> (FlightGlobals.Vessels);
			Dictionary<Guid,Vessel> vesselIDs = new Dictionary<Guid, Vessel> (loadedVesselIDs);
			foreach (Vessel v in FlightGlobals.Vessels.ToArray())
			{
				Debugger.LogWarning (v.vesselName);
				Guid vesselID = v.id;
				// If we know about this vessel, remove it from the list
				if (vesselIDs.ContainsKey (vesselID))
				{
					vesselIDs.Remove (vesselID);
					loadedVessels.Remove (v);
				}
			}
			// List now contains only vessels we don't know about or are duplicates
			Vessel[] badVessels = loadedVessels.ToArray ();
			foreach (Vessel v in badVessels)
			{
				RemoveVesselFromSystem (v);
			}
		}

		public void LoadAllProtoVessels ()
		{
			foreach (ProtoVessel vessel in flightState.protoVessels)
			{
				LoadVessel (vessel);
			}
			ValidateVessels ();
		}

public static bool RemoveVesselFromSystem (Vessel toRemove)
		{
			Debugger.LogWarning ("Clearing " + toRemove.vesselName);
			if (toRemove.loaded)
			{
				Debugger.LogError (toRemove.vesselName + " is loaded!");
				return false;
			}
			else
			{
				HighLogic.CurrentGame.DestroyVessel (toRemove);
				toRemove.DestroyVesselComponents ();
				return true;
			}
		}

		public static void EnsureInLoadedVessels (Vessel toAdd)
		{
			foreach (Vessel vessel in FlightGlobals.Vessels)
			{
				if (vessel.id == toAdd.id)
				{
					return;
				}
			}
			// Not in vessel list
			FlightGlobals.Vessels.Add (toAdd);
		}

		public static void EnsureUniqueVessels ()
		{
			Dictionary<Guid,Vessel> vesselLookup = new Dictionary<Guid, Vessel> ();
			Vessel[] allVessels = FlightGlobals.Vessels.ToArray ();
			for (int i = 0; i < allVessels.Length; i++)
			{
				Vessel vessel = allVessels [i];
				Debugger.Log ("Vessel: " + vessel.vesselName);
				if (vesselLookup.ContainsKey (vessel.id))
				{
					Debugger.LogError (vessel.vesselName + " is not unique!");
					if (!VesselManager.RemoveVesselFromSystem (vessel))
					{
						// Unable to remove ourselves from the system for whatever reason
						// Try to remove the other copy
						Vessel oldVessel = vesselLookup [vessel.id];
						if (VesselManager.RemoveVesselFromSystem (oldVessel))
						{
							vesselLookup [vessel.id] = vessel;
						}
						else
						{
							Debugger.LogError ("Unable to ensure " + vessel.vesselName + " is unique!");
						}
					}
					continue;
				}
				vesselLookup.Add (vessel.id, vessel);
			}
			FlightGlobals.Vessels.Clear ();
			foreach (KeyValuePair<Guid,Vessel> kvp in vesselLookup)
			{
				FlightGlobals.Vessels.Add (kvp.Value);
			}
		}
	}*/
}

