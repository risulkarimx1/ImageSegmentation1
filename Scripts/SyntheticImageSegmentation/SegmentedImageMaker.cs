using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;

namespace AAI.VDTSimulator.ImageSegmentation
{
	public class SegmentedImageMaker : MonoBehaviour
	{
		// Command buffer variables
		private int _objectId;
		private CommandBuffer _commandBuffer;
		private RenderTargetIdentifier _indexBufferRenderTargetIdentifier;

		private SyntheticObjectListMaker _syntheticObjectListMaker;
		private Camera _camera;
		private SegmentedTagsManager _segmentedSegmentedTagsManager;
		private AsyncImageSaver _asyncImageSaver;
		private SegmentedImageSetup _segmentedImageSetup;

		public void Initialize(AsyncImageSaver asyncImageSaver, SegmentedImageSetup segmentedImageSetup,
			SegmentedTagsManager segmentedTagsManager, Camera camera, SyntheticObjectListMaker syntheticObjectListMaker)
		{
			_camera = camera;
			_syntheticObjectListMaker = syntheticObjectListMaker;
			_segmentedSegmentedTagsManager = segmentedTagsManager;
			_segmentedImageSetup = segmentedImageSetup;
			_asyncImageSaver = asyncImageSaver;
			_objectId = Shader.PropertyToID("_ObjectId");
			Shader.PropertyToID(nameof(SegmentedImageMaker));

			_segmentedImageSetup.SegmentedOutputMaterial.SetFloat("_NumberOfSegments", _segmentedSegmentedTagsManager.TagsCount);
			_segmentedImageSetup.SegmentedOutputMaterial.SetTexture("_TagLookUp", _segmentedSegmentedTagsManager.TagColorTexture);

			// Command buffer initialization
			var renderTexture = RenderTexture.GetTemporary(_segmentedImageSetup.ImageWidth, _segmentedImageSetup.ImageHeight, 8, RenderTextureFormat.R8);
			renderTexture.filterMode = FilterMode.Point;
			renderTexture.useMipMap = false;
			_indexBufferRenderTargetIdentifier = new RenderTargetIdentifier(renderTexture);

			_segmentedImageSetup.SegmentedOutputMaterial.SetTexture("_IndexedTexture", renderTexture);

			_commandBuffer = new CommandBuffer
			{
				name = "Visible Object Indexing Command Buffer"
			};
			_camera.AddCommandBuffer(CameraEvent.AfterSkybox, _commandBuffer);
		}

		void LateUpdate()
		{
			if (_asyncImageSaver == null)
				return;
			// Filling up Command buffer 
			_commandBuffer.Clear();
			_commandBuffer.SetRenderTarget(_indexBufferRenderTargetIdentifier);
			_commandBuffer.ClearRenderTarget(true, true, new Color(0, 0, 0, 0));

			foreach (var meshFilter in _syntheticObjectListMaker.MeshFilterToTagsMap)
			{
				var objectId = meshFilter.Value;
				_commandBuffer.SetGlobalFloat(_objectId, objectId * 1.0f / 255.0f);

				for (int i = 0; i < meshFilter.Key.mesh.subMeshCount; i++)
				{
					_commandBuffer.DrawRenderer(meshFilter.Key.GetComponent<Renderer>(), _segmentedImageSetup.ObjectListMaterial, i);
				}
			}
		}

		void OnPostRender()
		{
			if (_asyncImageSaver == null)
				return;
			// Draw full Screen Quad
			var outputRenderTexture = _asyncImageSaver.GetTemporaryRenderTexture( _segmentedImageSetup.ImageWidth, _segmentedImageSetup.ImageHeight, 24, GraphicsFormat.R8G8B8A8_SRGB);

			var previousRenderTarget = RenderTexture.active;
			RenderTexture.active = outputRenderTexture;

			GL.Clear(true, true, new Color(0, 0, 0, 0));
			GL.PushMatrix();
			_segmentedImageSetup.SegmentedOutputMaterial.SetPass(0);
			GL.LoadOrtho();

			GL.Begin(GL.QUADS); // Quad

			GL.TexCoord2(0, 0);
			GL.Vertex3(0f, 0f, 0);

			GL.TexCoord2(0, 1);
			GL.Vertex3(0f, 1f, 0);

			GL.TexCoord2(1, 1);
			GL.Vertex3(1f, 1f, 0);

			GL.TexCoord2(1, 0);
			GL.Vertex3(1f, 0f, 0);

			GL.End();

			GL.PopMatrix();

			_asyncImageSaver.SetSegmentedRenderTexture(outputRenderTexture);
			RenderTexture.active = previousRenderTarget;
		}

		private void OnDisable()
		{
			_commandBuffer.Clear();
		}

		private void OnDestroy()
		{
			_segmentedImageSetup.SegmentedOutputMaterial.SetTexture("_IndexedTexture", null);
			_commandBuffer.Dispose();
		}
	}
}