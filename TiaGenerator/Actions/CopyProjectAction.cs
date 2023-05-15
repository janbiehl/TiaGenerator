using System;
using Mockup.Core.Interfaces;
using Mockup.Models;

namespace Mockup.Actions;

public class CopyProjectAction : GeneratorAction, ICopyProjectAction
{
	public string? SourceProjectFile { get; set; }
	
	public string? TargetProjectDirectory { get; set; }
	
	/// <inheritdoc />
	public override void Execute(DataStore dataStore)
	{
		throw new NotImplementedException();
	}
}