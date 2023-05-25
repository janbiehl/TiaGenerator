using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace TiaGenerator
{
	public static class FileManager
	{
		private static readonly SemaphoreSlim SemaphoreCreateDirectory = new(1, 1);
		private static readonly SemaphoreSlim SemaphoreCreateFile = new(1, 1);
		private static readonly SemaphoreSlim SemaphoreCopyFile = new(1, 1);
		private static readonly SemaphoreSlim SemaphoreCleanup = new(1, 1);

		private static readonly HashSet<string> Files = new();
		private static readonly HashSet<string> Directories = new();

		/// <summary>
		/// Create a directory and register it into the file manager
		/// </summary>
		/// <param name="directory">The directory to create</param>
		/// <param name="cancellationToken">Cancel async tasks</param>
		/// <exception cref="ArgumentException"></exception>
		/// <exception cref="InvalidOperationException"></exception>
		public static async Task<DirectoryInfo> CreateDirectory(string directory,
			CancellationToken cancellationToken = default)
		{
			if (string.IsNullOrWhiteSpace(directory))
				throw new ArgumentException("Value cannot be null or whitespace.", nameof(directory));

			var directoryName = Path.GetDirectoryName(directory);

			if (string.IsNullOrWhiteSpace(directoryName))
				throw new InvalidOperationException("There is no directory information provided");

			await SemaphoreCreateDirectory.WaitAsync(cancellationToken);

			try
			{
				RegisterDirectory(directoryName);
				return Directory.CreateDirectory(directoryName);
			}
			finally
			{
				SemaphoreCreateDirectory.Release();
			}
		}

		/// <summary>
		/// Create a file and register it into the file manager
		/// </summary>
		/// <remarks>Also creates the directory, when not present</remarks>
		/// <param name="filePath">The file to create</param>
		/// <param name="cancellationToken">Cancel async tasks</param>
		/// <returns></returns>
		/// <exception cref="ArgumentException"></exception>
		public static async Task<FileStream> CreateFile(string filePath, CancellationToken cancellationToken = default)
		{
			if (string.IsNullOrWhiteSpace(filePath))
				throw new ArgumentException("Value cannot be null or whitespace.", nameof(filePath));

			await CreateDirectory(filePath, cancellationToken);

			await SemaphoreCreateFile.WaitAsync(cancellationToken);

			try
			{
				RegisterFile(filePath);
				return File.Create(filePath);
			}
			finally
			{
				SemaphoreCreateFile.Release();
			}
		}

		public static async Task CreateFileAndWriteAll(string filePath, string content,
			CancellationToken cancellationToken = default)
		{
			using var fileStream = await CreateFile(filePath, cancellationToken);
			using var streamWriter = new StreamWriter(fileStream);
			await streamWriter.WriteAsync(content);
		}

		/// <summary>
		/// Copy a file and register it into the file manager
		/// </summary>
		/// <param name="sourceFile">The file to copy</param>
		/// <param name="destinationFile">The file or directory to copy the file to</param>
		/// <param name="overwrite">True means that a existing file will be overriden</param>
		/// <param name="cancellationToken">Cancel async tasks</param>
		public static async Task CopyFile(string sourceFile, string destinationFile, bool overwrite = false,
			CancellationToken cancellationToken = default)
		{
			await SemaphoreCopyFile.WaitAsync(cancellationToken);

			try
			{
				File.Copy(sourceFile, destinationFile, overwrite);
				RegisterFile(destinationFile);
			}
			finally
			{
				SemaphoreCopyFile.Release();
			}
		}

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

		/// <summary>
		/// Cleanup any files and directories that are registered in the file manager
		/// </summary>
		/// <param name="cancellationToken">Cancel async tasks</param>
		public static async Task Cleanup(CancellationToken cancellationToken = default)
		{
			await SemaphoreCleanup.WaitAsync(cancellationToken);

			try
			{
				foreach (var file in Files.Where(File.Exists))
				{
					File.Delete(file);
				}

				foreach (var directory in Directories)
				{
					Directory.Delete(directory, true);
				}

				// Empty the collections
				Files.Clear();
				Directories.Clear();
			}
			finally
			{
				SemaphoreCleanup.Release();
			}
		}
	}
}