using System;
using Siemens.Engineering;
using TiaGenerator.Core.Interfaces;
using TiaGenerator.Core.Models;
using TiaGenerator.Models;
using TiaGenerator.Services;
using TiaGenerator.Tia.Extensions;

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
				return (ActionResult.Failure, "PLC name is not set");

			if (string.IsNullOrWhiteSpace(PlcOrderNumber))
				return (ActionResult.Failure, "PLC order number is not set");

			try
			{
				var tiaProject = dataStore.GetValue<Project>(DataStore.TiaProjectKey);

				var device = tiaProject.CreateDevice($"OrderNumber:{PlcOrderNumber}", "newDevice", PlcName);

				if (device is null)
					return (ActionResult.Failure, "Device could not be created");

				dataStore.SetValue(DataStore.TiaPlcDeviceKey, device);
				return (ActionResult.Success, "Device created");
			}
			catch (Exception e)
			{
				return (ActionResult.Fatal, e.Message);
			}
		}
	}
}