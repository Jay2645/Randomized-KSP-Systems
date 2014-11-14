using System;
using System.Collections.Generic;
using RandomizedSystems.WarpDrivers;

namespace RandomizedSystems.Vessels
{
	public class PersistentVessel
	{
		public PersistentVessel (string seed, ProtoVessel protoVessel)
		{
			this.seed = seed;
			this.protoVessel = protoVessel;
			id = protoVessel.vesselID;
		}

		private string seed;
		private ProtoVessel protoVessel;

		public Guid id
		{
			get;
			private set;
		}

		public bool loaded
		{
			get
			{
				return _loaded;
			}
			private set
			{
				_loaded = value;
				if (value)
				{

				}
			}
		}

		private bool _loaded = false;
		private static Dictionary<string, Dictionary<Guid,Vessel>> seedGUIDLookup = new Dictionary<string, Dictionary<Guid, Vessel>> ();

		public Vessel Spawn ()
		{
			if (loaded)
			{
				if (protoVessel.vesselRef == null)
				{
					loaded = false;
				}
				else
				{
					return protoVessel.vesselRef;
				}
			}
			if (WarpDrive.seedString != seed || id == Guid.Empty)
			{
				// Only spawn us if we belong to the current system and have a valid ID
				return null;
			}
			// We have a valid seed, make sure we're unique
			Dictionary<Guid,Vessel> currentGuidLookup = null;
			if (seedGUIDLookup.ContainsKey (seed))
			{
				currentGuidLookup = seedGUIDLookup [seed];
			}
			else
			{
				currentGuidLookup = new Dictionary<Guid, Vessel> ();
			}
			if (currentGuidLookup.ContainsKey (id))
			{
				loaded = true;
				// Already spawned in this seed
				return currentGuidLookup [id];
			}
			// See if we're already in the loaded game
			// This shouldn't happen, but we want to make absolutely sure so we don't have duplicates
			foreach (Vessel vessel in FlightGlobals.Vessels)
			{
				if (vessel.id == id)
				{
					// Should not happen
					Debugger.LogWarning ("Tried to spawn duplicate vessel! Name: " + vessel.name);
					currentGuidLookup.Add (id, vessel);
					seedGUIDLookup [seed] = currentGuidLookup;
					loaded = true;
					return vessel;
				}
			}
			protoVessel.Load (HighLogic.CurrentGame.flightState);
			GameEvents.onNewVesselCreated.Fire (protoVessel.vesselRef);
			currentGuidLookup.Add (id, protoVessel.vesselRef);
			seedGUIDLookup [seed] = currentGuidLookup;
			loaded = protoVessel.vesselRef != null;
			return protoVessel.vesselRef;
		}

		public bool Despawn ()
		{
			if (protoVessel.vesselRef == null)
			{
				Debugger.LogError ("Cannot despawn persistent vessel " + protoVessel.vesselName + " because vessel reference was null!");
				return false;
			}
			HighLogic.CurrentGame.DestroyVessel (protoVessel.vesselRef);
			protoVessel.vesselRef.DestroyVesselComponents ();
			loaded = false;
			return true;
		}

		public void Warp (string newSeed)
		{
			Dictionary<Guid,Vessel> vesselLookup = null;
			if (seedGUIDLookup.ContainsKey (seed))
			{
				// Remove ourselves from the old system
				vesselLookup = seedGUIDLookup [seed];
				vesselLookup.Remove (id);
				seedGUIDLookup [seed] = vesselLookup;
			}
			this.seed = newSeed;
			// Add ourselves to the new system
			if (seedGUIDLookup.ContainsKey (seed))
			{
				vesselLookup = seedGUIDLookup [seed];
			}
			else
			{
				vesselLookup = new Dictionary<Guid, Vessel> ();
			}
			vesselLookup.Add (id, protoVessel.vesselRef);
			seedGUIDLookup [seed] = vesselLookup;
		}
	}
}