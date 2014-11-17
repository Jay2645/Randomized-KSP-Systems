using System.IO;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System;
using RandomizedSystems.Vessels;
using RandomizedSystems.Systems;
using RandomizedSystems.WarpDrivers;

namespace RandomizedSystems.SaveGames
{
	/// <summary>
	/// This handles modifying the persistence files of a system.
	/// </summary>
	public static class PersistenceGenerator
	{
		/// <summary>
		/// Looks for a persistence file (persistent.sfs) in the current save folder.
		/// </summary>
		/// <returns>A full path to the persistence file, or an empty string if it was not found.</returns>
		public static string FindPersistenceFile ()
		{
			try
			{
				string appPath = KSPUtil.ApplicationRootPath;
				string saveFolder = "";
				string[] allDirectories = Directory.GetDirectories (appPath);
				foreach (string directory in allDirectories)
				{
					if (Path.GetFileName (directory).ToLower () == "saves")
					{
						saveFolder = directory;
						break;
					}
				}
				saveFolder = Path.Combine (saveFolder, HighLogic.SaveFolder);
				if (Directory.Exists (saveFolder))
				{
					string persistence = Path.Combine (saveFolder, AstroUtils.DEFAULT_PERSISTENCE + AstroUtils.SFS);
					string backupFolder = Path.Combine (saveFolder, AstroUtils.STAR_SYSTEM_FOLDER_NAME);
					if (!Directory.Exists (backupFolder))
					{
						Directory.CreateDirectory (backupFolder);
					}
					if (File.Exists (persistence))
					{
						return persistence;
					}
				}
			}
			catch (IOException e)
			{
				Debugger.LogException ("Could not find persistence file!", e);
			}
			return string.Empty;
		}

		public static bool SystemPersistenceExists (string persistence, string prefix)
		{
			string persistenceDirectory = Path.GetDirectoryName (persistence);
			persistenceDirectory = Path.Combine (persistenceDirectory, AstroUtils.STAR_SYSTEM_FOLDER_NAME);
			if (Directory.Exists (persistenceDirectory))
			{
				string persistenceFilename = prefix + "_" + Path.GetFileName (persistence);
				return File.Exists (Path.Combine (persistenceDirectory, persistenceFilename));
			}
			return false;
		}

		/// <summary>
		/// Loads a snapshot.
		/// Will warp to the desired seed, remove all vessels from the entire game, then reload only those in the new snapshot.
		/// </summary>
		/// <param name="newSeed">The seed we are warping to.</param>
		public static void LoadSnapshot (string lastSeed, string newSeed)
		{
			Debugger.Log ("Loading snapshot: " + newSeed);
			WarpDrive.Warp (false, newSeed, false);
			// Clear the old vessels
			ClearSystemVessels (lastSeed);
			// Add the new vessels
			AddPersistenceVessels (lastSeed, newSeed);
			// Save
			SavePersistence ();
		}

		/// <summary>
		/// ONLY loads the vessels in a snapshot. Will create the snapshot if one doesn't exist.
		/// </summary>
		/// <param name="oldSeed">Old seed.</param>
		/// <param name="newSeed">New seed.</param>
		/// <param name="removeVesselFromSystem">If set to <c>true</c> remove the active vessel from the old system and add it to the new.</param>
		public static void WarpSingleVessel (string oldSeed, string newSeed, Vessel toWarp)
		{
			if (string.IsNullOrEmpty (oldSeed) || oldSeed == newSeed)
			{
				return;
			}
			bool removeVesselFromSystem = toWarp != null;
			Guid vesselID = Guid.Empty;
			// Note the ID of the vessel we are warping and get it clear of FlightGlobals so it doesn't get unloaded
			if (removeVesselFromSystem)
			{
				vesselID = toWarp.id;
				FlightGlobals.Vessels.Remove (toWarp);
			}
			// Get rid of all the vessels in FlightGlobals
			ClearSystemVessels (oldSeed, newSeed, vesselID);
			// Add the new vessels
			AddPersistenceVessels (oldSeed, newSeed);
			// Add us back to the active vessel list
			if (removeVesselFromSystem)
			{
				FlightGlobals.Vessels.Add (toWarp);
				FlightGlobals.ForceSetActiveVessel (toWarp);
			}
			// This updates the map view
			OrbitDriver[] orbitDrivers = Planetarium.Orbits.ToArray ();
			foreach (OrbitDriver orbit in orbitDrivers)
			{
				if (orbit.vessel != null)
				{
					Planetarium.Orbits.Remove (orbit);
				}
			}
			foreach (Vessel v in FlightGlobals.Vessels)
			{
				Planetarium.Orbits.Add (v.orbitDriver);
			}
			// Save us to the present persistence file
			SavePersistence ();
		}

		/// <summary>
		/// Saves a snapshot of the current system exactly as it is.
		/// This is the most reliable way of capturing everything in the system.
		/// </summary>
		/// <param name="seed">The seed to save the snapshot as.</param>
		public static void SaveSnapshot (string seed)
		{
			Debugger.Log ("Saving snapshot: " + seed);
			SaveGame (seed, seed + AstroUtils.SEED_PERSISTENCE, AstroUtils.STAR_SYSTEM_FOLDER_NAME);
		}

		public static void SavePersistence ()
		{
			try
			{
				SaveGame (WarpDrive.seedString, AstroUtils.DEFAULT_PERSISTENCE, "", false);
			}
			catch (Exception e)
			{
				Debugger.LogException ("Attempted to save game but got exception.", e);
			}
		}

