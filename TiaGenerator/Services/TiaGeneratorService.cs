using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TiaGenerator.Core.Interfaces;

namespace TiaGenerator.Services
{
	public class DataStore : IDataStore
	{
		public const string TiaPortalKey = "TiaPortal";
		public const string TiaProjectKey = "TiaProject";

		private readonly Dictionary<string, object> _data = new();

		public void SetValue<T>(string name, T value) where T : new()
		{
			_data[name] = value ?? throw new ArgumentNullException(nameof(value));
		}

		/// <inheritdoc />
		public T? GetValue<T>(string name) where T : new()
		{
			if (_data.TryGetValue(name, out var value))
				return (T?) value;

			return default;
		}
	}

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
			var data = _dataProvider.LoadData();

			if (data is null)
			{
				_logger.LogError("No data loaded.");
				return Task.CompletedTask;
			}

			_logger.LogDebug("Data loaded {Data}", data);

			var dataStore = new DataStore();

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