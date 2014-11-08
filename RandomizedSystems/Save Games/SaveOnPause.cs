using UnityEngine;
using RandomizedSystems.WarpDrivers;

namespace RandomizedSystems.SaveGames
{
	[KSPAddon(KSPAddon.Startup.Flight,false)]
	public class SaveOnPause : MonoBehaviour
	{
		private void Awake ()
		{
			GameEvents.onGamePause.Add (SaveGame);
			GameEvents.onGameSceneLoadRequested.Add (SaveGame);
		}

		private void SaveGame ()
		{
			PersistenceGenerator.SavePersistence ();
			PersistenceGenerator.SaveSnapshot (WarpDrive.seedString);
		}

		private void SaveGame (GameScenes scene)
		{
			SaveGame ();
		}
	}
}

