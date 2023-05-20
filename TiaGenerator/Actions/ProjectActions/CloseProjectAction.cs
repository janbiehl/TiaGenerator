using System;
using System.Threading.Tasks;
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
		public override Task<GeneratorActionResult> Execute(IDataStore datastore)
		{
			try
			{
				var tiaProject = datastore.GetValue<Project>(DataStore.TiaProjectKey) ??
				                 throw new NullReferenceException("There is no project to close");

				if (SaveProject)
					tiaProject.Save();

				tiaProject.Close();

				return Task.FromResult(new GeneratorActionResult(ActionResult.Success, "Project closed"));
			}
			catch (Exception e)
			{
				throw new ApplicationException("Could not close project", e);
			}
		}
	}
}