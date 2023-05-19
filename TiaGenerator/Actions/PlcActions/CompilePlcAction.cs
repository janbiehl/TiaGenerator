using System;
using TiaGenerator.Core.Interfaces;
using TiaGenerator.Core.Models;
using TiaGenerator.Models;
using TiaGenerator.Tia.Extensions;
using TiaGenerator.Tia.Models;

namespace TiaGenerator.Actions
{
	public class CompilePlcAction : GeneratorAction
	{
		/// <inheritdoc />
		public override (ActionResult result, string message) Execute(IDataStore datastore)
		{
			var plcDevice = datastore.GetValue<PlcDevice>(DataStore.TiaPlcDeviceKey)
			                ?? throw new InvalidOperationException("There is no plc device to compile.");

			var result = plcDevice.PlcSoftware.Compile();

			if (result is null)
				return (ActionResult.Fatal, "Could not compile plc");

			return (ActionResult.Success,
				$"PLC compiled: Warnings: {result.WarningCount}, Errors: {result.ErrorCount}");
		}
	}
}