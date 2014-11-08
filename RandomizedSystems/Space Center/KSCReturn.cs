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
				HighLogic.CurrentGame = GamePersistence.LoadGame (AstroUtils.KERBIN_SYSTEM_COORDS + "_persistent", 
				                                                  Path.Combine (HighLogic.SaveFolder, "Star Systems"),
				                                                  true,
				                                                  false);
				HighLogic.CurrentGame.startScene = GameScenes.FLIGHT;
			}
		}
	}
}

