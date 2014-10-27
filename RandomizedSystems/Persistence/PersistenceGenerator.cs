using System.IO;
using System.Text.RegularExpressions;

namespace RandomizedSystems.Persistence
{
	/// <summary>
	/// This handles modifying the persistence files of a system.
	/// </summary>
	public static class PersistenceGenerator
	{
		public static void CreatePersistenceFile (string oldSeed, string newSeed)
		{
			string persistence = FindPersistenceFile ();
			if (string.IsNullOrEmpty (persistence))
			{
				Debugger.LogError ("Could not find persistence file!");
				return;
			}
			// Copy over our current persistence file to the solar systems
			// We now have no "live" file
			CopyPersistenceFileToSystems (persistence, oldSeed);
			try
			{
				File.Delete (persistence);
			}
			catch (IOException e)
			{
				Debugger.LogException ("Could not delete persistence file!", e);
				return;
			}
			if (SystemPersistenceExists (persistence, newSeed))
			{
				// If we have a persistence file for this system, copy it to our current persistence file
				CopyPersistenceFileFromSystems (persistence, newSeed);
			}
			else
			{
				// If we don't have a persistence file for this system, strip our current persistence file and use that
				/*string output = Regex.Replace (File.ReadAllText (persistence), "(\t\tVESSEL)(.)*?(\n\t\t})", string.Empty, RegexOptions.Multiline);
				File.WriteAllText (persistence, output);
				CopyPersistenceFileToSystems (persistence, newSeed, true);*/
			}
		}

		public static string FindPersistenceFile ()
		{
			try
			{
				string appPath = KSPUtil.ApplicationRootPath;
				string saveFolder = "";
				while (!string.IsNullOrEmpty(appPath) && saveFolder == "")
				{
					if (Directory.Exists (appPath))
					{
						string[] allDirectories = Directory.GetDirectories (appPath);
						foreach (string directory in allDirectories)
						{
							if (Path.GetFileName (directory).ToLower () == "saves")
							{
								saveFolder = directory;
							}
						}
					}
					if (saveFolder == "")
					{
						// Shorten the path name
						appPath = Path.GetDirectoryName (appPath);
					}
				}
				saveFolder = Path.Combine (saveFolder, HighLogic.SaveFolder);
				if (Directory.Exists (saveFolder))
				{
					string persistence = Path.Combine (saveFolder, "persistent.sfs");
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
				// Make sure persistence does not exist
				if (File.Exists (persistence))
				{
					File.Delete (persistence);
				}
				// Copy over the filepath found in systems
				File.Copy (combinedPath, persistence);
				// Create the live file
				string livePath = Path.Combine (combinedPath, "Live");
				Directory.CreateDirectory (livePath);
				livePath = Path.Combine (livePath, persistenceFilename);
				// Get rid of anything in there
				if (File.Exists (livePath))
				{
					File.Delete (livePath);
				}
				// Copy over the live file
				File.Copy (combinedPath, livePath);
				// Delete the live file from the cached folder
				File.Delete (combinedPath);
			}
			catch (IOException e)
			{
				Debugger.LogException ("Could not copy from Star Systems!", e);
			}
		}

		public static void CopyPersistenceFileToSystems (string persistence, string prefix)
		{
			CopyPersistenceFileToSystems (persistence, prefix, false);
		}

		public static void CopyPersistenceFileToSystems (string persistence, string prefix, bool live)
		{
			try
			{
				string persistenceDirectory = Path.GetDirectoryName (persistence);
				persistenceDirectory = Path.Combine (persistenceDirectory, "Star Systems");
				Directory.CreateDirectory (persistenceDirectory);
				string persistenceFilename = prefix + "_" + Path.GetFileName (persistence);
				string combinedPath = Path.Combine (persistenceDirectory, persistenceFilename);
				if (File.Exists (combinedPath))
				{
					File.Delete (combinedPath);
				}
				File.Copy (persistence, combinedPath);
				// Delete the directory and all live files in it, since they are no longer live
				string livePath = Path.Combine (combinedPath, "Live");
				if (Directory.Exists (livePath))
				{
					Directory.Delete (livePath);
				}
				if (live)
				{
					//...Unless the current persistence file is actually live
					Directory.CreateDirectory (livePath);
					livePath = Path.Combine (livePath, persistenceFilename);
					if (File.Exists (livePath))
					{
						File.Delete (livePath);
					}
					File.Copy (persistence, livePath);
				}
			}
			catch (IOException e)
			{
				Debugger.LogException ("Could not copy to Star Systems!", e);
			}
		}
	}
}

