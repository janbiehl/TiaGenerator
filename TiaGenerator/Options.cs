using CommandLine;

// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable UnusedType.Global
// ReSharper disable UnusedMember.Global

namespace TiaGenerator
{
	/// <summary>
	/// The commandline options for this application
	/// </summary>
	public class Options
	{
		/// <summary>
		/// When true the application will log verbose messages
		/// </summary>
		[Option('v', "verbose", Required = false, HelpText = "Set output to verbose messages.")]
		public bool Verbose { get; set; }

		/// <summary>
		/// The path to the generator data file
		/// </summary>
		[Option('i', "input", Required = true, HelpText = "Input file to be processed.")]
		public string DataFilePath { get; set; } = null!;

		/// <summary>
		/// When true the generator will cleanup the temporary files
		/// </summary>
		[Option('c', "cleanup", Required = false, Default = true, HelpText = "Cleanup temporary files.")]
		public bool Cleanup { get; set; }

		/// <summary>
		/// This endpoint will be used for OpenTelemetry tracing
		/// </summary>
		[Option('o', "openTelemetry", Required = false, Default = null,
			HelpText = "OpenTelemetry endpoint. Example: http://localhost:4317/v1/trace")]
		public string? OpenTelemetryEndpoint { get; set; }
	}
}