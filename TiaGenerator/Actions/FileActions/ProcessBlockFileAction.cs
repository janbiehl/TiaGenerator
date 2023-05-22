using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using TiaGenerator.Core.Interfaces;
using TiaGenerator.Core.Models;
using TiaGenerator.Utils;

namespace TiaGenerator.Actions
{
	public class ProcessBlockFileAction : GeneratorAction
	{
		public string? BlockSourceFile { get; set; }
		public string? BlockDestinationFile { get; set; }
		public Dictionary<string, string>? Templates { get; set; }

		/// <inheritdoc />
		public override async Task<ActionResult> Execute(IDataStore datastore)
		{
			if (string.IsNullOrWhiteSpace(BlockSourceFile))
				return new ActionResult(ActionResultType.Failure, "No block file specified.");

			if (string.IsNullOrWhiteSpace(BlockDestinationFile))
				return new ActionResult(ActionResultType.Failure, "No block destination file specified.");

			if (!File.Exists(BlockSourceFile))
				return new ActionResult(ActionResultType.Failure,
					$"Block file '{BlockSourceFile}' does not exist.");

			if (Templates == null)
				return new ActionResult(ActionResultType.Failure, "No templates specified.");

			try
			{
				File.Copy(BlockSourceFile!, BlockDestinationFile!, true);
				await FileProcessorUtils.ReplaceInFile(BlockDestinationFile!, Templates!);

				FileManager.RegisterFile(BlockDestinationFile!);
				return new ActionResult(ActionResultType.Success, $"Block file '{BlockDestinationFile}' processed.");
			}
			catch (Exception e)
			{
				throw new ApplicationException("Error while importing and processing block.", e);
			}
		}
	}
}