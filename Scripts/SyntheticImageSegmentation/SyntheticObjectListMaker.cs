using System.Collections.Generic;
using Assets.Scripts.Utility;
using UnityEngine;
using Zenject;

namespace AAI.VDTSimulator.ImageSegmentation
{
	public class SyntheticObjectListMaker
	{
		[Inject] private SegmentedTagsManager _segmentedSegmentedTagsManager;

		private Dictionary<int, MeshFilter> _renderedObjectDictionary = new Dictionary<int, MeshFilter>();

		// For each MeshFilter to Tag (in number) dictionary
		public Dictionary<MeshFilter, int> MeshFilterToTagsMap { get; } = new Dictionary<MeshFilter, int>();

		public void AddRenderer(MeshFilter meshFilter, RendererType rendererType)
		{
			var key = meshFilter.gameObject.GetInstanceID();
			if (_renderedObjectDictionary.ContainsKey(key) == false)
			{
				var tag = "";
				switch (rendererType)
				{
					case RendererType.Actor:
						tag = Constants.Actor;
						break;
					case RendererType.Ego:
						tag = Constants.Ego;
						break;
					case RendererType.Static:
						tag = FindNearestTag(meshFilter);
						break;
					default:
						tag = "Untagged";
						break;
				}

				if (tag == "Untagged")
					return;

				if (_segmentedSegmentedTagsManager.StaticTagToIndexDictionary.ContainsKey(tag) == true)
				{
					MeshFilterToTagsMap.Add(meshFilter, _segmentedSegmentedTagsManager.StaticTagToIndexDictionary[tag]);
				}
				else if (_segmentedSegmentedTagsManager.DynamicTagToIndexDictionary.ContainsKey(tag) == true)
				{
					MeshFilterToTagsMap.Add(meshFilter,
						_segmentedSegmentedTagsManager.DynamicTagToIndexDictionary[tag]);
				}
				else
				{
					Debug.LogWarning(
						$"[{nameof(SyntheticObjectListMaker)}] - The Object {meshFilter.gameObject} is not properly tagged");
					return;
				}

				_renderedObjectDictionary.Add(key, meshFilter);
			}
		}

		public void RemoveRenderer(MeshFilter meshFilter)
		{
			var key = meshFilter.gameObject.GetInstanceID();
			if (!_renderedObjectDictionary.ContainsKey(key))
				return;

			MeshFilterToTagsMap.Remove(meshFilter);
			_renderedObjectDictionary.Remove(key);
		}

		private string FindNearestTag(MeshFilter meshFilter)
		{
			if (meshFilter.CompareTag("Untagged") == false)
				return meshFilter.tag;

			var currentObject = meshFilter.transform.parent;
			while (currentObject.parent != null)
			{
				if (currentObject.CompareTag("Untagged") == false)
					return currentObject.tag;
				currentObject = currentObject.parent;
			}

			return currentObject.tag;
		}
	}
}