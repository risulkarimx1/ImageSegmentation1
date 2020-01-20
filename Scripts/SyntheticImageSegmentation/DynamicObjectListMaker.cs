using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace AAI.VDTSimulator.ImageSegmentation
{
	public class DynamicObjectListMaker : IInitializable, ILateDisposable
	{
		[Inject] private SignalBus _signalBus;
		[Inject] private SyntheticObjectListMaker _syntheticObjectListMaker;
		[Inject] private ActorsManager _actorsManager;
		[Inject] private RenderStatusManager _renderStatusManager;

		private Dictionary<int, GameObject> _activeActorsDictionary;
		private bool _segmentationRunning;
		private GameObject _egoCar;

		public void Initialize()
		{
			_actorsManager.ActorAdded += ActorAdded;
			_actorsManager.ActorRemoved += ActorRemoved;
			_signalBus?.Subscribe<EgoJoinedSignal>(OnEgoJoined);
			_segmentationRunning = false;
			_activeActorsDictionary = new Dictionary<int, GameObject>();
		}

		private void OnEgoJoined(EgoJoinedSignal egoCar)
		{
			_egoCar = egoCar.VehicleCockpit.gameObject;
		}

		private void ActorAdded(GameObject actorObject)
		{
			// if not running, keep filling up the dictionary with Actors, the Add RenderStatus when StartSegmentation is called
			if (_segmentationRunning == false)
			{
				if (_activeActorsDictionary.ContainsKey(actorObject.GetInstanceID()) == false)
					_activeActorsDictionary.Add(actorObject.GetInstanceID(), actorObject);
			}
			else
			{
				// If segmentation running, assign RenderStatus right away and add to the list
				_renderStatusManager.AddRenderStatus(actorObject, RendererType.Actor);
				_activeActorsDictionary.Add(actorObject.GetInstanceID(), actorObject);
			}
		}

		private void ActorRemoved(GameObject actorObject)
		{
			if (_activeActorsDictionary.ContainsKey(actorObject.GetInstanceID()) == true)
				_activeActorsDictionary.Remove(actorObject.GetInstanceID());
		}

		public void ActivateRenderStatusOnObjects()
		{
			foreach (var actorObject in _activeActorsDictionary)
				_renderStatusManager.AddRenderStatus(actorObject.Value, RendererType.Actor);

			_renderStatusManager.AddRenderStatus(_egoCar, RendererType.Ego);

			_segmentationRunning = true;
		}

		public void StopSegmentation()
		{
			foreach (var actorObject in _activeActorsDictionary)
				_renderStatusManager.DisableRenderStatusComponent(actorObject.Value);

			_renderStatusManager.DisableRenderStatusComponent(_egoCar);

			_segmentationRunning = false;
		}

		public void LateDispose()
		{
			_actorsManager.ActorAdded -= ActorAdded;
			_actorsManager.ActorRemoved -= ActorRemoved;
			_signalBus.Unsubscribe<EgoJoinedSignal>(OnEgoJoined);
		}
	}
}