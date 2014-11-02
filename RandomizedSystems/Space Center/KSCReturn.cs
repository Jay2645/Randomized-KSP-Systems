using UnityEngine;
using RandomizedSystems.WarpDrivers;
using RandomizedSystems.Persistence;
using System.IO;

namespace RandomizedSystems.SpaceCenter
{
	[KSPAddon(KSPAddon.Startup.SpaceCentre,false)]
	public class KSCReturn : MonoBehaviour
	{
		private void Awake ()
		{
			if (WarpDrive.seedString != AstroUtils.KERBIN_SYSTEM_COORDS)
			{
				PersistenceGenerator.SaveSnapshot (WarpDrive.seedString);
				PersistenceGenerator.LoadSnapshot (AstroUtils.KERBIN_SYSTEM_COORDS);
			}
		}
	}

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

