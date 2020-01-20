using UnityEngine;

namespace AAI.VDTSimulator.ImageSegmentation
{
	public class RenderStatus : MonoBehaviour
	{
		public SyntheticObjectListMaker _syntheticObjectListMaker;
		private MeshFilter _meshFilter;
		private RendererType _rendererType;

		public void SetVisibleRendererList(SyntheticObjectListMaker syntheticObjectListMaker, RendererType rendererType)
		{
			_syntheticObjectListMaker = syntheticObjectListMaker;
			_meshFilter = GetComponent<MeshFilter>();
			_rendererType = rendererType;
		}

		private void OnBecameVisible()
		{
			_syntheticObjectListMaker.AddRenderer(_meshFilter, _rendererType);
		}

		private void OnBecameInvisible()
		{
			_syntheticObjectListMaker.RemoveRenderer(_meshFilter);
		}
	}
}