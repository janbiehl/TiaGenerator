using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TiaGenerator.Models;

namespace TiaGenerator.Services
{
	public class TiaGeneratorService : BackgroundService
	{
		private readonly ILogger<TiaGeneratorService> _logger;
		private readonly Options _options;
		private readonly DataProviderService _dataProvider;
		private readonly IHostApplicationLifetime _applicationLifetime;

		public TiaGeneratorService(ILogger<TiaGeneratorService> logger, Options options,
			DataProviderService dataProvider, IHostApplicationLifetime applicationLifetime)
		{
			_logger = logger;
			_options = options;
			_dataProvider = dataProvider;
			_applicationLifetime = applicationLifetime;
		}

		/// <inheritdoc />
		protected override Task ExecuteAsync(CancellationToken stoppingToken)
		{
			// TODO: Activate the following
			// Api.Global.Openness().Initialize(tiaMajorVersion: 17);
			//
			// if (!Api.Global.Openness().IsUserInGroup())
			// {
			// 	Console.WriteLine("The user is not in the group 'TIA_Portal_Developer'.");
			// 	return Task.CompletedTask;
			// }

			var data = _dataProvider.LoadData();

			if (data is null)
			{
				_logger.LogError("No data loaded.");
				return Task.CompletedTask;
			}

			_logger.LogDebug("Data loaded {Data}", data);

			using var dataStore = new DataStore();

			if (data.Actions != null)
			{
				foreach (var action in data.Actions)
				{
					action.Execute(dataStore);
				}
			}

			_applicationLifetime.StopApplication();
			return Task.CompletedTask;
		}
	}
}