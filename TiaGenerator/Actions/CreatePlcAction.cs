using Mockup.Core.Interfaces;
using Mockup.Models;

namespace Mockup.Actions;

public class CreatePlcAction : GeneratorAction, ICreatePlcAction
{
	/// <inheritdoc />
	public string? PlcName { get; set; }

	/// <inheritdoc />
	public string? PlcOrderNumber { get; set; }
	
	/// <inheritdoc />
	public override void Execute(DataStore dataStore)
	{
		throw new NotImplementedException();
	}

}