		private static void SaveGame (string seed, string filename, string subfolder = "", bool printVessels = true)
		{
			// Expand subfolder
			if (subfolder == "")
			{
				subfolder = HighLogic.SaveFolder;
			}
			else
			{
				subfolder = Path.Combine (HighLogic.SaveFolder, subfolder);
			}
			// Make sure everyone is on the same page
			Game savedGame = HighLogic.CurrentGame.Updated ();
			GamePersistence.SaveGame (savedGame, filename, subfolder, SaveMode.OVERWRITE);
			// Log all vessels in the current seed
			if (printVessels)
			{
				Debugger.Log ("Vessels in save " + seed + ":");
			}
			foreach (ProtoVessel v in HighLogic.CurrentGame.flightState.protoVessels)
			{
				if (printVessels)
				{
					Debugger.Log (v.vesselName);
				}
				// All this does is makes sure all the loaded vessels are internally set to the new seed
				VesselManager.LoadPersistentVessel (seed, v);
			}
			ForceTrackingStationUpdate ();
		}

		/// <summary>
		/// Removes all the vessels from a system.
		/// </summary>
		/// <param name="oldSeed">The seed of the vessels we are clearing.</param></param>
		public static void ClearSystemVessels (string oldSeed)
		{
			ClearSystemVessels (oldSeed, "", Guid.Empty);
		}

		public static void ClearSystemVessels (string oldSeed, string newSeed, Guid doNotFlush)
		{
			Debugger.Log ("Flushing vessel cache for seed " + oldSeed + ".");
			// Remove all old vessels
			VesselManager.FlushVesselCache (oldSeed, doNotFlush);
			// Warp any ignored vessels to the new system
			if (doNotFlush != Guid.Empty)
			{
				PersistentVessel currentVessel = VesselManager.GetPersistentVessel (doNotFlush);
				if (currentVessel != null)
				{
					Debugger.Log ("Warping " + currentVessel.name + " to seed " + newSeed);
					currentVessel.Warp (newSeed);
				}
			}
			ForceTrackingStationUpdate ();
		}

		/// <summary>
		/// Adds in the vessels from the new seed and saves the new seed to file.
		/// </summary>
		/// <param name="oldSeed">The seed we are leaving.</param>
		/// <param name="newSeed">The seed we are going to.</param>
		public static void AddPersistenceVessels (string oldSeed, string newSeed)
		{
			if (newSeed == AstroUtils.KERBIN_SYSTEM_COORDS && !WarpDrivers.WarpDrive.needsPurge)
			{
				// Don't need to merge, we never left Kerbol
				return;
			}
			string persistence = FindPersistenceFile ();
			if (string.IsNullOrEmpty (persistence))
			{
				Debugger.LogError ("Could not find persistence file!");
				return;
			}
			// Check to see if we already have a persistence file for this system
			if (SystemPersistenceExists (persistence, newSeed))
			{
				// Load the game
				// We don't actually have to load the WHOLE game, just the vessels
				Debugger.Log ("Loading existing system at " + newSeed);
				string path = Path.Combine (KSPUtil.ApplicationRootPath, "saves");
				path = Path.Combine (path, HighLogic.SaveFolder);
				path = Path.Combine (path, AstroUtils.STAR_SYSTEM_FOLDER_NAME);
				path = Path.Combine (path, newSeed + AstroUtils.SEED_PERSISTENCE + AstroUtils.SFS);
				// Generate root node from persistence file
				ConfigNode root = ConfigNode.Load (path).GetNode ("GAME");
				if (root == null)
				{
					throw new PlanetRandomizerException ("Could not load save file because the root node could not be found.");
				}
				// Find FLIGHTSTATE node in the root node
				ConfigNode flightStateNode = root.GetNode ("FLIGHTSTATE");
				// Generate new FlightState from the root
				FlightState flightState = new FlightState (flightStateNode, HighLogic.CurrentGame);
				// Load each ProtoVessel in the FlightState
				List<ProtoVessel> spawnedVessels = new List<ProtoVessel> ();
				List<Vessel> activeVessels = new List<Vessel> ();
				Debugger.Log (flightState.protoVessels.Count + " vessels in " + newSeed + ".");
				foreach (ProtoVessel proto in flightState.protoVessels)
				{
					Vessel vessel = VesselManager.LoadVessel (newSeed, proto);
					if (vessel == null)
					{
						Debugger.LogError (proto.vesselName + " was not spawned!");
					}
					else
					{
						Debugger.Log (vessel.vesselName + " is now in seed " + newSeed + ".");
						spawnedVessels.Add (proto);
						activeVessels.Add (vessel);
					}
				}
				flightState.protoVessels = spawnedVessels;
				HighLogic.CurrentGame.flightState = flightState;
				FlightGlobals.Vessels.Clear ();
				FlightGlobals.Vessels.AddRange (activeVessels);
				if (newSeed == AstroUtils.KERBIN_SYSTEM_COORDS)
				{
					// We've now purged the system of all the "old" data
					WarpDrivers.WarpDrive.needsPurge = false;
				}
			}
			else
			{
				// Create a blank FlightState for the new system
				HighLogic.CurrentGame.flightState = new FlightState ();
				Debugger.LogWarning ("Created a blank star system for seed " + newSeed);
			}
			Debugger.Log ("All vessels have been merged.");
			SavePersistence ();
		}

		private static void ForceTrackingStationUpdate ()
		{
			// This is a hacky way to force the tracking station to update
			try
			{
				GameEvents.onNewVesselCreated.Fire (null);
			}
			catch (NullReferenceException)
			{
				// Intentionally left blank
			}
		}
	}
}
