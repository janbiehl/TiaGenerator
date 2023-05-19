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
		private readonly DataProviderService _dataProvider;
		private readonly IHostApplicationLifetime _applicationLifetime;

		public TiaGeneratorService(ILogger<TiaGeneratorService> logger, DataProviderService dataProvider
			, IHostApplicationLifetime applicationLifetime)
		{
			_logger = logger;
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
					try
					{
						var result = action.Execute(dataStore);

						switch (result.result)
						{
							case ActionResult.Failure:
								_logger.LogError(result.message);
								break;
							case ActionResult.Fatal:
								_logger.LogCritical(result.message);
								break;
							case ActionResult.Success:
								_logger.LogInformation(result.message);
								break;
							default:
								throw new ArgumentOutOfRangeException(nameof(result.result), "The result is unknown");
						}
					}
					catch (ApplicationException e)
					{
						_logger.LogCritical(e, "Could not execute action {Action}", action);
						break; // Leave the loop as we had a fatal error
					}
				}
			}

			_applicationLifetime.StopApplication();
			return Task.CompletedTask;
		}
	}
}