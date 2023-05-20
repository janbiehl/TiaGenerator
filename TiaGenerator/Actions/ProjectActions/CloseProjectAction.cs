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
		public bool Save { get; set; }

		/// <inheritdoc />
		public override Task<ActionResult> Execute(IDataStore datastore)
		{
			if (datastore is not DataStore dataStore)
			{
				throw new InvalidOperationException("Invalid datastore");
			}

			try
			{
				var tiaProject = dataStore.TiaProject ??
				                 throw new NullReferenceException("There is no project to close");

				if (Save)
					tiaProject.Save();

				tiaProject.Close();

				return Task.FromResult(new ActionResult(ActionResultType.Success, "Project closed"));
			}
			catch (Exception e)
			{
				throw new ApplicationException("Could not close project", e);
			}
		}
	}
}