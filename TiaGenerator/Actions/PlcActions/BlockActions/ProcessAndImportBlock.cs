using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using OpenTelemetry.Trace;
using Siemens.Engineering;
using Siemens.Engineering.SW;
using TiaGenerator.Core.Interfaces;
using TiaGenerator.Core.Models;
using TiaGenerator.Models;
using TiaGenerator.Tia.Extensions;
using TiaGenerator.Utils;

namespace TiaGenerator.Actions
{
	[Obsolete("Use ProcessBlockFileAction & ImportBlockAction instead.", true)]
	public class ProcessAndImportBlock : GeneratorAction
	{
		public string? BlockSourceFile { get; set; }
		public string? BlockDestinationFile { get; set; }

		/// <summary>
		/// The block group to import the block to. Separated by '/'.
		/// </summary>
		public string? BlockGroup { get; set; }

		public Dictionary<string, string>? Templates { get; set; }

		/// <inheritdoc />
		public override async Task<ActionResult> Execute(IDataStore datastore)
		{
			using var activity = Tracing.ActivitySource.StartActivity(nameof(ProcessAndImportBlock));

			activity?.SetTag(nameof(BlockSourceFile), BlockSourceFile);
			activity?.SetTag(nameof(BlockDestinationFile), BlockDestinationFile);
			activity?.SetTag(nameof(BlockGroup), BlockGroup);
			activity?.SetTag(nameof(Templates), Templates);

			if (datastore is not DataStore dataStore)
			{
				throw new InvalidOperationException("Invalid datastore");
			}

			if (string.IsNullOrWhiteSpace(BlockSourceFile))
				return new ActionResult(ActionResultType.Failure, "No block file specified.");

			if (string.IsNullOrWhiteSpace(BlockDestinationFile))
				return new ActionResult(ActionResultType.Failure, "No block destination file specified.");

			if (!File.Exists(BlockSourceFile))
				return new ActionResult(ActionResultType.Failure,
					$"Block file '{BlockSourceFile}' does not exist.");

			if (string.IsNullOrWhiteSpace(BlockGroup))
				return new ActionResult(ActionResultType.Failure, "No block group specified.");

			if (Templates == null)
				return new ActionResult(ActionResultType.Failure, "No templates specified.");

			try
			{
				await FileManager.CopyFile(BlockSourceFile!, BlockDestinationFile!, true);
				await FileProcessorUtils.ReplaceInFile(BlockDestinationFile!, Templates!);

				var plcDevice = dataStore.TiaPlcDevice ??
				                throw new InvalidOperationException("There is no plc device in the data store.");

				var blockGroup = plcDevice.PlcSoftware.GetOrCreateGroup(BlockGroup.Split("/"));

				var blocks = blockGroup.Blocks.ImportBlocksFromFile(BlockDestinationFile!,
					ImportOptions.None,
					SWImportOptions.IgnoreMissingReferencedObjects | SWImportOptions.IgnoreStructuralChanges |
					SWImportOptions.IgnoreUnitAttributes);

				if (blocks.Count <= 0)
					return new ActionResult(ActionResultType.Failure,
						$"No blocks imported from '{BlockDestinationFile}'.");

				return new ActionResult(ActionResultType.Success, $"Imported {blocks.Count} blocks.");
			}
			catch (Exception e)
			{
				activity.RecordException(e);
				throw new ApplicationException("Error while importing and processing block.", e);
			}
		}
	}
}