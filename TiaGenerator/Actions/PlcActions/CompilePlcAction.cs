using System;
using System.Threading.Tasks;
using OpenTelemetry.Trace;
using TiaGenerator.Core.Interfaces;
using TiaGenerator.Core.Models;
using TiaGenerator.Models;
using TiaGenerator.Tia.Extensions;

namespace TiaGenerator.Actions
{
	public class CompilePlcAction : GeneratorAction, ICompilePlcAction
	{
		/// <inheritdoc />
		public override Task<ActionResult> Execute(IDataStore datastore)
		{
			using var activity = Tracing.ActivitySource.StartActivity(nameof(CompilePlcAction));

			if (datastore is not DataStore dataStore)
			{
				throw new InvalidOperationException("Invalid datastore");
			}

			try
			{
				var plcDevice = dataStore.TiaPlcDevice
				                ?? throw new InvalidOperationException("There is no plc device to compile.");

				var result = plcDevice.PlcSoftware.Compile();

				if (result is null)
					return Task.FromResult(new ActionResult(ActionResultType.Fatal, "Could not compile plc"));

				return Task.FromResult(new ActionResult(ActionResultType.Success,
					$"PLC compiled: Warnings: {result.WarningCount}, Errors: {result.ErrorCount}"));
			}
			catch (Exception e)
			{
				activity.RecordException(e);
				throw new ApplicationException("Could not compile plc", e);
			}
		}
	}
}