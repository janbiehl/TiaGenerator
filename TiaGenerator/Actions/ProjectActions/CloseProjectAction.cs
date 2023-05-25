using System;
using System.Threading.Tasks;
using OpenTelemetry.Trace;
using TiaGenerator.Core.Interfaces;
using TiaGenerator.Core.Models;
using TiaGenerator.Models;

namespace TiaGenerator.Actions
{
	public class CloseProjectAction : GeneratorAction
	{
		public bool Save { get; set; }

		/// <inheritdoc />
		public override Task<ActionResult> Execute(IDataStore datastore)
		{
			using var activity = Tracing.ActivitySource.StartActivity(nameof(CloseProjectAction));

			activity?.SetTag(nameof(Save), Save);

			if (datastore is not DataStore dataStore)
			{
				throw new InvalidOperationException("Invalid datastore");
			}

			try
			{
				var tiaProject = dataStore.TiaProject ??
				                 throw new NullReferenceException("There is no project to close");

				if (Save)
					tiaProject.Save();

				tiaProject.Close();

				return Task.FromResult(new ActionResult(ActionResultType.Success, "Project closed"));
			}
			catch (Exception e)
			{
				activity.RecordException(e);
				throw new ApplicationException("Could not close project", e);
			}
		}
	}
}