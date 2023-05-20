using System;
using System.Threading.Tasks;
using TiaGenerator.Core.Interfaces;
using TiaGenerator.Core.Models;
using TiaGenerator.Models;
using TiaGenerator.Tia.Extensions;

namespace TiaGenerator.Actions
{
	public class ExportBlockAction : GeneratorAction
	{
		public string? BlockName { get; set; }
		public string? FilePath { get; set; }

		/// <inheritdoc />
		public override Task<ActionResult> Execute(IDataStore datastore)
		{
			if (datastore is not DataStore dataStore)
			{
				throw new InvalidOperationException("Invalid datastore");
			}

			if (string.IsNullOrWhiteSpace(BlockName))
				throw new InvalidOperationException("Block name is not set properly");

			if (string.IsNullOrWhiteSpace(FilePath))
				throw new InvalidOperationException("File path is not set properly");

			try
			{
				var plcDevice = dataStore.TiaPlcDevice ??
				                throw new InvalidOperationException("There is no plc device to export block from.");

				var block = plcDevice.PlcSoftware.BlockGroup.Blocks.Find(BlockName) ??
				            throw new InvalidOperationException($"There is no block with name '{BlockName}'");

				block.ExportToFile(FilePath!);

				return Task.FromResult(new ActionResult(ActionResultType.Success,
					$"Block '{block.Name}' exported to '{FilePath}'"));
			}
			catch (Exception e)
			{
				throw new ApplicationException("Could not export block", e);
			}
		}
	}
}