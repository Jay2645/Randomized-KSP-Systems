using System;
using System.Collections;

namespace RandomizedSystems
{
	public class PlanetRandomizerException : Exception
	{
		public PlanetRandomizerException ()
		{
		}

		public PlanetRandomizerException (string message)
			: base(message)
		{
		}

		public PlanetRandomizerException (string message, Exception inner)
			: base(message, inner)
		{
		}
	}
}

