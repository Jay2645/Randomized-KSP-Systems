using System.IO;
using System.Text.RegularExpressions;
using RandomizedSystems.WarpDrivers;

namespace RandomizedSystems.SaveGames
{
	public static class SeedTracker
	{
		private static string addonPath = string.Empty;

		public static void Jump ()
		{
			CreateConfig (HighLogic.SaveFolder, WarpDrive.seedString);
		}

		public static string LastSeed (string saveFolder)
		{
			string lastSeed = GetSeedContents (saveFolder);
			if (string.IsNullOrEmpty (lastSeed))
			{
				Debugger.LogError (saveFolder + " had an empty seed!");
				return string.Empty;
			}
			// Replace any newline or tab characters.
			lastSeed = Regex.Replace (lastSeed, "[^ -~]+", string.Empty, RegexOptions.Multiline);
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
				addonPath = Path.Combine (addonFolder, "RandomizedSystems");
			}
			catch (IOException e)
			{
				Debugger.LogException ("Could not load config file!", e);
			}
		}

		public static void CreateConfig (string saveFolder, string seed)
		{
			if (string.IsNullOrEmpty (addonPath))
			{
				FindConfig ();
			}
			string cfgFile = Path.Combine (addonPath, saveFolder + ".seed");
			try
			{
				File.WriteAllText (cfgFile, seed);
			}
			catch (IOException e)
			{
				Debugger.LogException ("Could not write " + seed + " to config file!", e);
			}
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
				if (Path.GetExtension (file) != ".seed")
				{
					continue;
				}
				string fileName = Path.GetFileNameWithoutExtension (file);
				if (fileName == saveFolderName)
				{
					return File.ReadAllText (file);
				}
			}
			return string.Empty;
		}
	}
}

