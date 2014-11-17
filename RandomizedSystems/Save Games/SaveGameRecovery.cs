using UnityEngine;
using System.IO;

namespace RandomizedSystems.SaveGames
{
	[KSPAddon(KSPAddon.Startup.MainMenu, false)]
	public class SaveGameRecoveryMainMenu : MonoBehaviour
	{
		protected void Awake ()
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
				if (string.IsNullOrEmpty (appPath))
				{
					return;
				}
				foreach (string directory in Directory.GetDirectories(saveFolder))
				{
					string thisSave = Path.GetFileName (directory);
					string lastSeed = SeedTracker.LastSeed (thisSave);
					if (lastSeed == AstroUtils.KERBIN_SYSTEM_COORDS)
					{
						// Everything is okay!
						continue;
					}
					// We're in trouble here
					// Look in each save folder for a persistence file
					string persistence = Path.Combine (directory, AstroUtils.DEFAULT_PERSISTENCE + AstroUtils.SFS);
					string systemFolder = Path.Combine (directory, AstroUtils.STAR_SYSTEM_FOLDER_NAME);
					// Look for the Kerbin save
					string kerbinSave = AstroUtils.KERBIN_SYSTEM_COORDS + AstroUtils.SEED_PERSISTENCE + AstroUtils.SFS;
					string stockSaveGame = Path.Combine (systemFolder, kerbinSave);
					if (File.Exists (stockSaveGame))
					{
						// Found it!
						if (string.IsNullOrEmpty (lastSeed))
						{
							Debugger.LogWarning ("Could not save old persistence file because the last seed was null.");
						}
						else
						{
							// Copy over the current persistence file to the snapshot directory
							string oldSave = lastSeed + AstroUtils.SEED_PERSISTENCE + AstroUtils.SFS;
							string seedPath = Path.Combine (systemFolder, oldSave);
							File.WriteAllBytes (seedPath, File.ReadAllBytes (persistence));
						}
						File.WriteAllBytes (persistence, File.ReadAllBytes (stockSaveGame));
						SeedTracker.CreateConfig (thisSave, AstroUtils.KERBIN_SYSTEM_COORDS);
						continue;
					}
					// Nothing we can do. We'll have to keep the default persistence file if we have one.
				}
			}
			catch (IOException e)
			{
				Debugger.LogException ("Unable to recover save games!", e);
			}
		}
	}
}

