using UnityEngine;
using System.IO;

namespace RandomizedSystems.Persistence
{
	[KSPAddon(KSPAddon.Startup.SpaceCentre, false)]
	public class SaveGameRecovery : MonoBehaviour
	{
		protected void Awake ()
		{
			string persistenceFilePath = PersistenceGenerator.FindPersistenceFile ();
			if (PersistenceGenerator.PersistenceExists (persistenceFilePath, AstroUtils.KERBIN_SYSTEM_COORDS))
			{
				// Did not clean up properly last time we jumped
				string lastSeed = SeedTracker.LastSeed ();
				if (!string.IsNullOrEmpty (lastSeed))
				{
					PersistenceGenerator.CreatePersistenceFile (lastSeed, AstroUtils.KERBIN_SYSTEM_COORDS);
				}
			}
		}
	}
}

