using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Assets.Scripts.Utility;
using MessagePack.LZ4;
using UniRx.Async;
using Unity.Collections;
using UnityEngine;
using Zenject;

namespace AAI.VDTSimulator.ImageSegmentation
{
	public class ImageDataFileWriter : ITickable, IDisposable
	{
		private List<CompressionJob> _compressionJobsList = new List<CompressionJob>();
		private CancellationTokenSource _compressionJobCancellationToken;

		private ConcurrentQueue<CompressionJobResult> _compressionJobsConcurrentQueue = new ConcurrentQueue<CompressionJobResult>();

		private string _directoryPath;
		[Inject] private SegmentedImageSetup _segmentedImageSetup;

		public void StartSegmentation()
		{
			CreateNewDirectory();
			UniTask.Run(SaveFileAsync);
		}

		private void CreateNewDirectory()
		{
			_compressionJobCancellationToken = new CancellationTokenSource();
			_directoryPath = GetNewSessionPath();

			if (Directory.Exists(_directoryPath) == false)
				Directory.CreateDirectory(_directoryPath);

			Debug.Log($"[{nameof(ImageDataFileWriter)}] - New Session has been created at {_directoryPath}"); // TODO: Remove this line after development is done - Risul
		}

		private string GetNewSessionPath()
		{
			var sessionNumber = PlayerPrefs.GetInt(Constants.ImageSegmentationSessionCounter);
			var sessionFilePath = $"{Application.persistentDataPath}" +
								  $"/Synthetic Image Segmentation" +
								  $"/{DateTime.Now.Day}.{DateTime.Now.Month}.{DateTime.Now.Year}-" +
								  $"{DateTime.Now.Hour}.{DateTime.Now.Minute}.{DateTime.Now.Second}-" +
								  $"{sessionNumber}";
			PlayerPrefs.SetInt(Constants.ImageSegmentationSessionCounter, sessionNumber + 1);
			return sessionFilePath;
		}

		public void AddImageData(NativeArray<byte> inputBuffer, int timeStamp, SegmentedImageType segmentedImageType)
		{
			var inputBufferLength = inputBuffer.Length;
			var outputBuffer = new NativeArray<byte>(inputBuffer.Length * 2, Allocator.TempJob);
			var compressedSizeHolder = new NativeArray<int>(1, Allocator.TempJob);

			var encoderJobHandle = LZ4Codec.EncodeBurst(inputBuffer, outputBuffer, compressedSizeHolder);
			_compressionJobsList.Add(new CompressionJob()
			{
				InputBuffer = inputBuffer,
				CompressedData = outputBuffer,
				UncompressedLength = inputBufferLength,
				JobHandle = encoderJobHandle,
				CompressedLength = compressedSizeHolder,
				TimeStamp = timeStamp,
				SegmentedImageType = segmentedImageType
			});
		}

		public void Tick()
		{
			for (var i = 0; i < _compressionJobsList.Count; i++)
			{
				if (_compressionJobsList[i].JobHandle.IsCompleted == false)
					continue;

				_compressionJobsList[i].JobHandle.Complete();
				var compressionJob = _compressionJobsList[i];
				var compressionJobResult = new CompressionJobResult()
				{
					UncompressedLength = compressionJob.UncompressedLength,
					TimeStamp = compressionJob.TimeStamp,
					SegmentedImageType = compressionJob.SegmentedImageType,
					CompressedData = new NativeSlice<byte>(compressionJob.CompressedData, 0, compressionJob.CompressedLength[0]).ToArray()
				};
				_compressionJobsConcurrentQueue.Enqueue(compressionJobResult);

				compressionJob.CompressedLength.Dispose();
				compressionJob.CompressedData.Dispose();
				_compressionJobsList.RemoveAt(i);
			}
		}

		private void SaveFileAsync()
		{
			var fileNameCounter = 1;
			using (var memoryStream = new MemoryStream())
			using (var writer = new BinaryWriter(memoryStream))
			{
				byte[] buffer;
				while (true)
				{
					if (_compressionJobCancellationToken.IsCancellationRequested)
						break;

					if (_compressionJobsConcurrentQueue.Count == 0)
					{
						Thread.Sleep(10);
						continue;
					}

					if (!_compressionJobsConcurrentQueue.TryDequeue(out var compressionJob))
						continue;

					writer.Write(compressionJob.UncompressedLength);
					writer.Write(compressionJob.CompressedData.Length);
					writer.Write((int)compressionJob.SegmentedImageType);
					writer.Write(compressionJob.TimeStamp);
					writer.Write(_segmentedImageSetup.ImageWidth);
					writer.Write(_segmentedImageSetup.ImageHeight);
					writer.Write(compressionJob.CompressedData);

					if (memoryStream.Position <= _segmentedImageSetup.FileSizeInMb * 1_000_000)
						continue;

					buffer = new byte[memoryStream.Position];
					Array.Copy(memoryStream.GetBuffer(), 0, buffer, 0, memoryStream.Position);
					File.WriteAllBytes(Path.Combine(_directoryPath, $"Data_{fileNameCounter++}.bin"), buffer);
					memoryStream.Seek(0, SeekOrigin.Begin);
				}

				buffer = new byte[memoryStream.Position];
				Array.Copy(memoryStream.GetBuffer(), 0, buffer, 0, memoryStream.Position);
				File.WriteAllBytes(Path.Combine(_directoryPath, $"Data_{fileNameCounter++}.bin"), buffer);
			}
		}

		public void StopSegmenting()
		{
			_compressionJobCancellationToken?.Cancel();
			DecompressorExecutableManager.CopyExecutable(_directoryPath);
		}

		public void Dispose()
		{
			_compressionJobCancellationToken?.Dispose();
		}
	}
}