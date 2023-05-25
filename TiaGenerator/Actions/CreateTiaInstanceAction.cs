using System;
using System.Threading.Tasks;
using OpenTelemetry.Trace;
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
		public override Task<ActionResult> Execute(IDataStore datastore)
		{
			using var activity = Tracing.ActivitySource.StartActivity(nameof(ProcessBlockFileAction));

			activity?.SetTag(nameof(WithInterface), WithInterface);

			if (datastore is not DataStore dataStore)
			{
				throw new InvalidOperationException("Invalid datastore");
			}

			try
			{
				// Close project, when there is one
				var project = dataStore.TiaProject;
				project?.Close();
				dataStore.TiaPortal = null;

				// Close tia portal, when there is one
				var existingPortal = dataStore.TiaPortal;
				existingPortal?.Dispose();
				dataStore.TiaPortal = null;

				TiaPortal tiaPortal =
					new(WithInterface ? TiaPortalMode.WithUserInterface : TiaPortalMode.WithoutUserInterface);

				dataStore.TiaPortal = tiaPortal;

				return Task.FromResult(new ActionResult(ActionResultType.Success, "TIA Portal instance created"));
			}
			catch (Exception e)
			{
				activity.RecordException(e);
				throw new ApplicationException("Could not create TIA Portal instance", e);
			}
		}
	}
}