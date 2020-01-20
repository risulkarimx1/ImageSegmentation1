using UnityEngine;
using UnityEngine.Experimental.Rendering;

namespace AAI.VDTSimulator.ImageSegmentation
{
	public class RgbDepthImageMaker : MonoBehaviour
	{
		private AsyncImageSaver _asyncImageSaver;
		private SegmentedImageSetup _segmentedImageSetup;

		public void Initialize(AsyncImageSaver asyncImageSaver, SegmentedImageSetup segmentedImageSetup)
		{
			_asyncImageSaver = asyncImageSaver;
			_segmentedImageSetup = segmentedImageSetup;
		}

		void OnRenderImage(RenderTexture source, RenderTexture destination)
		{
			Graphics.Blit(source, (RenderTexture)null);
			if (_asyncImageSaver == null)
				return;
			var rgbRt = _asyncImageSaver.GetTemporaryRenderTexture(_segmentedImageSetup.ImageWidth,
				_segmentedImageSetup.ImageHeight, source.depth, GraphicsFormat.R8G8B8A8_SRGB);

			Graphics.Blit(source, rgbRt);
			var depthRT = _asyncImageSaver.GetTemporaryRenderTexture(_segmentedImageSetup.ImageWidth, 
				_segmentedImageSetup.ImageHeight, source.depth, GraphicsFormat.R8G8B8A8_SRGB);

			Graphics.Blit(source, depthRT, _segmentedImageSetup.DepthRenderMaterial);
			_asyncImageSaver.SetRgbAndDepthRenderTexture(rgbRt, depthRT);
		}
	}
}