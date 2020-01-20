using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace AAI.VDTSimulator.ImageSegmentation
{
	public class RenderStatusManager
	{
		[Inject] private SyntheticObjectListMaker _syntheticObjectListMaker;

		public List<RenderStatus> AddRenderStatus(GameObject actorObject, RendererType rendererType)
		{
			var renderStatusList = new List<RenderStatus>();

			var meshRenderers = actorObject.GetComponentsInChildren<MeshRenderer>();

			foreach (var meshRenderer in meshRenderers)
			{
				if (meshRenderer.GetComponent<RenderStatus>() == null)
				{
					var renderStatus = meshRenderer.gameObject.AddComponent<RenderStatus>();
					renderStatus.SetVisibleRendererList(_syntheticObjectListMaker, rendererType);
					renderStatusList.Add(renderStatus);
				}
				else if (meshRenderer.GetComponent<RenderStatus>().enabled == false)
				{
					var renderStatus = meshRenderer.GetComponent<RenderStatus>();
					renderStatus.enabled = true;
					renderStatusList.Add(renderStatus);
				}
			}

			return renderStatusList;
		}

		public void DisableRenderStatusComponent(GameObject actorObject)
		{
			var renderStatusComponents = actorObject.GetComponentsInChildren<RenderStatus>();
			foreach (var renderStatusComponent in renderStatusComponents)
				renderStatusComponent.enabled = false;
		}
	}
}