using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace TiaGenerator
{
	public static class FileManager
	{
		private static readonly HashSet<string> Files = new();
		private static readonly HashSet<string> Directories = new();

		public static void RegisterDirectory(string directory)
		{
			if (string.IsNullOrWhiteSpace(directory))
				throw new ArgumentException("Value cannot be null or whitespace.", nameof(directory));

			Directories.Add(directory);
		}

		public static void RegisterFile(string filePath)
		{
			if (string.IsNullOrWhiteSpace(filePath))
				throw new ArgumentException("Value cannot be null or whitespace.", nameof(filePath));

			Files.Add(filePath);
		}

		public static void Cleanup()
		{
			foreach (var file in Files.Where(File.Exists))
			{
				File.Delete(file);
			}

			foreach (var directory in Directories)
			{
				Directory.Delete(directory, true);
			}
		}
	}
}