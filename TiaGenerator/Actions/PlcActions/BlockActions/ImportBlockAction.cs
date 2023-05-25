using System;
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
	public class ImportBlockAction : GeneratorAction
	{
		public string? BlockFile { get; set; }

		/// <summary>
		/// The block group to import the block to. Separated by '/'.
		/// </summary>
		public string? BlockGroup { get; set; }

		/// <inheritdoc />
		public override Task<ActionResult> Execute(IDataStore datastore)
		{
			using var activity = Tracing.ActivitySource.StartActivity(nameof(ImportBlockAction));

			activity?.SetTag(nameof(BlockFile), BlockFile);
			activity?.SetTag(nameof(BlockGroup), BlockGroup);

			if (datastore is not DataStore dataStore)
			{
				throw new InvalidOperationException("Invalid datastore");
			}

			if (!File.Exists(BlockFile))
				return Task.FromResult(new ActionResult(ActionResultType.Failure, "The block file does not exist."));

			if (string.IsNullOrWhiteSpace(BlockGroup))
				return Task.FromResult(new ActionResult(ActionResultType.Failure, "No block group specified."));

			try
			{
				var plcDevice = dataStore.TiaPlcDevice ??
				                throw new InvalidOperationException("There is no plc device in the data store.");

				var blockGroup = plcDevice.PlcSoftware.GetOrCreateGroup(TiaUtils.GetBlockGroups(BlockGroup!));

				var blocks = blockGroup.Blocks.ImportBlocksFromFile(BlockFile!,
					ImportOptions.None,
					SWImportOptions.IgnoreMissingReferencedObjects | SWImportOptions.IgnoreStructuralChanges |
					SWImportOptions.IgnoreUnitAttributes);

				if (blocks.Count <= 0)
					return Task.FromResult(new ActionResult(ActionResultType.Failure,
						$"No blocks imported from '{BlockFile}'."));

				return Task.FromResult(new ActionResult(ActionResultType.Success, $"Imported {blocks.Count} blocks."));
			}
			catch (Exception e)
			{
				activity.RecordException(e);
				throw new ApplicationException("Error while importing and processing block.", e);
			}
		}
	}
}