using System;
using System.Threading.Tasks;
using Siemens.Engineering;
using TiaGenerator.Core.Interfaces;
using TiaGenerator.Core.Models;
using TiaGenerator.Models;
using TiaGenerator.Tia.Extensions;

namespace TiaGenerator.Actions
{
	public class GetFirstPlcAction : GeneratorAction
	{
		/// <inheritdoc />
		public override Task<ActionResult> Execute(IDataStore datastore)
		{
			if (datastore is not DataStore dataStore)
			{
				throw new InvalidOperationException("Invalid datastore");
			}

			try
			{
				var tiaProject = dataStore.TiaProject ??
				                 throw new InvalidOperationException("There is no project to get the first plc from.");

				var plcDevice = tiaProject.FindFirstPlcDevice() ??
				                throw new InvalidOperationException("There is no plc device in the project.");

				dataStore.TiaPlcDevice = plcDevice;

				return Task.FromResult(new ActionResult(ActionResultType.Success,
					$"PLC device '{plcDevice.PlcSoftware.Name}' found"));
			}
			catch (Exception e)
			{
				throw new ApplicationException("Could not get first plc", e);
			}
		}
	}
}