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
			CopyPersistenceFileToSystems (persistence, oldSeed);
			if (PersistenceExists (persistence, newSeed))
			{
				string output = Regex.Replace (File.ReadAllText (persistence), "(\t\tVESSEL)((\\s)*(\\S)*)*(\t\t})", string.Empty, RegexOptions.Multiline);
				File.WriteAllText (persistence, output);
				/*File.Delete (persistence);
				CopyPersistenceFileFromSystems (persistence, newSeed);*/
			}
			else
			{
				string output = Regex.Replace (File.ReadAllText (persistence), "(\t\tVESSEL)((\\s)*(\\S)*)*(\t\t})", string.Empty, RegexOptions.Multiline);
				File.WriteAllText (persistence, output);
			}
		}

		public static string FindPersistenceFile ()
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
			return string.Empty;
		}

		public static bool PersistenceExists (string persistence, string prefix)
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
			string persistenceDirectory = Path.GetDirectoryName (persistence);
			persistenceDirectory = Path.Combine (persistenceDirectory, "Star Systems");
			string persistenceFilename = prefix + "_" + Path.GetFileName (persistence);
			string combinedPath = Path.Combine (persistenceDirectory, persistenceFilename);
			if (!File.Exists (persistence))
			{
				File.Copy (combinedPath, persistence);
			}
		}

		public static void CopyPersistenceFileToSystems (string persistence, string prefix)
		{
			string persistenceDirectory = Path.GetDirectoryName (persistence);
			persistenceDirectory = Path.Combine (persistenceDirectory, "Star Systems");
			Directory.CreateDirectory (persistenceDirectory);
			string persistenceFilename = prefix + "_" + Path.GetFileName (persistence);
			string combinedPath = Path.Combine (persistenceDirectory, persistenceFilename);
			if (!File.Exists (combinedPath))
			{
				File.Copy (persistence, combinedPath);
			}
		}
	}
}

