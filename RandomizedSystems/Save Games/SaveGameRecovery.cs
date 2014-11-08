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
					Debugger.LogError ("Could not find save folder!");
					return;
				}
				foreach (string directory in Directory.GetDirectories(saveFolder))
				{
					string thisSave = Path.GetDirectoryName (saveFolder);
					string lastSeed = SeedTracker.LastSeed (thisSave);
					if (lastSeed == AstroUtils.KERBIN_SYSTEM_COORDS)
					{
						// Everything is okay!
						continue;
					}
					Debugger.LogWarning ("Attempting recovery for save folder " + thisSave);
					// We're in trouble here
					// Look in each save folder for a persistence file
					string persistence = Path.Combine (directory, "persistent.sfs");
					string systemFolder = Path.Combine (directory, "Star Systems");
					// Look for the Kerbin save
					string kerbinSave = AstroUtils.KERBIN_SYSTEM_COORDS + "_persistent.sfs";
					string stockSaveGame = Path.Combine (systemFolder, kerbinSave);
					if (File.Exists (stockSaveGame))
					{
						// Found it!
						Debugger.Log ("Found Kerbol persistence file!");
						File.WriteAllBytes (persistence, File.ReadAllBytes (stockSaveGame));
						continue;
					}
					// Really in trouble now
					/*string liveFolder = Path.Combine (systemFolder, "Live");
					if (Directory.Exists (liveFolder))
					{
						string liveSaveGame = Path.Combine (liveFolder, kerbinSave);
						if (File.Exists (liveSaveGame))
						{
							// Yay!
							File.WriteAllBytes (persistence, File.ReadAllBytes (liveSaveGame));
							continue;
						}
					}*/
					// Nothing we can do. We'll have to keep the default persistence file if we have one.
					Debugger.LogWarning ("Going to keep default persistence for " + directory + ".");
				}
			}
			catch (IOException e)
			{
				Debugger.LogException ("Unable to recover save games!", e);
			}
		}
	}
}

