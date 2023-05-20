using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
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
		public override Task<GeneratorActionResult> Execute(IDataStore dataStore)
		{
			try
			{
				if (string.IsNullOrWhiteSpace(ProjectFilePath))
					return Task.FromResult(new GeneratorActionResult(ActionResult.Failure, "Project file is not set"));

				if (!File.Exists(ProjectFilePath))
					return Task.FromResult(new GeneratorActionResult(ActionResult.Failure,
						"Project file does not exist"));

				var tiaPortal = dataStore.GetValue<TiaPortal>(DataStore.TiaPortalKey);

				if (tiaPortal is null)
					return Task.FromResult(new GeneratorActionResult(ActionResult.Failure,
						"TIA Portal instance not found"));

				var existingProject = dataStore.GetValue<Project>(DataStore.TiaProjectKey);

				if (existingProject != null)
					return Task.FromResult(new GeneratorActionResult(ActionResult.Fatal,
						"There is already an open project"));

				var project = tiaPortal.OpenProject(ProjectFilePath!, true, credentials =>
				{
					credentials.Type = UmacUserType.Project;
					credentials.Name = Username;
					credentials.SetPassword(new NetworkCredential(string.Empty, Password).SecurePassword);
				});

				if (project is null)
					return Task.FromResult(new GeneratorActionResult(ActionResult.Failure,
						"Project could not be opened"));

				dataStore.SetValue(DataStore.TiaProjectKey, project);
				return Task.FromResult(new GeneratorActionResult(ActionResult.Success, "Project opened"));
			}
			catch (Exception e)
			{
				throw new ApplicationException("Could not open project", e);
			}
		}
	}
}