using System.Collections.Generic;
using TiaGenerator.Core.Models;

namespace TiaGenerator.Models
{
	public class GeneratorConfiguration
	{
		public string Name { get; set; }
		public string Description { get; set; }
		public string Author { get; set; }
		public string Date { get; set; }
		public IEnumerable<GeneratorAction> Actions { get; set; }
	}
}