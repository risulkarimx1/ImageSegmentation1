using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using Zenject;

namespace AAI.VDTSimulator.ImageSegmentation
{
	[CreateAssetMenu(fileName = "Tags Color Table", menuName = "Image Segmentation/Tag Color Table", order = 1)]
	public class SegmentedTagsManager : ScriptableObject, IInitializable
	{
		[SerializeField] private List<TagColor> _staticObjctsTagColorList;
		[SerializeField] private List<TagColor> _dynamicObjectsTagColorList;
		private int _tagsCount;
		public Dictionary<string, int> StaticTagToIndexDictionary { get; } = new Dictionary<string, int>();
		public Dictionary<string, int> DynamicTagToIndexDictionary { get; } = new Dictionary<string, int>();
		public Texture2D TagColorTexture { get; private set; }

		public int TagsCount => _tagsCount;
		
		public void Initialize()
		{
			StaticTagToIndexDictionary.Clear();
			DynamicTagToIndexDictionary.Clear();
			_tagsCount = 1;
			foreach (var tagsList in _staticObjctsTagColorList)
			{
				StaticTagToIndexDictionary.Add(tagsList.Tag, _tagsCount++);
			}

			foreach (var tagsList in _dynamicObjectsTagColorList)
			{
				DynamicTagToIndexDictionary.Add(tagsList.Tag, _tagsCount++);
			}

			CreateTexture();
		}

		private void CreateTexture()
		{
			TagColorTexture = new Texture2D(TagsCount, 1, TextureFormat.ARGB32, false)
			{
				filterMode = FilterMode.Point,
				wrapMode = TextureWrapMode.Repeat
			};

			var pixelIndex = 0;
			for (var i = 0; i < _staticObjctsTagColorList.Count; i++)
			{
				TagColorTexture.SetPixel(pixelIndex++, 0, _staticObjctsTagColorList[i].Color);
			}

			for (int i = 0; i < _dynamicObjectsTagColorList.Count; i++)
			{
				TagColorTexture.SetPixel(pixelIndex++, 0, _dynamicObjectsTagColorList[i].Color);
			}

			TagColorTexture.Apply(false);
		}

	}
}