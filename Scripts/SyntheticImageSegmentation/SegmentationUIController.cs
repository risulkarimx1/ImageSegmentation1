using UnityEngine;
using Zenject;

namespace AAI.VDTSimulator.ImageSegmentation
{
	public class SegmentationUIController : ITickable
	{
		[Inject] private SegmentationTimerController _segmentationTimerController;
		[Inject] private SegmentationCameraSetup _segmentationCameraSetup;
		public void Tick()
		{
			if ((Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) && Input.GetKeyDown(KeyCode.I))
			{
				_segmentationCameraSetup.SetImageResolution();
				_segmentationTimerController.StartSegmentation();
			}

			if ((Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) && Input.GetKeyDown(KeyCode.O))
			{
				_segmentationTimerController.StopSegmentation();
			}
		}
	}
}