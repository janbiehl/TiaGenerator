using System;
using System.Linq;
using System.Threading.Tasks;
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
		public override Task<ActionResult> Execute(IDataStore datastore)
		{
			if (datastore is not DataStore dataStore)
			{
				throw new InvalidOperationException("Invalid datastore");
			}

			try
			{
				if (string.IsNullOrWhiteSpace(PlcName))
					return Task.FromResult(new ActionResult(ActionResultType.Fatal, "No PLC name specified."));

				var tiaProject = dataStore.TiaProject;

				if (tiaProject is null)
					return Task.FromResult(new ActionResult(ActionResultType.Fatal, "No TIA project found."));

				var plcDevices = DeviceUtils.FindAnyPlcDevices(tiaProject);
				var plcDevice = plcDevices.FirstOrDefault(x => x.PlcSoftware.Name == PlcName) ??
				                throw new NullReferenceException("The PLC device could not be found.");

				dataStore.TiaPlcDevice = plcDevice;
				return Task.FromResult(new ActionResult(ActionResultType.Success,
					$"Found PLC device '{plcDevice.PlcSoftware.Name}'."));
			}
			catch (Exception e)
			{
				throw new ApplicationException("Could not get PLC device", e);
			}
		}
	}
}