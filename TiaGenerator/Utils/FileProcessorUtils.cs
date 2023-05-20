using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace TiaGenerator.Utils
{
	public static class FileProcessorUtils
	{
		/// <summary>
		/// Reads the source file line by line, replaces the search string with the replace string and writes the result to the temp file.
		/// </summary>
		/// <param name="filePath">The file the text should be replaced in</param>
		/// <param name="search">The text to replace</param>
		/// <param name="replace">The value to replace</param>
		public static async Task ReplaceInFile(string filePath, string search, string replace)
		{
			if (!File.Exists(filePath))
				throw new FileNotFoundException("File not found", filePath);
			if (string.IsNullOrWhiteSpace(search))
				throw new ArgumentException("Value cannot be null or whitespace.", nameof(search));
			if (string.IsNullOrWhiteSpace(replace))
				throw new ArgumentException("Value cannot be null or whitespace.", nameof(replace));

			var tempFilePath = filePath + ".tmp";

			using (var sourceFile = File.OpenText(filePath))
			using (var tempFile = File.CreateText(tempFilePath))
			{
				while (await sourceFile.ReadLineAsync().ConfigureAwait(false) is { } line)
				{
					await tempFile.WriteLineAsync(Regex.Replace(line, search, replace))
						.ConfigureAwait(false);
				}
			}

			File.Replace(tempFilePath, filePath, null);
		}

		/// <summary>
		/// Replace multiple values in a file.
		/// </summary>
		/// <param name="filePath">The file which values should be replaced</param>
		/// <param name="values">The key defines the text to search, the value defines the text that will be inserted</param>
		/// <exception cref="FileNotFoundException">The file does not exist</exception>
		/// <exception cref="ArgumentException">Invalid input data</exception>
		public static async Task ReplaceInFile(string filePath, List<KeyValuePair<string, string>> values)
		{
			if (!File.Exists(filePath))
				throw new FileNotFoundException("File not found", filePath);

			foreach (var valuePair in values)
			{
				if (string.IsNullOrWhiteSpace(valuePair.Key) || string.IsNullOrWhiteSpace(valuePair.Value))
					throw new ArgumentException("Value cannot be null or whitespace.", nameof(values));
			}

			var tempFilePath = filePath + ".tmp";
			using (var sourceFile = File.OpenText(filePath))
			using (var tempFile = File.CreateText(tempFilePath))
			{
				while (await sourceFile.ReadLineAsync().ConfigureAwait(false) is { } rawLine)
				{
					var processedLine = rawLine;
					foreach (var valuePair in values)
					{
						processedLine = Regex.Replace(processedLine, valuePair.Key, valuePair.Value);
						//processedLine = processedLine.Replace(valuePair.Key, valuePair.Value);
					}

					await tempFile.WriteLineAsync(processedLine).ConfigureAwait(false);
				}
			}

			File.Replace(tempFilePath, filePath, null);
		}
	}
}