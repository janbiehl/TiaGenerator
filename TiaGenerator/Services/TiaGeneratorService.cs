using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TiaGenerator.Core.Models;
using TiaGenerator.Models;

namespace TiaGenerator.Services
{
	internal sealed class TiaGeneratorService : BackgroundService
	{
		private readonly ILogger<TiaGeneratorService> _logger;
		private readonly Options _options;
		private readonly DataProviderService _dataProvider;
		private readonly IHostApplicationLifetime _applicationLifetime;

		public TiaGeneratorService(ILogger<TiaGeneratorService> logger, Options options,
			DataProviderService dataProvider
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

			var configuration = _dataProvider.LoadConfiguration();

			if (configuration is null)
			{
				_logger.LogError("No Configuration loaded.");
			}
			else
			{
				_logger.LogDebug("Configuration loaded {@Data}", configuration);

				await ExecuteActions(configuration.Actions, stoppingToken);

				// Cleanup any temporary files or directories
				if (_options.Cleanup)
					await FileManager.Cleanup(stoppingToken);
			}

			_applicationLifetime.StopApplication();
		}

		/// <summary>
		/// Execute any actions that are read from configuration
		/// </summary>
		/// <param name="actions">Actions that will be used</param>
		/// <param name="cancellationToken">Will cancel the operation while it is running</param>
		/// <exception cref="ArgumentOutOfRangeException"></exception>
		private async Task ExecuteActions(IEnumerable<GeneratorAction> actions, CancellationToken cancellationToken)
		{
			// The datastore is used to store data between actions
			using var dataStore = new DataStore();

			foreach (var action in actions)
			{
				if (cancellationToken.IsCancellationRequested)
				{
					_logger.LogInformation("Cancellation requested. Stopping.");
					cancellationToken.ThrowIfCancellationRequested();
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
				catch (Exception e)
				{
					_logger.LogCritical(e, "Could not execute action {Action}", action);
					break; // Leave the loop as we had a fatal error
				}
			}
		}
	}
}