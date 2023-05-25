using System;
using System.IO;
using System.Threading.Tasks;
using OpenTelemetry.Trace;
using Serilog;
using TiaGenerator.Core.Interfaces;
using TiaGenerator.Core.Models;
using TiaGenerator.Models;
using TiaGenerator.Tia.Extensions;
using ILogger = Serilog.ILogger;

namespace TiaGenerator.Actions
{
	public class ExportBlockAction : GeneratorAction
	{
		private readonly ILogger _logger;

		public string? BlockName { get; set; }
		public string? FilePath { get; set; }

		public ExportBlockAction()
		{
			_logger = Log.Logger;
		}

		/// <inheritdoc />
		public override Task<ActionResult> Execute(IDataStore datastore)
		{
			using var activity = Tracing.ActivitySource.StartActivity(nameof(ExportBlockAction));

			activity?.SetTag(nameof(BlockName), BlockName);
			activity?.SetTag(nameof(FilePath), FilePath);

			if (datastore is not DataStore dataStore)
				throw new InvalidOperationException("Invalid datastore");

			if (string.IsNullOrWhiteSpace(BlockName))
				throw new InvalidOperationException("Block name is not set properly");

			if (string.IsNullOrWhiteSpace(FilePath))
				throw new InvalidOperationException("File path is not set properly");

			try
			{
				_logger.Debug("Exporting block started..");

				var plcDevice = dataStore.TiaPlcDevice ??
				                throw new InvalidOperationException("There is no plc device to export block from.");

				var block = plcDevice.PlcSoftware.BlockGroup.Blocks.Find(BlockName) ??
				            throw new InvalidOperationException($"There is no block with name '{BlockName}'");

				var directory = Path.GetDirectoryName(FilePath);
				if (!Directory.Exists(directory))
				{
					Directory.CreateDirectory(directory!);
					FileManager.RegisterDirectory(directory!);
				}

				if (File.Exists(FilePath))
					File.Delete(FilePath!);

				block.ExportToFile(FilePath!);

				FileManager.RegisterFile(FilePath!);

				return Task.FromResult(new ActionResult(ActionResultType.Success,
					$"Exported block '{BlockName}' to '{FilePath}'"));
			}
			catch (Exception e)
			{
				activity.RecordException(e);
				throw new ApplicationException("Could not export block", e);
			}
		}
	}
}