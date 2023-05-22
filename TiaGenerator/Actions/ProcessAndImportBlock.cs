using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Siemens.Engineering;
using Siemens.Engineering.SW;
using TiaGenerator.Core.Interfaces;
using TiaGenerator.Core.Models;
using TiaGenerator.Models;
using TiaGenerator.Tia.Extensions;
using TiaGenerator.Utils;

namespace TiaGenerator.Actions
{
	public class ProcessBlockAction : GeneratorAction
	{
		public string? BlockSourceFile { get; set; }
		public string? BlockDestinationFile { get; set; }
		public Dictionary<string, string>? Templates { get; set; }

		/// <inheritdoc />
		public override async Task<ActionResult> Execute(IDataStore datastore)
		{
			if (string.IsNullOrWhiteSpace(BlockSourceFile))
				return new ActionResult(ActionResultType.Failure, "No block file specified.");

			if (string.IsNullOrWhiteSpace(BlockDestinationFile))
				return new ActionResult(ActionResultType.Failure, "No block destination file specified.");

			if (!File.Exists(BlockSourceFile))
				return new ActionResult(ActionResultType.Failure,
					$"Block file '{BlockSourceFile}' does not exist.");

			if (Templates == null)
				return new ActionResult(ActionResultType.Failure, "No templates specified.");

			try
			{
				File.Copy(BlockSourceFile!, BlockDestinationFile!, true);
				await FileProcessorUtils.ReplaceInFile(BlockDestinationFile!, Templates!);

				FileManager.RegisterFile(BlockDestinationFile!);
				return new ActionResult(ActionResultType.Success, $"Block file '{BlockDestinationFile}' processed.");
			}
			catch (Exception e)
			{
				throw new ApplicationException("Error while importing and processing block.", e);
			}
		}
	}

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

				var blockGroup = plcDevice.PlcSoftware.GetOrCreateGroup(BlockGroup.Split("/"));

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
				throw new ApplicationException("Error while importing and processing block.", e);
			}
		}
	}

	[Obsolete("Use ProcessBlockAction & ImportBlockAction instead.", true)]
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
				File.Copy(BlockSourceFile!, BlockDestinationFile!, true);
				await FileProcessorUtils.ReplaceInFile(BlockDestinationFile!, Templates!);

				FileManager.RegisterFile(BlockDestinationFile!);

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
				throw new ApplicationException("Error while importing and processing block.", e);
			}
		}
	}
}