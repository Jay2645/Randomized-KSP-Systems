using UnityEngine;

namespace RandomizedSystems
{
	public static class Debugger
	{
		public static void Log (object message)
		{
			Debug.Log ("**Warp Drive** " + message);
		}

		public static void Log (object message, string methodName)
		{
			Log (methodName + ": " + message);
		}

		public static void LogWarning (object message)
		{
			Debug.LogWarning ("**Warp Drive** " + message);
		}

		public static void LogWarning (object message, string methodName)
		{
			LogWarning (methodName + ": " + message);
		}

		public static void LogError (object message)
		{
			Debug.LogError ("**Warp Drive** " + message);
		}

		public static void LogError (object message, string methodName)
		{
			LogError (methodName + ": " + message);
		}

		public static void LogException (object message, System.Exception exception)
		{
			LogError ("Exception! " + message + " Exception data: " + exception.Message + "," + exception.StackTrace);
		}
	}
}

