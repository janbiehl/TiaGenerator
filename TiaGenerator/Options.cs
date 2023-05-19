using CommandLine;

// ReSharper disable UnusedType.Global
// ReSharper disable UnusedMember.Global

namespace TiaGenerator
{
	public class Options
	{
		[Option('v', "verbose", Required = false, HelpText = "Set output to verbose messages.")]
		public bool Verbose { get; set; }

		[Option('i', "input", Required = true, HelpText = "Input file to be processed.")]
		public string DataFilePath { get; set; } = null!;
	}
}