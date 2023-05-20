using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace TiaGenerator.Utils
{
	public static class PathUtils
	{
		public static string ApplicationTempDirectory => Path.Combine(Path.GetTempPath(), "TiaGenerator");

		public static string GetBlockFilePath(string directory, string blockName)
		{
			if (string.IsNullOrWhiteSpace(directory))
				throw new ArgumentException("Value cannot be null or whitespace.", nameof(directory));
			if (string.IsNullOrWhiteSpace(blockName))
				throw new ArgumentException("Value cannot be null or whitespace.", nameof(blockName));

			return Path.Combine(directory, $"{blockName}.xml");
		}

		public static string GetBlockMetaFilePath(string directory, string blockName)
		{
			if (string.IsNullOrWhiteSpace(directory))
				throw new ArgumentException("Value cannot be null or whitespace.", nameof(directory));
			if (string.IsNullOrWhiteSpace(blockName))
				throw new ArgumentException("Value cannot be null or whitespace.", nameof(blockName));

			return Path.Combine(directory, $"{blockName}.meta");
		}

		public static string GetDirectoryFromGroup(string baseDirectory, IEnumerable<string> groups)
		{
			return groups.Aggregate(baseDirectory, (current, group) => Path.Combine(current, group));
		}

		public static string GetPathRelativeToApplicationDirectory(string relativePath)
		{
			return Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location)
			                    ?? throw new InvalidOperationException(), relativePath);
		}
	}
}