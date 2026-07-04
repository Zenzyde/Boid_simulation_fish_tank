using System.Collections;
using System.Collections.Generic;
using System.Timers;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.PlayerLoop;

public class CamObserve : MonoBehaviour
{
	[SerializeField] private float rotationSpeed, obervationDistance, yAxisOffset, minZoom, maxZoom;
	[SerializeField] private ObservationAxis observationAxis;
	[SerializeField] private Transform observationPivot;
	[SerializeField] private InputAction toggleAutoRotateAction;

	bool noPivot = false;
	bool autoRotate = false;
	Camera camera;

	private Transform observationPivotCache;

	void Awake()
	{
		if (observationPivot == null)
		{
			Debug.LogError("No obervation pivot for observation camera! Assign a transform for use as pivot for observation camera!");
			noPivot = true;
			return;
		}

		transform.position = observationPivot.position;
		transform.rotation = Quaternion.LookRotation((observationPivot.position - transform.position).normalized);
		transform.position -= transform.forward * obervationDistance;
		transform.position += Vector3.up * yAxisOffset;
		transform.rotation = Quaternion.LookRotation((observationPivot.position - transform.position).normalized);
		transform.SetParent(observationPivot);
		camera = GetComponent<Camera>();
	}

	void Start()
	{
		if (toggleAutoRotateAction != null)
			toggleAutoRotateAction.Enable();
	}

	void OnEnable()
	{
		if (toggleAutoRotateAction != null)
			toggleAutoRotateAction.Enable();
	}

	void OnDisable()
	{
		if (toggleAutoRotateAction != null)
			toggleAutoRotateAction.Disable();
	}

	void Update()
	{
		if (toggleAutoRotateAction.triggered)
			autoRotate = !autoRotate;

		if (Mouse.current.scroll.ReadValue().y != 0)
		{
			if (Mouse.current.scroll.ReadValue().y > 0)
				camera.fieldOfView -= 150 * Time.deltaTime;
			else if (Mouse.current.scroll.ReadValue().y < 0)
				camera.fieldOfView += 150 * Time.deltaTime;
			camera.fieldOfView = Mathf.Clamp(camera.fieldOfView, minZoom, maxZoom);
		}
	}

	// Update is called once per frame
	void LateUpdate()
	{
		if (noPivot)
			return;

		if (autoRotate)
		{
			switch ((int)observationAxis)
			{
				case 0:
					observationPivot.Rotate(Vector3.right, rotationSpeed * Time.deltaTime);
					break;
				case 1:
					observationPivot.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
					break;
				case 2:
					observationPivot.Rotate(Vector3.forward, rotationSpeed * Time.deltaTime);
					break;
				case 3:
					observationPivot.Rotate(new Vector3(1, 1, 0), rotationSpeed * Time.deltaTime);
					break;
				case 4:
					observationPivot.Rotate(new Vector3(1, 0, 1), rotationSpeed * Time.deltaTime);
					break;
				case 5:
					observationPivot.Rotate(new Vector3(0, 1, 1), rotationSpeed * Time.deltaTime);
					break;
			}
		}
		else
		{
			if (Keyboard.current.aKey.isPressed)
			{
				observationPivot.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
			}
			if (Keyboard.current.dKey.isPressed)
			{
				observationPivot.Rotate(Vector3.up, -rotationSpeed * Time.deltaTime);
			}
			if (Keyboard.current.wKey.isPressed)
			{
				observationPivot.Rotate(Vector3.right, rotationSpeed * Time.deltaTime);
			}
			if (Keyboard.current.sKey.isPressed)
			{
				observationPivot.Rotate(Vector3.right, -rotationSpeed * Time.deltaTime);
			}
			Vector3 currentRotation = observationPivot.eulerAngles;
			currentRotation.z = 0;
			observationPivot.rotation = Quaternion.Euler(currentRotation);
		}
	}

	public void SetObservationPivot(Transform pivot)
	{
		if (observationPivotCache == null)
			observationPivotCache = observationPivot;
		if (pivot == null)
		{
			observationPivot = observationPivotCache;
		}
		else
		{
			observationPivot = pivot;
		}
		transform.position = observationPivot.position;
		transform.rotation = Quaternion.LookRotation((observationPivot.position - transform.position).normalized);
		transform.position -= transform.forward * obervationDistance;
		transform.position += Vector3.up * yAxisOffset;
		transform.rotation = Quaternion.LookRotation((observationPivot.position - transform.position).normalized);
		transform.SetParent(observationPivot);
	}

	enum ObservationAxis
	{
		X,
		Y,
		Z,
		XY,
		XZ,
		YZ
	}
}