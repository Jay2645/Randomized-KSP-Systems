using RandomizedSystems.SaveGames;
using UnityEngine;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using RandomizedSystems.Systems;

namespace RandomizedSystems.WarpDrivers
{
	[KSPAddon(KSPAddon.Startup.SpaceCentre,false)]
	public class WarpDrive : MonoBehaviour
	{
		public static int seed = 0;

		public static string seedString
		{
			get;
			private set;
		}

		private static string currentSeed = AstroUtils.KERBIN_SYSTEM_COORDS;
		private static string lastSeed = string.Empty;
		private static bool hasInit = false;
		private static Rect windowPosition;
		private static List<OnWarpDelegate> nextWarpActions = new List<OnWarpDelegate> ();

		public static SolarData currentSystem
		{
			get
			{
				return SolarData.currentSystem;
			}
		}

		private const int windowWidth = 150;
		private const int windowHeight = 75;
		/// <summary>
		/// If this is true, we need to purge all "old" vessels from the current system.
		/// </summary>
		public static bool needsPurge = false;

		public delegate void OnWarpDelegate ();

		private void Awake ()
		{
			if (!hasInit)
			{
				hasInit = true;
				// Please let us stay alive, Mr. Garbage Collector
				DontDestroyOnLoad (this);
				// Warp to Kerbin
				Warp (false, AstroUtils.KERBIN_SYSTEM_COORDS);
			}
		}

		/// <summary>
		/// Opens a new window asking a player to enter Hyperspace coordinates.
		/// When the player finishes typing, it will warp to those coordinates and close the window.
		/// </summary>
		public static void OpenWindow ()
		{
			windowPosition = new Rect ((Screen.width / 2) - windowWidth, (Screen.height / 2) - windowHeight, 0, 0);
			RenderingManager.AddToPostDrawQueue (0, OnDraw);
		}

		/// <summary>
		/// Automatically jumps to kerbol.
		/// </summary>
		public static void JumpToKerbol (bool processActions, bool removeVesselFromSystem = true)
		{
			currentSeed = AstroUtils.KERBIN_SYSTEM_COORDS;
			Warp (processActions, currentSeed, false);
			PersistenceGenerator.LoadSnapshotVessels (lastSeed, seedString, removeVesselFromSystem);
		}

		private static void OnDraw ()
		{
			// Use the KSP skin
			GUI.skin = HighLogic.Skin;
			// Open the window
			windowPosition = GUILayout.Window (10, 
			                                   windowPosition, 
			                                   OnWindow, 
			                                   "Enter Hyperspace Coordinates", 
			                                   GUILayout.Height (windowHeight), 
			                                   GUILayout.Width (windowWidth));
		}

		private static void OnWindow (int windowID)
		{
			// Start a new GUILayout
			GUILayout.BeginVertical (GUILayout.Width (250.0f));
			// Create the text field
			GUI.SetNextControlName ("Warp TextField");
			currentSeed = GUILayout.TextField (currentSeed);
			// Select the text in the text field
			GUI.FocusControl ("Warp TextField");
			// Make the button
			if (GUILayout.Button ("Start Warp Drive") || Input.GetKeyDown (KeyCode.Return) || Input.GetKeyDown (KeyCode.KeypadEnter))
			{
				// User has hit the button or pressed enter
				Warp (true, currentSeed, false);
				PersistenceGenerator.LoadSnapshotVessels (lastSeed, seedString, true);
				RenderingManager.RemoveFromPostDrawQueue (0, OnDraw);
			}
			GUILayout.EndVertical ();
			// Allow the window to be draggable
			GUI.DragWindow ();
		}

		/// <summary>
		/// Will set a method or multiple methods to be called next time we warp.
		/// </summary>
		/// <param name="nextWarpAction">A method to be called next time we warp.</param>
		public static void SetNextWarpAction (params OnWarpDelegate[] nextWarpAction)
		{
			// System.Action causes a TypeLoadException
			nextWarpActions.AddRange (nextWarpAction);
		}

		/// <summary>
		/// Warps to the current seed.
		/// </summary>
		public static void Warp (bool processActions, string theSeed, bool savePersistence = true)
		{
			Debugger.Log ("Beginning warp to " + theSeed);
			// Replace any newline or tab characters.
			theSeed = Regex.Replace (theSeed, "[^ -~]+", string.Empty, RegexOptions.Multiline);
			// Make sure the seed is valid
			if (string.IsNullOrEmpty (theSeed))
			{
				ScreenMessages.PostScreenMessage ("Invalid coordinates.", 3.0f, ScreenMessageStyle.UPPER_CENTER);
				return;
			}
			// Set the seeds
			lastSeed = seedString;
			seedString = theSeed;
			try
			{
				// Create the RNG
				Randomizers.WarpRNG.ReSeed (seedString);
				// Create and randomize the system
				SolarData.CreateSystem (seedString, true);
				// Write the current seed to file
				SeedTracker.Jump ();
			}
			catch (System.Exception e)
			{
				// Catch all exceptions so users know if something goes wrong
				ScreenMessages.PostScreenMessage ("Warp Drive failed due to " + e.GetType () + ".");
				Debugger.LogException ("Unable to jump to system!", e);
				return;
			}
			if (seedString != AstroUtils.KERBIN_SYSTEM_COORDS)
			{
				// We've left Kerbol, so we need to purge the Kerbol vessels
				needsPurge = true;
			}
			Debugger.LogWarning ("Created system " + currentSystem.name + " from string " + seedString + ".");
			if (processActions)
			{
				// Call each post-warp action
				foreach (OnWarpDelegate onWarp in nextWarpActions)
				{
					onWarp ();
				}
				// Clear the list of methods
				nextWarpActions.Clear ();
			}
			if (savePersistence)
			{
				PersistenceGenerator.SavePersistence ();
			}
			Debugger.Log ("All post-warp actions done.");
		}
	}
}

