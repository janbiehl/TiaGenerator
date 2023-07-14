using System.Diagnostics;

namespace TiaGenerator
{
	public static class Tracing
	{
		internal const string InstrumentationName = "TiaGenerator";
		internal const string InstrumentationVersion = "0.0.1";

		public static readonly ActivitySource ActivitySource = new(InstrumentationName, InstrumentationVersion);
	}
}