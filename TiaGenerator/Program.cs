using System;
using System.Threading.Tasks;
using CommandLine;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using TiaGenerator.Core.Services;
using TiaGenerator.Services;
using ISerializer = TiaGenerator.Core.Services.ISerializer;

namespace TiaGenerator
{
	internal abstract class Program
	{
		private static Options Options { get; set; } = null!;

		public static async Task Main(string[] args)
		{
			var start = DateTime.Now;
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

			// Create the logger
			Log.Logger = new LoggerConfiguration()
				.MinimumLevel.Information()

#if DEBUG
				.MinimumLevel.Debug()	
#endif
				
				.WriteTo.File("log.txt", rollingInterval: RollingInterval.Day)
				.WriteTo.Console()
				.CreateLogger();

			// Create the application builder
			var builder = Host.CreateApplicationBuilder(args);
			
			// Clear the default log provider
			builder.Logging.ClearProviders();

			builder.Logging.AddSerilog(Log.Logger, true);

			// Register services
			RegisterServices(builder.Services);

			using var host =  builder.Build();
			
			await host.RunAsync();
			
			var end = DateTime.Now;
			
			Console.WriteLine($"Duration: {end - start}");
		}

		private static void RegisterServices(IServiceCollection services)
		{
			services.AddSingleton(Options);
			services.AddSingleton<ISerializer, YamlSerializer>();
			services.AddSingleton<DataProviderService>();
			services.AddHostedService<TiaGeneratorService>();
		}
	}
}