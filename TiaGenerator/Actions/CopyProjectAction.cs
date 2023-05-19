using System;
using System.IO;
using Siemens.Engineering;
using TiaGenerator.Core.Interfaces;
using TiaGenerator.Core.Models;
using TiaGenerator.Tia.Utils;

namespace TiaGenerator.Actions
{
	public class CopyProjectAction : GeneratorAction, ICopyProjectAction
	{
		public string? SourceProjectFile { get; set; }

		public string? TargetProjectDirectory { get; set; }

		/// <inheritdoc />
		public override (ActionResult result, string message) Execute(IDataStore dataStore)
		{
			if (string.IsNullOrWhiteSpace(SourceProjectFile))
				return (ActionResult.Failure, "Source project file is not set");

			if (string.IsNullOrWhiteSpace(TargetProjectDirectory))
				return (ActionResult.Failure, "Target project directory is not set");

			try
			{
				var tiaPortal = new TiaPortal();
				var project = tiaPortal.Projects.Open(new FileInfo(SourceProjectFile!));

				ProjectUtils.SaveProjectAsNew(project, TargetProjectDirectory!);
				return (ActionResult.Success, "Project copied");
			}
			catch (Exception e)
			{
				return (ActionResult.Fatal, e.Message);
			}
		}
	}
}