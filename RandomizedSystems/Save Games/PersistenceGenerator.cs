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
		public static void LoadSnapshot (string newSeed)
		{
			string lastSeed = WarpDrive.seedString;
			WarpDrive.Warp (false, newSeed, false);
			MergePersistenceVessels (lastSeed, newSeed);
			SavePersistence ();
		}

		/// <summary>
		/// ONLY loads the vessels in a snapshot. Will create the snapshot if one doesn't exist.
		/// </summary>
		/// <param name="oldSeed">Old seed.</param>
		/// <param name="newSeed">New seed.</param>
		/// <param name="removeVesselFromSystem">If set to <c>true</c> remove the active vessel from the old system and add it to the new.</param>
		public static void LoadSnapshotVessels (string oldSeed, string newSeed, bool removeVesselFromSystem = true)
		{
			if (string.IsNullOrEmpty (oldSeed) || oldSeed == newSeed)
			{
				return;
			}
			// Get our current vessel
			Vessel ourVessel = FlightGlobals.ActiveVessel;
			// If we don't have an active vessel, clear the whole system
			if (ourVessel == null)
			{
				removeVesselFromSystem = false;
			}
			// If we AREN'T clearing the active vessel, note its ID
			Guid vesselID = Guid.Empty;
			if (removeVesselFromSystem)
			{
				vesselID = ourVessel.id;
				FlightGlobals.Vessels.Remove (ourVessel);
			}
			// Clear all the vessels except for the active vessel (if applicable)
			// This will also add in the new vessels
			MergePersistenceVessels (oldSeed, newSeed, vesselID);
			if (removeVesselFromSystem)
			{
				// Add us back to the active vessel list
				FlightGlobals.Vessels.Add (ourVessel);
				FlightGlobals.ForceSetActiveVessel (ourVessel);
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
				SaveGame (WarpDrive.seedString, AstroUtils.DEFAULT_PERSISTENCE);
			}
			catch (Exception e)
			{
				Debugger.LogException ("Attempted to save game but got exception.", e);
			}
		}

		private static void SaveGame (string seed, string filename, string subfolder = "")
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
			foreach (Vessel v in UnityEngine.GameObject.FindObjectsOfType<Vessel>())
			{
				VesselManager.LoadPersistentVessel (seed, v.BackupVessel ());
			}
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

		/// <summary>
		/// Clears the old vessels, adds in the vessels from the new seed, and saves the new seed to file.
		/// Will not warp us anywhere.
		/// </summary>
		/// <param name="oldSeed">The seed we are leaving.</param>
		/// <param name="newSeed">The seed we are going to.</param>
		public static void MergePersistenceVessels (string oldSeed, string newSeed)
		{
			MergePersistenceVessels (oldSeed, newSeed, Guid.Empty);
		}

		public static void MergePersistenceVessels (string oldSeed, string newSeed, Guid doNotFlush)
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
			// Remove all old vessels
			VesselManager.FlushVesselCache (oldSeed, doNotFlush);
			if (doNotFlush != Guid.Empty)
			{
				PersistentVessel currentVessel = VesselManager.GetPersistentVessel (doNotFlush);
				if (currentVessel != null)
				{
					currentVessel.Warp (newSeed);
				}
			}
			Debugger.Log ("Vessels flushed!");
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
				SolarData.currentSystem.flightState = new FlightState (flightStateNode, HighLogic.CurrentGame);
				foreach (ProtoVessel proto in SolarData.currentSystem.flightState.protoVessels)
				{
					VesselManager.LoadVessel (newSeed, proto);
				}
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
			Debugger.Log ("All vessels have been merged successfully.");
			SavePersistence ();
		}
	}
}
