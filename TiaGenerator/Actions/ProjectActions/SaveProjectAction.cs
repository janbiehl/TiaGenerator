using System;
using System.Threading.Tasks;
using TiaGenerator.Core.Interfaces;
using TiaGenerator.Core.Models;
using TiaGenerator.Models;

namespace TiaGenerator.Actions
{
	public class SaveProjectAction : GeneratorAction
	{
		/// <inheritdoc />
		public override Task<ActionResult> Execute(IDataStore datastore)
		{
			if (datastore is not DataStore dataStore)
			{
				throw new InvalidOperationException("Invalid datastore");
			}

			try
			{
				var project = dataStore.TiaProject ??
				              throw new NullReferenceException("There is no project to save.");

				project.Save();
				return Task.FromResult(new ActionResult(ActionResultType.Success, "Project saved."));
			}
			catch (Exception e)
			{
				throw new ApplicationException("Could not save project", e);
			}
		}
	}
}