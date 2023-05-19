using System;
using Siemens.Engineering;
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
		public override (ActionResult result, string message) Execute(IDataStore dataStore)
		{
			if (string.IsNullOrWhiteSpace(PlcName))
				return (ActionResult.Fatal, "PLC name is not set");

			if (string.IsNullOrWhiteSpace(PlcOrderNumber))
				return (ActionResult.Fatal, "PLC order number is not set");

			try
			{
				var tiaProject = dataStore.GetValue<Project>(DataStore.TiaProjectKey);

				var device = tiaProject.CreateDevice($"OrderNumber:{PlcOrderNumber}", "newDevice", PlcName!);

				PlcDevice plcDevice = new(device);

				dataStore.SetValue(DataStore.TiaPlcDeviceKey, plcDevice);

				return (ActionResult.Success, "Device created");
			}
			catch (Exception e)
			{
				throw new ApplicationException("Could not create device", e);
			}
		}
	}
}