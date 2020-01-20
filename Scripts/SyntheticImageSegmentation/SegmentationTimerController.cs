using System;
using System.Collections;
using UniRx;
using UnityEngine;
using Zenject;

namespace AAI.VDTSimulator.ImageSegmentation
{
	public class SegmentationTimerController : IInitializable, ITickable, IDisposable
	{
		private RgbDepthImageMaker _rgbDepthImageMaker;
		private SegmentedImageMaker _segmentedImageMaker;

		private bool _segmentationRunning;
		private float _segmentationStartingTime;
		private bool _isFirstCaptureRequest;
		private bool _takeSnap;
		private float _lastCaptureTime;

		[Inject(Id = "MainCamera")] private Camera _camera;
		[Inject] private AsyncImageSaver _asyncImageSaver;
		[Inject] private SegmentedImageSetup _segmentedImageSetup;
		[Inject] private StaticObjectListMaker _staticObjectListMaker;
		[Inject] private DynamicObjectListMaker _dynamicObjectListMaker;
		private IDisposable _startSegmentationCoRoutineDisposible;

		public void Initialize()
		{
			_rgbDepthImageMaker = _camera.gameObject.GetComponent<RgbDepthImageMaker>();
			_segmentedImageMaker = _camera.gameObject.GetComponent<SegmentedImageMaker>();

			_isFirstCaptureRequest = false;
		}

		public void Tick()
		{
			if (_takeSnap)
			{
				if (_isFirstCaptureRequest) // Required to make the first snap time 0
				{
					_segmentationStartingTime = Time.time;
					_isFirstCaptureRequest = false;
				}

				_asyncImageSaver.CreateRequest((int)((Time.time - _segmentationStartingTime) * 1000));
				_takeSnap = false;
				_rgbDepthImageMaker.enabled = false;
				_segmentedImageMaker.enabled = false;
			}

			if (_segmentationRunning)
			{
				if ((_segmentedImageSetup.ImageWidth != _camera.pixelWidth) ||
				    (_segmentedImageSetup.ImageHeight != _camera.pixelHeight))
				{
					Debug.LogWarning($"[{nameof(SegmentationTimerController)}] - Detected change in 3D Client's resolution. Segmentation was stopped");
					StopSegmentation();
					return;
				}

				if (Time.time - _lastCaptureTime > AppConfiguration.Configuration.ImageSegmentationConfig.Interval)
				{
					_takeSnap = true;

					_rgbDepthImageMaker.enabled = true;
					_segmentedImageMaker.enabled = true;

					_lastCaptureTime = Time.time;
				}
			}
		}

		public void StopSegmentation()
		{
			if (_segmentationRunning == false)
				return;

			_segmentationRunning = false;
			_asyncImageSaver.StopSegmentation();
			_staticObjectListMaker.StopSegmentation();
			_dynamicObjectListMaker.StopSegmentation();
		}

		public void StartSegmentation()
		{
			if (_segmentationRunning)
				return;

			_startSegmentationCoRoutineDisposible = Observable.FromCoroutine(StartSegmentationAsync).Subscribe();
		}

		private IEnumerator StartSegmentationAsync()
		{
			_camera.enabled = false;
			_staticObjectListMaker.ActivateRenderStatusOnObjects();
			_dynamicObjectListMaker.ActivateRenderStatusOnObjects();
			yield return new WaitForSeconds(1);
			_camera.enabled = true;

			_segmentationRunning = true;
			_isFirstCaptureRequest = true;
			_asyncImageSaver.StartSegmentation();
		}

		public void Dispose()
		{
			if (_segmentationRunning)
				StopSegmentation();

			_startSegmentationCoRoutineDisposible?.Dispose();
		}
	}
}