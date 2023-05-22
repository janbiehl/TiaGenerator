using System;
using System.Threading.Tasks;
using TiaGenerator.Core.Interfaces;
using TiaGenerator.Core.Models;
using TiaGenerator.Models;
using TiaGenerator.Tia.Extensions;
using TiaGenerator.Tia.Models;

namespace TiaGenerator.Actions
{
	public class CreatePlcAction : GeneratorAction, ICreatePlcAction
	{
		/// <inheritdoc />
		public string? PlcName { get; set; }

		/// <inheritdoc />
		public string? PlcOrderNumber { get; set; }

		/// <inheritdoc />
		public override Task<ActionResult> Execute(IDataStore datastore)
		{
			if (datastore is not DataStore dataStore)
			{
				throw new InvalidOperationException("Invalid datastore");
			}

			if (string.IsNullOrWhiteSpace(PlcName))
				return Task.FromResult(new ActionResult(ActionResultType.Fatal, "PLC name is not set"));

			if (string.IsNullOrWhiteSpace(PlcOrderNumber))
				return Task.FromResult(new ActionResult(ActionResultType.Fatal, "PLC order number is not set"));

			try
			{
				var tiaProject = dataStore.TiaProject;

				var device = tiaProject.CreateDevice($"OrderNumber:{PlcOrderNumber}", "newDevice", PlcName!);

				PlcDevice plcDevice = new(device);

				dataStore.TiaPlcDevice = plcDevice;

				return Task.FromResult(new ActionResult(ActionResultType.Success, "Device created"));
			}
			catch (Exception e)
			{
				throw new ApplicationException("Could not create device", e);
			}
		}
	}
}