using System.IO;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System;
using RandomizedSystems.Vessels;
using RandomizedSystems.WarpDrivers;

namespace RandomizedSystems.Persistence
{
	/// <summary>
	/// This handles modifying the persistence files of a system.
	/// </summary>
	public static class PersistenceGenerator
	{
		public static void CreatePersistenceFile (string oldSeed, string newSeed, bool removeVesselFromSystem = true)
		{
			if (string.IsNullOrEmpty (oldSeed) || oldSeed == newSeed)
			{
				return;
			}
			Vessel ourVessel = FlightGlobals.ActiveVessel;
			if (ourVessel == null)
			{
				removeVesselFromSystem = false;
			}
			if (removeVesselFromSystem)
			{
				// We remove the active vessel so it doesn't get written to the "last" system
				// It should jump out of the current system
				// We don't destroy the vessel "normally" because that would also destroy all parts
				FlightGlobals.Vessels.Remove (ourVessel);
				// Reset the flight state to flush the ProtoVessels
				HighLogic.CurrentGame.flightState = new FlightState ();
				Game game = HighLogic.CurrentGame.Updated ();
				// Save a snapshot of our current game to Star Systems just before we jump
				GamePersistence.SaveGame (game, oldSeed + "_persistent", Path.Combine (HighLogic.SaveFolder, "Star Systems"), SaveMode.OVERWRITE);
			}
			else
			{
				// We can use the snapshot, which will reliably get *all* the vessels
				SaveSnapshot (oldSeed);
			}
			// Merge in all the other vessels
			MergePersistenceVessels (newSeed);
			if (removeVesselFromSystem)
			{
				// Add us back to the active vessel list
				FlightGlobals.Vessels.Add (ourVessel);
				FlightGlobals.ForceSetActiveVessel (ourVessel);
			}
			// Ensure that everyone is on the same page

			// Save us to the present persistence file
			SavePersistence ();
		}

		public static void MergePersistenceVessels (string newSeed)
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
			// Get all the vessels
			Vessel[] allVessels = FlightGlobals.Vessels.ToArray ();
			foreach (Vessel v in allVessels)
			{
				Debugger.Log ("Clearing " + v.vesselName);
				// Destroy them all
				VesselManager.RemoveVesselFromSystem (v);
			}
			// Check to see if we already have a persistence file for this system
			if (SystemPersistenceExists (persistence, newSeed))
			{
				// Load the game
				// We don't actually have to load the WHOLE game, just the vessels
				string path = Path.Combine (KSPUtil.ApplicationRootPath, "saves");
				path = Path.Combine (path, HighLogic.SaveFolder);
				path = Path.Combine (path, "Star Systems");
				path = Path.Combine (path, newSeed + "_persistent.sfs");
				// Generate root node from persistence file
				ConfigNode root = ConfigNode.Load (path).GetNode ("GAME");
				// Find FLIGHTSTATE node in the root node
				ConfigNode flightStateNode = root.GetNode ("FLIGHTSTATE");
				// Generate new FlightState from the root
				SolarData.currentSystem.flightState = new FlightState (flightStateNode, HighLogic.CurrentGame);
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
			}
			SavePersistence ();
		}

		/// <summary>
		/// Looks for a persistence file ("persistent.sfs") in the current save folder.
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
					string persistence = Path.Combine (saveFolder, "persistent.sfs");
					string backupFolder = Path.Combine (saveFolder, "Star Systems");
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
			persistenceDirectory = Path.Combine (persistenceDirectory, "Star Systems");
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
		/// <param name="seed">Seed.</param>
		public static void LoadSnapshot (string seed)
		{
			WarpDrive.Warp (false, seed);
			Vessel[] allVessels = (Vessel[])UnityEngine.Object.FindObjectsOfType<Vessel> ();
			foreach (Vessel v in allVessels)
			{
				VesselManager.RemoveVesselFromSystem (v);
			}
			SavePersistence ();
			MergePersistenceVessels (seed);
		}

		/// <summary>
		/// Saves a snapshot of the current system exactly as it is.
		/// This is the most reliable way of capturing everything in the system.
		/// </summary>
		/// <param name="seed">The seed to save the snapshot as.</param>
		public static void SaveSnapshot (string seed)
		{
			Debugger.Log ("Saving snapshot: " + seed);
			SaveGame (seed + "_persistent", "Star Systems");
		}

		public static void SavePersistence ()
		{
			Debugger.Log ("Saving persistence.");
			SaveGame ("persistent");
		}

		private static void SaveGame (string filename, string subfolder = "")
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
			Vessel[] allVessels = UnityEngine.Object.FindObjectsOfType<Vessel> ();
			Debugger.Log ("Saving game. Vessel count: " + allVessels.Length);
			foreach (Vessel vessel in allVessels)
			{
				// This will add us to FlightGlobals.Vessels if we are not in it already
				VesselManager.EnsureInLoadedVessels (vessel);
			}
			VesselManager.EnsureUniqueVessels ();
			Debugger.Log ("FlightGlobals vessel count: " + FlightGlobals.Vessels.Count);
			Game savedGame = HighLogic.CurrentGame.Updated ();
			Debugger.Log ("Known ProtoVessels:");
			foreach (ProtoVessel protoVessel in HighLogic.CurrentGame.flightState.protoVessels)
			{
				Debugger.LogWarning ("ProtoVessel: " + protoVessel.vesselName);
			}
			GamePersistence.SaveGame (savedGame, filename, subfolder, SaveMode.OVERWRITE);
			// This is a hacky way to force the tracking station to update
			try
			{
				GameEvents.onNewVesselCreated.Fire (null);
			}
			catch (NullReferenceException)
			{
				// Intentionally left blank
			}
			Debugger.LogWarning ("PersistenceGenerator has saved game as " + filename);
		}
	}
}
