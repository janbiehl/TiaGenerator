using System;
using System.IO;
using System.Threading.Tasks;
using OpenTelemetry.Trace;
using TiaGenerator.Core.Interfaces;
using TiaGenerator.Core.Models;
using TiaGenerator.Models;
using TiaGenerator.Tia.Utils;

namespace TiaGenerator.Actions
{
	public class CopyProjectAction : GeneratorAction, ICopyProjectAction
	{
		public string? SourceProjectFile { get; set; }

		public string? TargetProjectDirectory { get; set; }

		/// <inheritdoc />
		public override Task<ActionResult> Execute(IDataStore datastore)
		{
			using var activity = Tracing.ActivitySource.StartActivity(nameof(CopyProjectAction));

			activity?.SetTag(nameof(SourceProjectFile), SourceProjectFile);
			activity?.SetTag(nameof(TargetProjectDirectory), TargetProjectDirectory);

			if (datastore is not DataStore dataStore)
			{
				throw new InvalidOperationException("Invalid datastore");
			}

			if (string.IsNullOrWhiteSpace(SourceProjectFile))
				return Task.FromResult(new ActionResult(ActionResultType.Fatal, "Source project file is not set"));

			if (string.IsNullOrWhiteSpace(TargetProjectDirectory))
				return Task.FromResult(new ActionResult(ActionResultType.Fatal,
					"Target project directory is not set"));

			try
			{
				var tiaPortal = dataStore.TiaPortal ?? throw new Exception("TIA Portal is not open");

				var project = tiaPortal.Projects.Open(new FileInfo(SourceProjectFile!));

				ProjectUtils.SaveProjectAsNew(project, TargetProjectDirectory!);

				project.Close();
				return Task.FromResult(new ActionResult(ActionResultType.Success, "Project copied"));
			}
			catch (Exception e)
			{
				activity.RecordException(e);
				throw new ApplicationException("Could not copy project", e);
			}
		}
	}
}