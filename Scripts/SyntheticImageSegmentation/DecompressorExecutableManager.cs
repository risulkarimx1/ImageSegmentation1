using System;
using System.Diagnostics;
using System.IO;
using Debug = UnityEngine.Debug;

namespace AAI.VDTSimulator.ImageSegmentation
{
	public static class DecompressorExecutableManager
	{
		public static void CopyExecutable(string destinationDirectory)
		{
			try
			{
				// TODO: Fix the directory once the project in build automatically - Risul
				Debug.Log($"[{nameof(DecompressorExecutableManager)}] - Saving exe files to directory {destinationDirectory}");
				Process.Start(destinationDirectory);
				var sourceDirectoryPath = Path.Combine("vdt_Data", "Tools", "ImageSegmentationDecompressor");
				if (Directory.Exists(sourceDirectoryPath) == false)
				{
					Debug.LogWarning($"[{nameof(DecompressorExecutableManager)}] - The Folder for Decompressor is not created yet. Run Build Script to create Decompressor");
					return;
				}

				var sourceFilePaths = Directory.GetFiles(sourceDirectoryPath);
				foreach (var sourceFilePath in sourceFilePaths)
				{
					var fileName = Path.GetFileName(sourceFilePath);
					var destinationFilePath = Path.Combine(destinationDirectory, fileName);
					File.Copy(sourceFilePath, destinationFilePath);
				}

				Process.Start(destinationDirectory);
				RunExeFile(destinationDirectory);
			}
			catch (Exception e)
			{
				Debug.Log($"[{nameof(DecompressorExecutableManager)}] - Exception occured during copying Decompressor file {e.StackTrace}");
				throw;
			}
		}

		private static void RunExeFile(string destinationDirectory)
		{
			var processStartInfo = new ProcessStartInfo()
			{
				FileName = "ImageSegmentationDecompressor.exe",
				CreateNoWindow = true
			};

			var currentDirectoryBackup = Environment.CurrentDirectory;
			Environment.CurrentDirectory = destinationDirectory;
			Process.Start(processStartInfo);
			Environment.CurrentDirectory = currentDirectoryBackup;
		}
	}
}
