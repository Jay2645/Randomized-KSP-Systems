using System.IO;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace RandomizedSystems.Persistence
{
	/// <summary>
	/// This handles modifying the persistence files of a system.
	/// </summary>
	public static class PersistenceGenerator
	{
		public static void CreatePersistenceFile (string oldSeed, string newSeed)
		{
			if (string.IsNullOrEmpty (oldSeed))
			{
				return;
			}
			Vessel ourVessel = FlightGlobals.ActiveVessel;
			if (ourVessel == null)
			{
				return;
			}
			// We remove the active vessel so it doesn't get written to the "last" system
			// It should jump out of the current system
			// We don't destroy the vessel "normally" because that would also destroy all parts
			FlightGlobals.Vessels.Remove (ourVessel);
			// Reset the flight state to flush the ProtoVessels
			HighLogic.CurrentGame.flightState = new FlightState ();
			// Save the game to a new persistence file
			string persistence = FindPersistenceFile ();
			if (string.IsNullOrEmpty (persistence))
			{
				Debugger.LogError ("Could not find persistence file!");
				return;
			}
			// Save a snapshot of our current game to Star Systems just before we jump
			GamePersistence.SaveGame (oldSeed + "_persistent", Path.Combine (HighLogic.SaveFolder, "Star Systems"), SaveMode.OVERWRITE);
			if (SystemPersistenceExists (persistence, newSeed))
			{
				// Copy over the "old" persistence file and delete the cached version in Star Systems
				CopyPersistenceFileFromSystems (persistence, newSeed);
				// Load the game
				HighLogic.CurrentGame = GamePersistence.LoadGame ("persistent", HighLogic.SaveFolder, true, false);
				/* Do we need to start?
				 * HighLogic.CurrentGame.startScene = GameScenes.FLIGHT;
				HighLogic.CurrentGame.Start ();*/
			}
			else
			{
				// Get all the vessels
				Vessel[] allVessels = FlightGlobals.Vessels.ToArray ();
				foreach (Vessel v in allVessels)
				{
					// Destroy them all
					// We're not in this list anymore, so don't worry about us!
					HighLogic.CurrentGame.DestroyVessel (v);
					v.DestroyVesselComponents ();
				}
			}
			// Add us back to the active vessel list
			FlightGlobals.Vessels.Add (ourVessel);
			// Create a blank FlightState for the new system
			HighLogic.CurrentGame.flightState = new FlightState ();
			// Save us to the present persistence file
			GamePersistence.SaveGame ("persistent", HighLogic.SaveFolder, SaveMode.OVERWRITE);
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

		public static void CopyPersistenceFileFromSystems (string persistence, string prefix)
		{
			try
			{
				// Find the persistence directory
				string persistenceDirectory = Path.GetDirectoryName (persistence);
				// Navigate to the star systems subfolder
				persistenceDirectory = Path.Combine (persistenceDirectory, "Star Systems");
				// Generate our filename
				string persistenceFilename = prefix + "_" + Path.GetFileName (persistence);
				// Add our filename to the combined path
				string combinedPath = Path.Combine (persistenceDirectory, persistenceFilename);
				if (!File.Exists (combinedPath))
				{
					// Make sure we exist
					Debugger.LogException ("", new IOException ("Cannot copy persistence file over because Star Systems file does not exist!"));
					return;
				}
				byte[] cachedBytes = File.ReadAllBytes (combinedPath);
				// Delete the cached file, since we don't need it anymore
				File.Delete (combinedPath);
				// Copy over the filepath found in systems
				File.WriteAllBytes (persistence, cachedBytes);
			}
			catch (IOException e)
			{
				Debugger.LogException ("Could not copy from Star Systems!", e);
			}
		}
	}
}
