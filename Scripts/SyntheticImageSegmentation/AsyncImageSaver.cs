using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Profiling;
using UnityEngine.Rendering;
using Zenject;

namespace AAI.VDTSimulator.ImageSegmentation
{
	public class AsyncImageSaver : MonoBehaviour
	{
		private RenderTexture _rgbRenderTexture;
		private RenderTexture _segmentedRenderTexture;
		private RenderTexture _depthRenderTexture;

		private Queue<AsyncGpuRequest> _asyncGpuReadbackQueue = new Queue<AsyncGpuRequest>();
		private List<RenderTexture> _temporaryRenderTextures = new List<RenderTexture>();

		[Inject] private ImageDataFileWriter _imageDataFileWriter;
		[Inject] private SegmentedImageSetup _segmentedImageSetup;

		private void Start()
		{
#if UNITY_EDITOR
			// TODO: DDD-1473 Remove the debugger when its done - Risul
			if (_segmentedImageSetup.DebugMode)
			{
				Instantiate(_segmentedImageSetup.SegmentedImageDebugger);
				Instantiate(_segmentedImageSetup.DepthImageDebugger);
			}
#endif
		}
		
		private void Update()
		{
			while (_asyncGpuReadbackQueue.Count > 0)
			{
				var request = _asyncGpuReadbackQueue.Peek();
				if (request.RgbImageRequest.hasError || request.SegmentedRequest.hasError || request.DepthRequest.hasError)
				{
					Debug.LogError($"[{nameof(AsyncImageSaver)}] - GPU Readback error detected");
					_asyncGpuReadbackQueue.Dequeue();
				}
				else if (request.RgbImageRequest.done && request.SegmentedRequest.done && request.DepthRequest.done)
				{
					Profiler.BeginSample("Segmentation - Peeking request");

					var rgbOutputBuffer = request.RgbImageRequest.GetData<byte>();
					var segmentedOutputBuffer = request.SegmentedRequest.GetData<byte>();
					var depthOutputBuffer = request.DepthRequest.GetData<byte>();

					RemoveTemporaryRenderTexture(request.RgbRenderTexture);
					RemoveTemporaryRenderTexture(request.SegmentedRenderTexture);
					RemoveTemporaryRenderTexture(request.DepthRenderTexture);

					_imageDataFileWriter.AddImageData(rgbOutputBuffer, request.TimeStamp, SegmentedImageType.RGB);
					_imageDataFileWriter.AddImageData(segmentedOutputBuffer, request.TimeStamp, SegmentedImageType.Segmented);
					_imageDataFileWriter.AddImageData(depthOutputBuffer, request.TimeStamp, SegmentedImageType.Depth);

					_asyncGpuReadbackQueue.Dequeue();
					Profiler.EndSample();
				}
				else
				{
					break;
				}
			}
		}

		private void OnDestroy()
		{
			foreach (var temporaryRenderTexture in _temporaryRenderTextures)
			{
				temporaryRenderTexture.Release();
			}
		}

		private void RemoveTemporaryRenderTexture(RenderTexture renderTexture)
		{
			_temporaryRenderTextures.Remove(renderTexture);
			RenderTexture.ReleaseTemporary(renderTexture);
		}

		public RenderTexture GetTemporaryRenderTexture(int width, int height, int depth, GraphicsFormat graphicsFormat)
		{
			var tempRenderTexture = RenderTexture.GetTemporary(width, height, depth, graphicsFormat);
			_temporaryRenderTextures.Add(tempRenderTexture);
			return tempRenderTexture;
		}

		public void CreateRequest(int timeStamp)
		{
			if (_asyncGpuReadbackQueue.Count >= 8)
			{
				Debug.LogWarning($"[{nameof(AsyncImageSaver)}] -  Too many requests");
				return;
			}

			Profiler.BeginSample("Segmentation- Creating Request");
			_asyncGpuReadbackQueue.Enqueue(new AsyncGpuRequest
			{
				RgbImageRequest = AsyncGPUReadback.Request(_rgbRenderTexture),
				RgbRenderTexture = _rgbRenderTexture,

				SegmentedRequest = AsyncGPUReadback.Request(_segmentedRenderTexture),
				SegmentedRenderTexture = _segmentedRenderTexture,

				DepthRequest = AsyncGPUReadback.Request(_depthRenderTexture),
				DepthRenderTexture = _depthRenderTexture,

				TimeStamp = timeStamp
			});
			Profiler.EndSample();
		}
		public void StartSegmentation()
		{
			_imageDataFileWriter.StartSegmentation();
		}

		public void StopSegmentation()
		{
			_imageDataFileWriter.StopSegmenting();
		}

		public void SetRgbAndDepthRenderTexture(RenderTexture rgbRenderTexture, RenderTexture depthRenderTexture) // OnPostRenderImage
		{
			_rgbRenderTexture = rgbRenderTexture;
			_depthRenderTexture = depthRenderTexture;
#if UNITY_EDITOR
			// TODO: DDD-1473 Remove the debugger when segmentation is fully implemented - Risul
			if (_segmentedImageSetup.DebugMode)
				_segmentedImageSetup.DepthOutputDebugMaterial.mainTexture = depthRenderTexture;
#endif
		}

		public void SetSegmentedRenderTexture(RenderTexture rt) // OnRenderImage
		{
			_segmentedRenderTexture = rt;
#if UNITY_EDITOR
			// TODO: DDD-1473 Remove the debugger when its done - Risul
			if (_segmentedImageSetup.DebugMode)
				_segmentedImageSetup.SegmentedOutputDebugMaterial.mainTexture = rt;
#endif
		}
	}
}