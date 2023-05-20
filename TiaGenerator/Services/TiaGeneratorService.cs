using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TiaGenerator.Core.Models;
using TiaGenerator.Models;

namespace TiaGenerator.Services
{
	public class TiaGeneratorService : BackgroundService
	{
		private readonly ILogger<TiaGeneratorService> _logger;
		private readonly Options _options;
		private readonly DataProviderService _dataProvider;
		private readonly IHostApplicationLifetime _applicationLifetime;

		public TiaGeneratorService(ILogger<TiaGeneratorService> logger, Options options, DataProviderService dataProvider
			, IHostApplicationLifetime applicationLifetime)
		{
			_logger = logger;
			_options = options;
			_dataProvider = dataProvider;
			_applicationLifetime = applicationLifetime;
		}

		/// <inheritdoc />
		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
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
			}

			_logger.LogDebug("Data loaded {@Data}", data);

			using var dataStore = new DataStore();

			if (data?.Actions != null)
			{
				foreach (var action in data.Actions)
				{
					if (stoppingToken.IsCancellationRequested)
					{
						_logger.LogInformation("Cancellation requested. Stopping.");
						break;
					}
					
					try
					{
						var result = await action.Execute(dataStore);

						switch (result.Result)
						{
							case ActionResultType.Failure:
								_logger.LogError(result.Message);
								break;
							case ActionResultType.Fatal:
								_logger.LogCritical(result.Message);
								break;
							case ActionResultType.Success:
								_logger.LogInformation(result.Message);
								break;
							default:
								throw new ArgumentOutOfRangeException(nameof(result.Result), "The result is unknown");
						}
					}
					catch (ApplicationException e)
					{
						_logger.LogCritical(e, "Could not execute action {Action}", action);
						break; // Leave the loop as we had a fatal error
					}
				}
			}

			if (_options.Cleanup)
				FileManager.Cleanup();
			
			_applicationLifetime.StopApplication();
		}
	}
}