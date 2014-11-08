using System.IO;
using System.Collections.Generic;
using RandomizedSystems.WarpDrivers;

namespace RandomizedSystems.SaveGames
{
	public static class SeedTracker
	{
		private static string addonPath = string.Empty;

		public static void Jump ()
		{
			CreateConfig (WarpDrive.seedString);
		}

		public static string LastSeed (string saveFolder)
		{
			string lastSeed = GetSeedContents (saveFolder);
			if (string.IsNullOrEmpty (lastSeed))
			{
				Debugger.LogError (saveFolder + " had an empty seed!");
				return string.Empty;
			}
			return lastSeed;
		}

		private static void FindConfig ()
		{
			try
			{
				string appPath = KSPUtil.ApplicationRootPath;
				string addonFolder = "";
				if (Directory.Exists (appPath))
				{
					string[] allDirectories = Directory.GetDirectories (appPath);
					foreach (string directory in allDirectories)
					{
						if (Path.GetFileName (directory).ToLower () == "gamedata")
						{
							addonFolder = directory;
						}
					}
				}
				addonFolder = Path.Combine (addonFolder, "RandomizedSystems");
			}
			catch (IOException e)
			{
				Debugger.LogException ("Could not load config file!", e);
			}
		}

		private static void CreateConfig (string seed)
		{
			if (string.IsNullOrEmpty (addonPath))
			{
				FindConfig ();
			}
			string saveFolder = HighLogic.SaveFolder;
			string cfgFile = Path.Combine (addonPath, HighLogic.SaveFolder + ".seed");
			if (!File.Exists (cfgFile))
			{
				File.Create (cfgFile);
			}
			File.WriteAllText (cfgFile, seed);
		}

		private static string GetSeedContents (string saveFolderName)
		{
			if (string.IsNullOrEmpty (addonPath))
			{
				FindConfig ();
			}
			string[] files = Directory.GetFiles (addonPath);
			foreach (string file in files)
			{
				if (Path.GetExtension (file) == "seed" && Path.GetFileNameWithoutExtension (file) == saveFolderName)
				{
					return File.ReadAllText (file);
				}
			}
			return string.Empty;
		}
	}
}

