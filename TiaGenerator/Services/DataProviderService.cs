using System;
using Microsoft.Extensions.Logging;
using TiaGenerator.Core.Models;
using TiaGenerator.Core.Services;

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

		public GeneratorConfiguration? LoadData()
		{
			try
			{
				var data = _serializer.Deserialize<GeneratorConfiguration>(_options.DataFilePath);
				return data;
			}
			catch (Exception e)
			{
				_logger.LogCritical(e, "Failed to load data");
			}

			return null;
		}
	}
}