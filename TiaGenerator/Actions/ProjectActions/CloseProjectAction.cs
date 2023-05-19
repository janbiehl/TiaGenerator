using System;
using Siemens.Engineering;
using TiaGenerator.Core.Interfaces;
using TiaGenerator.Core.Models;
using TiaGenerator.Models;

namespace TiaGenerator.Actions
{
	public class CloseProjectAction : GeneratorAction
	{
		public bool SaveProject { get; set; }

		/// <inheritdoc />
		public override (ActionResult result, string message) Execute(IDataStore datastore)
		{
			try
			{
				var tiaProject = datastore.GetValue<Project>(DataStore.TiaProjectKey) ??
				                 throw new NullReferenceException("There is no project to close");

				if (SaveProject)
					tiaProject.Save();

				tiaProject.Close();

				return (ActionResult.Success, "Project closed");
			}
			catch (Exception e)
			{
				throw new ApplicationException("Could not close project", e);
			}
		}
	}
}