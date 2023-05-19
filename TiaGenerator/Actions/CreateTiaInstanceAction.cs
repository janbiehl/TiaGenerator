using System;
using Siemens.Engineering;
using TiaGenerator.Core.Interfaces;
using TiaGenerator.Core.Models;
using TiaGenerator.Models;
using TiaGenerator.Services;

namespace TiaGenerator.Actions
{
	public class CreateTiaInstanceAction : GeneratorAction, ICreateTiaInstanceAction
	{
		/// <inheritdoc />
		public bool WithInterface { get; set; }

		/// <inheritdoc />
		public override (ActionResult result, string message) Execute(IDataStore datastore)
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

				return (ActionResult.Success, "TIA Portal instance created");
			}
			catch (Exception e)
			{
				return (ActionResult.Failure, e.Message);
			}
		}
	}
}