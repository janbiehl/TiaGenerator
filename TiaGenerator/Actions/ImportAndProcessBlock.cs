using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Siemens.Engineering;
using Siemens.Engineering.SW;
using TiaGenerator.Core.Interfaces;
using TiaGenerator.Core.Models;
using TiaGenerator.Tia.Extensions;
using TiaGenerator.Tia.Models;
using TiaGenerator.Utils;

namespace TiaGenerator.Actions
{
	public class ImportAndProcessBlock : GeneratorAction
	{
		public string? BlockSourceFile { get; set; }
		public string? BlockDestinationFile { get; set; }

		/// <summary>
		/// The block group to import the block to. Separated by '/'.
		/// </summary>
		public string? BlockGroup { get; set; }

		public List<KeyValuePair<string, string>>? Templates { get; set; }

		/// <inheritdoc />
		public override async Task<GeneratorActionResult> Execute(IDataStore datastore)
		{
			if (string.IsNullOrWhiteSpace(BlockSourceFile))
				return new GeneratorActionResult(ActionResult.Failure, "No block file specified.");

			if (string.IsNullOrWhiteSpace(BlockDestinationFile))
				return new GeneratorActionResult(ActionResult.Failure, "No block destination file specified.");

			if (!File.Exists(BlockSourceFile))
				return new GeneratorActionResult(ActionResult.Failure,
					$"Block file '{BlockSourceFile}' does not exist.");

			if (string.IsNullOrWhiteSpace(BlockGroup))
				return new GeneratorActionResult(ActionResult.Failure, "No block group specified.");

			try
			{
				await FileProcessorUtils.ReplaceInFile(BlockSourceFile, Templates);

				using var sourceReader = new StreamReader(BlockSourceFile!, Encoding.UTF8);
				using var destinationWriter = new StreamWriter(BlockDestinationFile!, false, Encoding.UTF8);

				while (await sourceReader.ReadLineAsync() is { } fileLine)
				{
					var tmpLine = fileLine;

					foreach (var template in Templates!)
					{
						tmpLine = Regex.Replace(tmpLine, template.Key, template.Value);
					}

					await destinationWriter.WriteLineAsync(tmpLine);
				}

				var plcDevice = datastore.GetValue<PlcDevice>("PlcDevice") ??
				                throw new InvalidOperationException("There is no plc device in the data store.");

				var blockGroup = plcDevice.PlcSoftware.GetOrCreateGroup(BlockGroup.Split("/"));

				var blocks = blockGroup.Blocks.ImportBlocksFromFile(BlockDestinationFile!,
					ImportOptions.None,
					SWImportOptions.IgnoreMissingReferencedObjects | SWImportOptions.IgnoreStructuralChanges |
					SWImportOptions.IgnoreUnitAttributes);

				if (blocks.Count <= 0)
					return new GeneratorActionResult(ActionResult.Failure,
						$"No blocks imported from '{BlockDestinationFile}'.");

				return new GeneratorActionResult(ActionResult.Success, $"Imported {blocks.Count} blocks.");
			}
			catch (Exception e)
			{
				throw new ApplicationException("Error while importing and processing block.", e);
			}
		}
	}
}