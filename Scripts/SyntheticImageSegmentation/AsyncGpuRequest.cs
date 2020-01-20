using UnityEngine;
using UnityEngine.Rendering;

namespace AAI.VDTSimulator.ImageSegmentation
{
	public struct AsyncGpuRequest
	{
		public AsyncGPUReadbackRequest RgbImageRequest;
		public RenderTexture RgbRenderTexture;

		public AsyncGPUReadbackRequest SegmentedRequest;
		public RenderTexture SegmentedRenderTexture;

		public AsyncGPUReadbackRequest DepthRequest;
		public RenderTexture DepthRenderTexture;

		public int TimeStamp;
	}
}