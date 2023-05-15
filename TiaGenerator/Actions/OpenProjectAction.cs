using Mockup.Core.Interfaces;
using Mockup.Models;

namespace Mockup.Actions;

public class OpenProjectAction : GeneratorAction, IOpenProjectAction
{
	/// <inheritdoc />
	public string? ProjectFilePath { get; set; }

	/// <inheritdoc />
	public override void Execute(DataStore dataStore)
	{
		throw new NotImplementedException();
	}
}