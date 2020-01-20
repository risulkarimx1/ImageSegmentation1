using UnityEngine;

namespace AAI.VDTSimulator.ImageSegmentation
{
	[CreateAssetMenu(fileName = "Segmented Image Settings", menuName = "Image Segmentation/Segmented Image Settings", order = 1)]
	public class SegmentedImageSetup : ScriptableObject
	{
		[Header("Materials with shader to process segmented images")]
		[SerializeField] private Material _segmentedOutputMaterial;
		[SerializeField] private Material _objectListMaterial;

		[Header("Materials with shader to process Depth")]
		[SerializeField] private Material _depthRenderMaterial;

		[Header("Debug Objects")]
		[SerializeField] private bool _debugMode;

		// TODO: Remove the debugger code if segmentation is done or its not needed to show segmentation in real time - Risul
		[Header("Segmentation Debugger")]
		[SerializeField] private Material _segmentedOutputDebugMaterial;
		[SerializeField] private GameObject _segmentedImageDebugger;

		[Header("Depth Debugger")]
		[SerializeField] private Material _depthOutputDebugMaterial;
		[SerializeField] private GameObject _depthImageDebugger;
		public Material ObjectListMaterial => _objectListMaterial;
		public Material SegmentedOutputMaterial => _segmentedOutputMaterial;
		public Material DepthRenderMaterial => _depthRenderMaterial;

		// Screen Resolution
		public int ImageWidth { get; set; }
		public int ImageHeight { get; set; }

		[Header("File Size in MB")]
		[SerializeField] private int _fileSizeInMb;
		public int FileSizeInMb => _fileSizeInMb;

		// TODO: Remove the debugger code if segmentation is done or its not needed to show segmentation in real time - Risul
		public bool DebugMode => _debugMode;
		public Material SegmentedOutputDebugMaterial => _segmentedOutputDebugMaterial;
		public Material DepthOutputDebugMaterial => _depthOutputDebugMaterial;
		public GameObject SegmentedImageDebugger => _segmentedImageDebugger;
		public GameObject DepthImageDebugger => _depthImageDebugger;
	}
}