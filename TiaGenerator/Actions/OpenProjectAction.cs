using System;
using System.IO;
using System.Net;
using Siemens.Engineering;
using TiaGenerator.Core.Interfaces;
using TiaGenerator.Core.Models;
using TiaGenerator.Models;
using TiaGenerator.Services;
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
		public override (ActionResult result, string message) Execute(IDataStore dataStore)
		{
			try
			{
				if (string.IsNullOrWhiteSpace(ProjectFilePath))
					return (ActionResult.Failure, "Project file is not set");

				if (!File.Exists(ProjectFilePath))
					return (ActionResult.Failure, "Project file does not exist");

				var tiaPortal = dataStore.GetValue<TiaPortal>(DataStore.TiaPortalKey);

				if (tiaPortal is null)
					return (ActionResult.Failure, "TIA Portal instance not found");

				var project = tiaPortal.OpenProject(ProjectFilePath!, true, credentials =>
				{
					credentials.Type = UmacUserType.Project;
					credentials.Name = Username;
					credentials.SetPassword(new NetworkCredential(string.Empty, Password).SecurePassword);
				});

				if (project is null)
					return (ActionResult.Failure, "Project could not be opened");

				dataStore.SetValue(DataStore.TiaProjectKey, project);
				return (ActionResult.Success, "Project opened");
			}
			catch (Exception e)
			{
				return (ActionResult.Fatal, e.Message);
			}
		}
	}
}