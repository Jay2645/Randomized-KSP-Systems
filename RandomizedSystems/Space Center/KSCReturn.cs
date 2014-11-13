using UnityEngine;
using RandomizedSystems.WarpDrivers;
using RandomizedSystems.SaveGames;
using System.IO;

namespace RandomizedSystems.SpaceCenter
{
	[KSPAddon(KSPAddon.Startup.TrackingStation,false)]
	public class UniqueIDEnforcer : MonoBehaviour
	{
		private void Start ()
		{
			PersistenceGenerator.SavePersistence ();
		}
	}

	[KSPAddon(KSPAddon.Startup.Flight,false)]
	public class RevertFlightReturn : MonoBehaviour
	{
		private void Awake ()
		{
			if (!FlightDriver.flightStarted && WarpDrive.seedString != AstroUtils.KERBIN_SYSTEM_COORDS)
			{
				HighLogic.CurrentGame = GamePersistence.LoadGame (AstroUtils.KERBIN_SYSTEM_COORDS + AstroUtils.SEED_PERSISTENCE, 
				                                                  Path.Combine (HighLogic.SaveFolder, AstroUtils.STAR_SYSTEM_FOLDER_NAME),
				                                                  true,
				                                                  false);
				HighLogic.CurrentGame.startScene = GameScenes.FLIGHT;
			}
		}
	}
}

