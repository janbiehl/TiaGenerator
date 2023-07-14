using System;
using Microsoft.Extensions.Logging;
using TiaGenerator.Core.Services;
using TiaGenerator.Models;

namespace TiaGenerator.Services
{
	public class DataProviderService
	{
		private readonly ILogger<DataProviderService> _logger;
		private readonly Options _options;
		private readonly ISerializer _serializer;

		public DataProviderService(ILogger<DataProviderService> logger, Options options, ISerializer serializer)
		{
			_logger = logger;
			_options = options;
			_serializer = serializer;
		}

		public GeneratorConfiguration? LoadConfiguration()
		{
			try
			{
				return _serializer.Deserialize<GeneratorConfiguration>(_options.DataFilePath);
			}
			catch (Exception e)
			{
				_logger.LogCritical(e, "Failed to load data");
			}

			return null;
		}
	}
}