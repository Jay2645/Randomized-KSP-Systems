using UnityEngine;
using System.IO;

namespace RandomizedSystems.Persistence
{
	[KSPAddon(KSPAddon.Startup.SpaceCentre, false)]
	public class SaveGameRecoveryKSC : MonoBehaviour
	{
		protected void Awake ()
		{
			string persistenceFilePath = PersistenceGenerator.FindPersistenceFile ();
			if (PersistenceGenerator.SystemPersistenceExists (persistenceFilePath, AstroUtils.KERBIN_SYSTEM_COORDS))
			{
				// Did not clean up properly last time we jumped
				string lastSeed = SeedTracker.LastSeed ();
				if (!string.IsNullOrEmpty (lastSeed) && lastSeed != AstroUtils.KERBIN_SYSTEM_COORDS)
				{
					//PersistenceGenerator.CreatePersistenceFile (lastSeed, AstroUtils.KERBIN_SYSTEM_COORDS);
				}
			}
		}
	}

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
					// Look in each save folder for a persistence file
					string persistence = Path.Combine (directory, "persistent.sfs");
					if (File.Exists (persistence))
					{
						// Everything is okay!
						continue;
					}
					// We're in trouble here
					string systemFolder = Path.Combine (directory, "Star Systems");
					// Look for the Kerbin save
					string kerbinSave = AstroUtils.KERBIN_SYSTEM_COORDS + "_persistent.sfs";
					string stockSaveGame = Path.Combine (systemFolder, kerbinSave);
					if (File.Exists (stockSaveGame))
					{
						// Found it!
						File.WriteAllBytes (persistence, File.ReadAllBytes (stockSaveGame));
						continue;
					}
					// Really in trouble now
					string liveFolder = Path.Combine (systemFolder, "Live");
					if (Directory.Exists (liveFolder))
					{
						string liveSaveGame = Path.Combine (liveFolder, kerbinSave);
						if (File.Exists (liveSaveGame))
						{
							// Yay!
							File.WriteAllBytes (persistence, File.ReadAllBytes (liveSaveGame));
							continue;
						}
					}
					// Nothing we can do; save data has been lost. :(
					Debugger.LogError ("Lost save data for directory " + directory + ". :(");
				}
			}
			catch (IOException e)
			{
				Debugger.LogException ("Unable to recover save games!", e);
			}
		}
	}
}

