using System;
using System.Threading.Tasks;
using Siemens.Engineering;
using TiaGenerator.Core.Interfaces;
using TiaGenerator.Core.Models;
using TiaGenerator.Models;

namespace TiaGenerator.Actions
{
	public class SaveProjectAction : GeneratorAction
	{
		/// <inheritdoc />
		public override Task<GeneratorActionResult> Execute(IDataStore datastore)
		{
			try
			{
				var project = datastore.GetValue<Project>(DataStore.TiaProjectKey) ??
				              throw new NullReferenceException("There is no project to save.");

				project.Save();
				return Task.FromResult(new GeneratorActionResult(ActionResult.Success, "Project saved."));
			}
			catch (Exception e)
			{
				throw new ApplicationException("Could not save project", e);
			}
		}
	}
}