using UnityEngine;
using RandomizedSystems.Persistence;
using System;

namespace RandomizedSystems
{
	public static class HyperdriveWarper
	{
		private static Rect windowPosition;
		public static int seed = 0;

		public static string seedString
		{
			get;
			private set;
		}

		private static string lastSeed = string.Empty;

		public static void OpenWindow ()
		{
			windowPosition = new Rect (100, 100, 0, 0);
			lastSeed = seedString;
			RenderingManager.AddToPostDrawQueue (0, OnDraw);
		}

		private static void OnDraw ()
		{
			windowPosition = GUILayout.Window (10, windowPosition, OnWindow, "Enter Hyperspace Coordinates");
		}

		private static void OnWindow (int windowID)
		{
			GUILayout.BeginVertical (GUILayout.Width (250.0f));
			seedString = GUILayout.TextField (seedString);
			if (GUILayout.Button ("Start Warp Drive") || Input.GetKeyDown (KeyCode.Return) || Input.GetKeyDown (KeyCode.KeypadEnter))
			{
				Warp (true);
				RenderingManager.RemoveFromPostDrawQueue (0, OnDraw);
			}
			GUILayout.EndVertical ();
			GUI.DragWindow ();
		}

		public static void SetSeed (string newSeed)
		{
			lastSeed = seedString;
			seedString = newSeed;
		}

		public static void Warp (bool showMessage, Action onWarp = null)
		{
			SolarData system = null;
			seedString = seedString.Replace ("\n", string.Empty);
			if (string.IsNullOrEmpty (seedString))
			{
				ScreenMessages.PostScreenMessage ("Invalid coordinates.", 3.0f, ScreenMessageStyle.UPPER_CENTER);
				return;
			}
			try
			{
				system = SolarData.CreateSystem (seedString);
				PersistenceGenerator.CreatePersistenceFile (lastSeed, seedString);
				SeedTracker.Jump ();
				if (onWarp != null)
				{
					onWarp ();
				}
			}
			catch (System.Exception e)
			{
				// Catch all exceptions so users know if something goes wrong
				ScreenMessages.PostScreenMessage ("Warp Drive failed due to " + e.GetType () + ".");
				Debugger.LogException ("Unable to jump to system!", e);
				return;
			}
			Debugger.LogWarning ("Created system " + system.name + " from string " + seedString + ".");
			if (showMessage)
			{
				ScreenMessages.PostScreenMessage ("Warp Drive initialized. Traveling to the " + system.name + " system, at coordinates " + seedString + ".", 3.0f, ScreenMessageStyle.UPPER_CENTER);
			}
		}
	}
}

