using System.Collections.Generic;
using System;

namespace RandomizedSystems.Vessels
{
	public class VesselManager
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
				Debugger.LogWarning (vessel.vesselName + " is unique!");
				vesselLookup.Add (vessel.id, vessel);
			}
			FlightGlobals.Vessels.Clear ();
			foreach (KeyValuePair<Guid,Vessel> kvp in vesselLookup)
			{
				FlightGlobals.Vessels.Add (kvp.Value);
			}
		}
	}
}

