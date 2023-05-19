using System;
using System.Linq;
using Siemens.Engineering;
using TiaGenerator.Core.Interfaces;
using TiaGenerator.Core.Models;
using TiaGenerator.Models;
using TiaGenerator.Tia.Utils;

namespace TiaGenerator.Actions
{
	public class GetPlcAction : GeneratorAction
	{
		public string? PlcName { get; set; }

		/// <inheritdoc />
		public override (ActionResult result, string message) Execute(IDataStore datastore)
		{
			try
			{
				if (string.IsNullOrWhiteSpace(PlcName))
					return (ActionResult.Fatal, "No PLC name specified.");

				var tiaProject = datastore.GetValue<Project>(DataStore.TiaProjectKey);

				if (tiaProject is null)
					return (ActionResult.Fatal, "No TIA project found.");

				var plcDevices = DeviceUtils.FindAnyPlcDevices(tiaProject);
				var plcDevice = plcDevices.FirstOrDefault(x => x.PlcSoftware.Name == PlcName) ??
				                throw new NullReferenceException("The PLC device could not be found.");

				datastore.SetValue(DataStore.TiaPlcDeviceKey, plcDevice);
				return (ActionResult.Success, $"Found PLC device '{plcDevice.PlcSoftware.Name}'.");
			}
			catch (Exception e)
			{
				throw new ApplicationException("Could not get PLC device", e);
			}
		}
	}
}