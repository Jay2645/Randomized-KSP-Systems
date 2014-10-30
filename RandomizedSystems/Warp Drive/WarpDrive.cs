using RandomizedSystems.Persistence;
using UnityEngine;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace RandomizedSystems.WarpDrivers
{
	public static class WarpDrive
	{
		public static int seed = 0;
		public static string seedString = AstroUtils.KERBIN_SYSTEM_COORDS;
		private static string currentSeed = AstroUtils.KERBIN_SYSTEM_COORDS;
		private static string lastSeed = string.Empty;
		private static bool hasInit = false;
		private static bool doneInit = false;
		private static Rect windowPosition;
		private static List<OnWarpDelegate> nextWarpActions = new List<OnWarpDelegate> ();
		public static SolarData currentSystem;
		private const int windowWidth = 150;
		private const int windowHeight = 75;

		public delegate void OnWarpDelegate ();

		public static void OpenWindow ()
		{
			windowPosition = new Rect ((Screen.width / 2) - windowWidth, (Screen.height / 2) - windowHeight, 0, 0);
			RenderingManager.AddToPostDrawQueue (0, OnDraw);
		}

		/// <summary>
		/// Automatically jumps to kerbol.
		/// </summary>
		public static void JumpToKerbol ()
		{
			currentSeed = AstroUtils.KERBIN_SYSTEM_COORDS;
			Warp ();
		}

		private static void OnDraw ()
		{
			GUI.skin = HighLogic.Skin;
			windowPosition = GUILayout.Window (10, 
			                                   windowPosition, 
			                                   OnWindow, 
			                                   "Enter Hyperspace Coordinates", 
			                                   GUILayout.Height (windowHeight), 
			                                   GUILayout.Width (windowWidth));
		}

		private static void OnWindow (int windowID)
		{
			GUILayout.BeginVertical (GUILayout.Width (250.0f));
			GUI.SetNextControlName ("Warp TextField");
			currentSeed = GUILayout.TextField (currentSeed);
			GUI.FocusControl ("Warp TextField");
			if (GUILayout.Button ("Start Warp Drive") || Input.GetKeyDown (KeyCode.Return) || Input.GetKeyDown (KeyCode.KeypadEnter))
			{
				Warp ();
				RenderingManager.RemoveFromPostDrawQueue (0, OnDraw);
			}
			GUILayout.EndVertical ();
			GUI.DragWindow ();
		}

		public static void SetNextWarpAction (params OnWarpDelegate[] nextWarpAction)
		{
			// System.Action causes a TypeLoadException
			nextWarpActions.AddRange (nextWarpAction);
		}

		public static void Warp ()
		{
			if (!hasInit)
			{
				string tempSeed = currentSeed;
				currentSeed = AstroUtils.KERBIN_SYSTEM_COORDS;
				hasInit = true;
				Warp ();
				currentSeed = tempSeed;
				doneInit = true;
			}
			currentSeed = Regex.Replace (currentSeed, "[^ -~]+", string.Empty, RegexOptions.Multiline);
			if (string.IsNullOrEmpty (currentSeed))
			{
				ScreenMessages.PostScreenMessage ("Invalid coordinates.", 3.0f, ScreenMessageStyle.UPPER_CENTER);
				return;
			}
			lastSeed = seedString;
			seedString = currentSeed;
			try
			{
				Randomizers.WarpRNG.ReSeed (seedString);
				currentSystem = SolarData.CreateSystem (seedString);
				PersistenceGenerator.CreatePersistenceFile (lastSeed, seedString);
				SeedTracker.Jump ();
			}
			catch (System.Exception e)
			{
				// Catch all exceptions so users know if something goes wrong
				ScreenMessages.PostScreenMessage ("Warp Drive failed due to " + e.GetType () + ".");
				Debugger.LogException ("Unable to jump to system!", e);
				return;
			}
			Debugger.LogWarning ("Created system " + currentSystem.name + " from string " + seedString + ".");
			if (doneInit)
			{
				foreach (OnWarpDelegate onWarp in nextWarpActions)
				{
					onWarp ();
				}
				nextWarpActions.Clear ();
			}
		}
	}
}

