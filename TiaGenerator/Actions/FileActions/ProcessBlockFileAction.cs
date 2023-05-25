using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using OpenTelemetry.Trace;
using TiaGenerator.Core.Interfaces;
using TiaGenerator.Core.Models;
using TiaGenerator.Utils;

namespace TiaGenerator.Actions
{
	public class ProcessBlockFileAction : GeneratorAction
	{
		public string? BlockSourceFile { get; set; }
		public string? BlockDestinationFile { get; set; }
		public Dictionary<string, string>? Templates { get; set; }

		/// <inheritdoc />
		public override async Task<ActionResult> Execute(IDataStore datastore)
		{
			using var activity = Tracing.ActivitySource.StartActivity(nameof(ProcessBlockFileAction));

			activity?.SetTag(nameof(BlockSourceFile), BlockSourceFile);
			activity?.SetTag(nameof(BlockDestinationFile), BlockDestinationFile);
			activity?.SetTag(nameof(Templates), Templates);

			if (string.IsNullOrWhiteSpace(BlockSourceFile))
				return new ActionResult(ActionResultType.Failure, "No block file specified.");

			if (string.IsNullOrWhiteSpace(BlockDestinationFile))
				return new ActionResult(ActionResultType.Failure, "No block destination file specified.");

			if (Templates == null)
				return new ActionResult(ActionResultType.Failure, "No templates specified.");

			try
			{
				await FileManager.CopyFile(BlockSourceFile!, BlockDestinationFile!, true);
				await FileProcessorUtils.ReplaceInFile(BlockDestinationFile!, Templates!);

				return new ActionResult(ActionResultType.Success, $"Block file '{BlockDestinationFile}' processed.");
			}
			catch (Exception e)
			{
				activity.RecordException(e);
				throw new ApplicationException("Error while importing and processing block.", e);
			}
		}
	}
}