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
		public override Task<GeneratorActionResult> Execute(IDataStore datastore)
		{
			try
			{
				var tiaProject = datastore.GetValue<Project>(DataStore.TiaProjectKey) ??
				                 throw new InvalidOperationException("There is no project to get the first plc from.");

				var plcDevice = tiaProject.FindFirstPlcDevice() ??
				                throw new InvalidOperationException("There is no plc device in the project.");

				datastore.SetValue(DataStore.TiaPlcDeviceKey, plcDevice);
				return Task.FromResult(new GeneratorActionResult(ActionResult.Success,
					$"PLC device '{plcDevice.PlcSoftware.Name}' found"));
			}
			catch (Exception e)
			{
				throw new ApplicationException("Could not get first plc", e);
			}
		}
	}
}