using CommandLine;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TiaGenerator.Core.Services;
using TiaGenerator.Services;
using ISerializer = TiaGenerator.Core.Services.ISerializer;

namespace TiaGenerator
{
	internal abstract class Program
	{
		private static Options Options { get; set; } = null!;

		public static void Main(string[] args)
		{
			var invalidOptions = false;

			// Parse command line options
			Parser.Default
				.ParseArguments<Options>(args)
				.WithParsed(options =>
				{
					if (options is null)
						invalidOptions = true;
					else
						Options = options;
				});

			// Ensure that options are set
			if (invalidOptions)
			{
				return;
			}

			var builder = Host.CreateDefaultBuilder(args);

			// Configure the logging
			builder.ConfigureLogging(loggingBuilder =>
			{
				loggingBuilder.AddSimpleConsole(options =>
				{
					options.SingleLine = true;
					options.TimestampFormat = "[HH:mm:ss] ";
				});

				loggingBuilder.SetMinimumLevel(Options.Verbose ? LogLevel.Debug : LogLevel.Information);

#if DEBUG
				loggingBuilder.SetMinimumLevel(LogLevel.Debug);
#endif
			});

			// Configure our services
			builder.ConfigureServices(services =>
			{
				services.AddSingleton(Options);
				services.AddSingleton<ISerializer, YamlSerializer>();
				services.AddSingleton<DataProviderService>();
				services.AddHostedService<TiaGeneratorService>();
			});

			using var host = builder.Build();

			host.Run();
		}
	}
}