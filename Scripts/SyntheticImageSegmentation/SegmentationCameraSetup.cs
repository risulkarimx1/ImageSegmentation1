using UnityEngine;
using Zenject;

namespace AAI.VDTSimulator.ImageSegmentation
{
	public class SegmentationCameraSetup : IInitializable
	{
		[Inject(Id = "MainCamera")] private Camera _camera;
		[Inject] private AsyncImageSaver _asyncImageSaver;
		[Inject] private SegmentedImageSetup _segmentedImageSetup;
		[Inject] private SegmentedTagsManager _segmentedTagsManager;
		[Inject] private SyntheticObjectListMaker _syntheticObjectListMaker;

		public void Initialize()
		{
			SetImageResolution();

			_camera.depthTextureMode = DepthTextureMode.Depth;
			var rgbDepthImageMaker = _camera.gameObject.AddComponent<RgbDepthImageMaker>();
			var segmentedImageMaker = _camera.gameObject.AddComponent<SegmentedImageMaker>();

			rgbDepthImageMaker.Initialize(_asyncImageSaver, _segmentedImageSetup);
			segmentedImageMaker.Initialize(_asyncImageSaver, _segmentedImageSetup, _segmentedTagsManager, _camera, _syntheticObjectListMaker);

			rgbDepthImageMaker.enabled = false;
			segmentedImageMaker.enabled = false;
		}

		public void SetImageResolution()
		{
			_segmentedImageSetup.ImageWidth = _camera.pixelWidth;
			_segmentedImageSetup.ImageHeight = _camera.pixelHeight;
		}
	}
}