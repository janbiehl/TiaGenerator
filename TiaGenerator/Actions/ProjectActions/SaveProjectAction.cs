using System;
using Siemens.Engineering;
using TiaGenerator.Core.Interfaces;
using TiaGenerator.Core.Models;
using TiaGenerator.Models;

namespace TiaGenerator.Actions
{
	public class SaveProjectAction : GeneratorAction
	{
		/// <inheritdoc />
		public override (ActionResult result, string message) Execute(IDataStore datastore)
		{
			try
			{
				var project = datastore.GetValue<Project>(DataStore.TiaProjectKey) ??
				              throw new NullReferenceException("There is no project to save.");

				project.Save();
				return new(ActionResult.Success, "Project saved.");
			}
			catch (Exception e)
			{
				throw new ApplicationException("Could not save project", e);
			}
		}
	}
}