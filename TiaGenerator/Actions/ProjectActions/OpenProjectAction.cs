using System;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using OpenTelemetry.Trace;
using Siemens.Engineering;
using TiaGenerator.Core.Interfaces;
using TiaGenerator.Core.Models;
using TiaGenerator.Models;
using TiaGenerator.Tia.Extensions;

namespace TiaGenerator.Actions
{
	public class OpenProjectAction : GeneratorAction, IOpenProjectAction
	{
		/// <inheritdoc />
		public string? ProjectFilePath { get; set; }

		/// <inheritdoc />
		public string? Username { get; set; }

		/// <inheritdoc />
		public string? Password { get; set; }

		/// <inheritdoc />
		public override Task<ActionResult> Execute(IDataStore datastore)
		{
			using var activity = Tracing.ActivitySource.StartActivity(nameof(OpenProjectAction));

			activity?.SetTag(nameof(ProjectFilePath), ProjectFilePath);
			activity?.SetTag(nameof(Username), Username);
			activity?.SetTag(nameof(Password), Regex.Replace(Password ?? string.Empty, ".*", "*"));

			if (datastore is not DataStore dataStore)
			{
				throw new InvalidOperationException("Invalid datastore");
			}

			try
			{
				if (string.IsNullOrWhiteSpace(ProjectFilePath))
					return Task.FromResult(new ActionResult(ActionResultType.Failure, "Project file is not set"));

				if (!File.Exists(ProjectFilePath))
					return Task.FromResult(new ActionResult(ActionResultType.Failure,
						"Project file does not exist"));

				var tiaPortal = dataStore.TiaPortal;

				if (tiaPortal is null)
					return Task.FromResult(new ActionResult(ActionResultType.Fatal,
						"TIA Portal instance not found"));

				var existingProject = dataStore.TiaProject;

				if (existingProject != null)
					return Task.FromResult(new ActionResult(ActionResultType.Fatal,
						"There is already an open project"));

				var project = tiaPortal.OpenProject(ProjectFilePath!, true, credentials =>
				{
					credentials.Type = UmacUserType.Project;
					credentials.Name = Username;
					credentials.SetPassword(new NetworkCredential(string.Empty, Password).SecurePassword);
				});

				if (project is null)
					return Task.FromResult(new ActionResult(ActionResultType.Failure,
						"Project could not be opened"));

				dataStore.TiaProject = project;
				return Task.FromResult(new ActionResult(ActionResultType.Success, "Project opened"));
			}
			catch (Exception e)
			{
				activity.RecordException(e);
				throw new ApplicationException("Could not open project", e);
			}
		}
	}
}