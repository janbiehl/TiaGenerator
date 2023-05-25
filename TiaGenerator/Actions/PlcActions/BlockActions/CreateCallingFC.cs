using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using OpenTelemetry.Trace;
using Siemens.Engineering;
using Siemens.Engineering.SW;
using TiaGenerator.Core.Interfaces;
using TiaGenerator.Core.Models;
using TiaGenerator.Models;
using TiaGenerator.Templates;
using TiaGenerator.Tia;
using TiaGenerator.Tia.Extensions;
using TiaGenerator.Tia.Utils;
using TiaGenerator.Utils;

namespace TiaGenerator.Actions
{
	/// <summary>
	/// This action creates a FC that calls every block that is contained in a specific block group
	/// </summary>
	public class CreateCallingFC : GeneratorAction
	{
		/// <summary>
		/// The block group that contains the blocks that should be called. Separated by '/'.
		/// </summary>
		/// <remarks>This works for FC and FB block calls</remarks>
		public string? TargetBlockGroup { get; set; }

		/// <summary>
		/// The block group the calling FC should be created in
		/// </summary>
		public string? BlockGroup { get; set; }

		/// <summary>
		/// The name that the generated FC should have
		/// </summary>
		public string? BlockName { get; set; }

		/// <summary>
		/// The author that will be used for the generated block
		/// </summary>
		public string? Author { get; set; }

		/// <summary>
		/// The family that will be used for the generated block
		/// </summary>
		public string? Family { get; set; }

		/// <summary>
		/// The number that will be used for the generated block
		/// </summary>
		public int BlockNumber { get; set; }

		/// <summary>
		/// Whether the block should be automatically numbered or not
		/// </summary>
		public bool AutoNumber { get; set; }

		/// <inheritdoc />
		public override async Task<ActionResult> Execute(IDataStore datastore)
		{
			using var activity = Tracing.ActivitySource.StartActivity(nameof(CreateCallingFC));

			activity?.SetTag(nameof(TargetBlockGroup), TargetBlockGroup);
			activity?.SetTag(nameof(BlockGroup), BlockGroup);
			activity?.SetTag(nameof(BlockName), BlockName);
			activity?.SetTag(nameof(Author), Author);
			activity?.SetTag(nameof(Family), Family);
			activity?.SetTag(nameof(BlockNumber), BlockNumber);
			activity?.SetTag(nameof(AutoNumber), AutoNumber);

			if (string.IsNullOrWhiteSpace(TargetBlockGroup))
				return new ActionResult(ActionResultType.Failure, "No target block group specified.");

			if (string.IsNullOrWhiteSpace(BlockGroup))
				return new ActionResult(ActionResultType.Failure, "No block group specified.");

			if (string.IsNullOrWhiteSpace(BlockName))
				return new ActionResult(ActionResultType.Failure, "No block name specified.");

			if (string.IsNullOrWhiteSpace(Author))
				return new ActionResult(ActionResultType.Failure, "No author specified.");

			if (string.IsNullOrWhiteSpace(Family))
				return new ActionResult(ActionResultType.Failure, "No family specified.");

			if (BlockNumber < 1)
				return new ActionResult(ActionResultType.Failure, "Invalid block number -> It may not be 0 or less");

			try
			{
				if (datastore is not DataStore dataStore)
				{
					throw new InvalidOperationException("Invalid datastore");
				}

				var plcDevice = dataStore.TiaPlcDevice ??
				                throw new InvalidOperationException("There is no plc device in the data store.");

				var targetBlockGroup =
					PlcSoftwareUtils.GetBlockGroup(plcDevice.PlcSoftware, TargetBlockGroup!.Split('/'));

				if (targetBlockGroup is null)
					return new ActionResult(ActionResultType.Failure,
						$"Block group '{TargetBlockGroup}' does not exist.");

				var networks = new List<string>();

				// Get the information about the blocks inside the block group
				foreach (var block in targetBlockGroup.Blocks)
				{
					var blockType = block.GetBlockType();

					switch (blockType)
					{
						case BlockType.Fb:
						{
							TemplateFbBlockCall fbBlockCall = new()
							{
								BlockName = block.Name,
								BlockInstanceName = $"iDB_{block.Name}"
							};

							networks.Add(fbBlockCall.TransformText());
							break;
						}
						case BlockType.Fc:
						{
							TemplateFcBlockCall dummy = new()
							{
								BlockName = block.Name
							};

							networks.Add(dummy.TransformText());
							break;
						}
						case BlockType.Undefined:
						case BlockType.Ob:
						case BlockType.Db:
						case BlockType.Idb:
						case BlockType.Adb:
							break;
						default:
							throw new ArgumentOutOfRangeException();
					}
				}

				TemplateBlockFC blockFc = new()
				{
					BlockName = BlockName!,
					Author = Author!,
					Family = Family!,
					AutoNumber = AutoNumber,
					BlockNumber = BlockNumber,
					Networks = networks
				};

				var tempBlockFilePath = PathUtils.GetBlockFilePath(PathUtils.ApplicationTempDirectory, BlockName!);
				await FileManager.CreateFileAndWriteAll(tempBlockFilePath, blockFc.TransformText());

				var blockGroup = dataStore.TiaPlcDevice.PlcSoftware.GetOrCreateGroup(BlockGroup!.Split('/'));
				var importedBlocks = blockGroup.Blocks.ImportBlocksFromFile(tempBlockFilePath, ImportOptions.None,
					SWImportOptions.IgnoreStructuralChanges | SWImportOptions.IgnoreUnitAttributes |
					SWImportOptions.IgnoreMissingReferencedObjects);

				if (importedBlocks.Count > 0)
					return new ActionResult(ActionResultType.Success,
						$"FC '{BlockName}' created in block group '{BlockGroup}', invoking the blocks from {TargetBlockGroup}");

				return new ActionResult(ActionResultType.Failure,
					$"FC '{BlockName}' could not be created in block group '{BlockGroup}'");
			}
			catch (Exception e)
			{
				activity.RecordException(e);
				throw new ApplicationException("Error while processing block file action.", e);
			}
		}
	}
}