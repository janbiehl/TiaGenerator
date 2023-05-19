using System;
using TiaGenerator.Core.Interfaces;
using TiaGenerator.Core.Models;
using TiaGenerator.Models;
using TiaGenerator.Tia.Extensions;
using TiaGenerator.Tia.Models;

namespace TiaGenerator.Actions
{
	public class ExportBlockAction : GeneratorAction
	{
		public string? BlockName { get; set; }
		public string? FilePath { get; set; }

		/// <inheritdoc />
		public override (ActionResult result, string message) Execute(IDataStore datastore)
		{
			if (string.IsNullOrWhiteSpace(BlockName))
				throw new InvalidOperationException("Block name is not set properly");

			if (string.IsNullOrWhiteSpace(FilePath))
				throw new InvalidOperationException("File path is not set properly");

			try
			{
				var plcDevice = datastore.GetValue<PlcDevice>(DataStore.TiaPlcDeviceKey) ??
				                throw new InvalidOperationException("There is no plc device to export block from.");

				var block = plcDevice.PlcSoftware.BlockGroup.Blocks.Find(BlockName) ??
				            throw new InvalidOperationException($"There is no block with name '{BlockName}'");

				block.ExportToFile(FilePath!);

				return (ActionResult.Success, $"Block '{block.Name}' exported to '{FilePath}'");
			}
			catch (Exception e)
			{
				throw new ApplicationException("Could not export block", e);
			}
		}
	}
}