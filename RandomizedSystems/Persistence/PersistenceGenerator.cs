using System.IO;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System;

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
			// Get all the vessels
			Vessel[] allVessels = FlightGlobals.Vessels.ToArray ();
			foreach (Vessel v in allVessels)
			{
				// Destroy them all
				// We're not in this list anymore, so don't worry about us!
				HighLogic.CurrentGame.DestroyVessel (v);
				v.DestroyVesselComponents ();
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
				FlightState flightState = new FlightState (flightStateNode, HighLogic.CurrentGame);
				// Load all the ProtoVessels into the game proper
				// (This part drove me nuts)
				foreach (ProtoVessel vessel in flightState.protoVessels)
				{
					vessel.Load (flightState);
				}
				// Reset the current FlightState
				HighLogic.CurrentGame.flightState = flightState;
			}
			else
			{
				// Create a blank FlightState for the new system
				HighLogic.CurrentGame.flightState = new FlightState ();
			}
			// Add us back to the active vessel list
			FlightGlobals.Vessels.Add (ourVessel);
			FlightGlobals.ForceSetActiveVessel (ourVessel);
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
	}
}
