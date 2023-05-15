using Mockup.Models;

namespace Mockup.Actions;

public abstract class GeneratorAction
{
	/// <summary>
	/// The type of action that was used
	/// </summary>
	public string? Type { get; set; }
	
	/// <summary>
	/// Execute the action using the the datastore
	/// </summary>
	/// <param name="dataStore">The store that holds the data</param>
	public abstract void Execute(DataStore dataStore);
}