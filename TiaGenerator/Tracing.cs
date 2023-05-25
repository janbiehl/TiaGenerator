using System.Diagnostics;

namespace TiaGenerator
{
	public class Tracing
	{
		internal const string InstrumentationName = "TiaGenerator";
		internal const string InstrumentationVersion = "0.0.1";

		public static ActivitySource ActivitySource = new(InstrumentationName, InstrumentationVersion);
	}
}