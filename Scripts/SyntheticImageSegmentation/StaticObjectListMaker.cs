using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace AAI.VDTSimulator.ImageSegmentation
{
	public class StaticObjectListMaker
	{
		[Inject] private SyntheticObjectListMaker _syntheticObjectListMaker;
		[Inject] private SegmentedTagsManager _segmentedSegmentedTagsManager;
		[Inject] private RenderStatusManager _renderStatusManager;

		// List to hold all rendered object in the scene with RenderStatus script attached
		private List<RenderStatus> _staticObjectRenderStatusList = new List<RenderStatus>();

		public void ActivateRenderStatusOnObjects()
		{
			_staticObjectRenderStatusList.Clear();

			foreach (var tag in _segmentedSegmentedTagsManager.StaticTagToIndexDictionary)
			{
				var SegmentedObjectParent = GameObject.FindGameObjectsWithTag(tag.Key);
				foreach (var segmentedObject in SegmentedObjectParent)
				{
					_staticObjectRenderStatusList.AddRange(_renderStatusManager.AddRenderStatus(segmentedObject, RendererType.Static));
				}
			}
		}

		public void StopSegmentation()
		{
			foreach (var renderStatus in _staticObjectRenderStatusList)
			{
				renderStatus.enabled = false;
			}

			_staticObjectRenderStatusList.Clear();
		}
	}
}