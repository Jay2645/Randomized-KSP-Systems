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
			ScreenMessages.PostScreenMessage ("An exception has occured involving the warp drive!" +
				"\nPlease press Alt+F2 and copy and paste or send a screenshot of the debugger to the Warp Drive developers!" +
				"\nException Message: " + exception.Message, 10.0f, ScreenMessageStyle.UPPER_CENTER);
		}
	}
}

