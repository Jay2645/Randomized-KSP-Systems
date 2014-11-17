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
			GameEvents.onVesselSituationChange.Add (SaveGame);
			GameEvents.onCrash.Add (SaveGame);
			GameEvents.onCrashSplashdown.Add (SaveGame);
			GameEvents.onCrewOnEva.Add (SaveGame);
			GameEvents.onGameSceneLoadRequested.Add (SaveGame);
		}

		private float lastSaveTime = 0.0f;
		private const float MIN_TIME_BETWEEN_SAVES = 10.0f;

		private void SaveGame ()
		{
			if (Time.time - lastSaveTime < MIN_TIME_BETWEEN_SAVES)
			{
				return;
			}
			lastSaveTime = Time.time;
			PersistenceGenerator.SavePersistence ();
			PersistenceGenerator.SaveSnapshot (WarpDrive.seedString);
		}

		private void SaveGame (GameEvents.FromToAction<Part, Part> eva)
		{
			SaveGame ();
		}

		private void SaveGame (EventReport report)
		{
			SaveGame ();
		}

		private void SaveGame (GameEvents.HostedFromToAction<Vessel, Vessel.Situations> vesselSituation)
		{
			Vessel.Situations situation = vesselSituation.to;
			if (situation == Vessel.Situations.LANDED)
			{
				SaveGame ();
			}
		}

		private void SaveGame (GameScenes scene)
		{
			SaveGame ();
		}
	}
}

