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
			loaded = protoVessel.vesselRef != null;
			vesselType = protoVessel.vesselType;
			id = protoVessel.vesselID;
			Dictionary<Guid, PersistentVessel> seedPersistentVessels = null;
			if (persistentVesselLookup.ContainsKey (seed))
			{
				seedPersistentVessels = persistentVesselLookup [seed];
			}
			else
			{
				seedPersistentVessels = new Dictionary<Guid, PersistentVessel> ();
			}
			seedPersistentVessels [id] = this;
			persistentVesselLookup [seed] = seedPersistentVessels;
			vesselSeedLookup [id] = seed;
		}

		private string seed;
		private ProtoVessel protoVessel;

		public VesselType vesselType
		{
			get;
			private set;
		}

		public Guid id
		{
			get;
			private set;
		}

		public string name
		{
			get
			{
				if (protoVessel != null)
				{
					return protoVessel.vesselName;
				}
				return id.ToString ();
			}
		}

		public bool loaded
		{
			get;
			private set;
		}

		private static Dictionary<Guid, string> vesselSeedLookup = new Dictionary<Guid, string> ();
		private static Dictionary<string, Dictionary<Guid,Vessel>> seedGUIDLookup = new Dictionary<string, Dictionary<Guid, Vessel>> ();
		private static Dictionary<string, Dictionary<Guid, PersistentVessel>> persistentVesselLookup = new Dictionary<string, Dictionary<Guid, PersistentVessel>> ();

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
					Debugger.Log (name + " is already loaded.");
					return protoVessel.vesselRef;
				}
			}
			if (WarpDrive.seedString != seed)
			{
				Debugger.LogError ("Vessel " + name + " seed " + seed + " does not match Warp Drive seed " + WarpDrive.seedString + "!");
				// Don't spawn us
				return null;
			}
			if (id == Guid.Empty)
			{
				Debugger.LogWarning ("Generating new GUID for " + name + ".");
				id = Guid.NewGuid ();
				protoVessel.vesselID = id;
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
			if (currentGuidLookup.ContainsKey (id) && currentGuidLookup [id] != null)
			{
				loaded = true;
				Debugger.Log (name + " already exists in " + seed + ".");
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
			Debugger.Log ("Spawning vessel " + name);
			protoVessel.Load (HighLogic.CurrentGame.flightState);
			GameEvents.onNewVesselCreated.Fire (protoVessel.vesselRef);
			currentGuidLookup [id] = protoVessel.vesselRef;
			seedGUIDLookup [seed] = currentGuidLookup;
			loaded = protoVessel.vesselRef != null;
			if (!loaded)
			{
				Debugger.LogError ("Could not load " + protoVessel.vesselName + "!");
			}
			return protoVessel.vesselRef;
		}

		public bool Despawn ()
		{
			if (protoVessel.vesselRef != null)
			{
				HighLogic.CurrentGame.DestroyVessel (protoVessel.vesselRef);
				protoVessel.vesselRef.DestroyVesselComponents ();
				UnityEngine.GameObject.Destroy (protoVessel.vesselRef.gameObject);
				string oldSeed = vesselSeedLookup [id];
				if (seedGUIDLookup.ContainsKey (oldSeed))
				{
					if (seedGUIDLookup [seed].ContainsKey (id))
					{
						seedGUIDLookup [seed].Remove (id);
					}
				}
			}
			loaded = false;
			return true;
		}

		public void Warp (string newSeed)
		{
			WarpVessel (this, newSeed);
		}

		public static PersistentVessel CreateVessel (string seed, Vessel vessel)
		{
			return CreateVessel (seed, vessel.BackupVessel ());
		}

		public static PersistentVessel CreateVessel (string seed, ProtoVessel vessel)
		{
			Guid id = vessel.vesselID;
			if (id == Guid.Empty)
			{
				Debugger.LogWarning ("Generated GUID for " + vessel.vesselName);
				id = Guid.NewGuid ();
				vessel.vesselID = id;
			}
			else
			{
				foreach (KeyValuePair<string, Dictionary<Guid, PersistentVessel>> kvp in persistentVesselLookup)
				{
					// Make sure we don't exist in another system
					if (kvp.Value.ContainsKey (id))
					{
						PersistentVessel persistentVessel = kvp.Value [id];
						if (kvp.Key != seed)
						{
							Debugger.LogWarning ("Found " + persistentVessel.name + " in seed " + kvp.Key + ". Moving to seed " + seed + ".");
							persistentVessel.Warp (seed);
						}
						return persistentVessel;
					}
				}
			}
			return new PersistentVessel (seed, vessel);
		}

		public static void WarpVessel (PersistentVessel vessel, string warpSeed)
		{
			VesselType vesselType = vessel.vesselType;
			bool canWarp = vesselType != VesselType.Debris && 
				vesselType != VesselType.EVA && 
				vesselType != VesselType.Flag &&
				vesselType != VesselType.SpaceObject &&
				vesselType != VesselType.Unknown;
			string seed = vessel.seed;
			if (warpSeed == seed || !canWarp || !vessel.loaded)
			{
				if (!canWarp)
				{
					Debugger.Log (vessel.name + " was the wrong type of vessel to warp.");
				}
				return;
			}
			Guid id = vessel.id;

			// Remove ourselves from the old system
			if (seedGUIDLookup.ContainsKey (seed))
			{
				seedGUIDLookup [seed].Remove (id);
			}
			if (persistentVesselLookup.ContainsKey (seed))
			{
				persistentVesselLookup [seed].Remove (id);
			}

			// Change the vessel seed
			vessel.seed = warpSeed;

			// Add ourselves to the new system
			// PersistentVessel lookup
			Dictionary<Guid, PersistentVessel> seedPersistentVessels = null;
			if (persistentVesselLookup.ContainsKey (warpSeed))
			{
				seedPersistentVessels = persistentVesselLookup [warpSeed];
			}
			else
			{
				seedPersistentVessels = new Dictionary<Guid, PersistentVessel> ();
			}
			seedPersistentVessels [id] = vessel;
			persistentVesselLookup [warpSeed] = seedPersistentVessels;

			// Vessel Lookup
			Dictionary<Guid,Vessel> vesselLookup = null;
			if (seedGUIDLookup.ContainsKey (warpSeed))
			{
				vesselLookup = seedGUIDLookup [warpSeed];
			}
			else
			{
				vesselLookup = new Dictionary<Guid, Vessel> ();
			}
			vesselLookup [id] = vessel.protoVessel.vesselRef;
			seedGUIDLookup [warpSeed] = vesselLookup;

			vesselSeedLookup [id] = seed;
		}
	}
}