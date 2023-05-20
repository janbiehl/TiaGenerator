using System;
using System.Threading.Tasks;
using Siemens.Engineering;
using TiaGenerator.Core.Interfaces;
using TiaGenerator.Core.Models;
using TiaGenerator.Models;

namespace TiaGenerator.Actions
{
	public class CreateTiaInstanceAction : GeneratorAction, ICreateTiaInstanceAction
	{
		/// <inheritdoc />
		public bool WithInterface { get; set; }

		/// <inheritdoc />
		public override Task<GeneratorActionResult> Execute(IDataStore datastore)
		{
			try
			{
				// Close project, when there is one
				var project = datastore.GetValue<Project>(DataStore.TiaPortalKey);
				project?.Close();

				// Close tia portal, when there is one
				var existingPortal = datastore.GetValue<TiaPortal>(DataStore.TiaPortalKey);
				existingPortal?.Dispose();

				TiaPortal tiaPortal =
					new(WithInterface ? TiaPortalMode.WithUserInterface : TiaPortalMode.WithoutUserInterface);

				datastore.SetValue(DataStore.TiaPortalKey, tiaPortal);

				return Task.FromResult(new GeneratorActionResult(ActionResult.Success, "TIA Portal instance created"));
			}
			catch (Exception e)
			{
				throw new ApplicationException("Could not create TIA Portal instance", e);
			}
		}
	}
}