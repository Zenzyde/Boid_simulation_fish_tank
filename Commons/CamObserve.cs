using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamObserve : MonoBehaviour
{
	[SerializeField] private float rotationSpeed, obervationDistance, yAxisOffset;
	[SerializeField] private ObservationAxis observationAxis;
	[SerializeField] private Transform observationPivot;

	bool noPivot = false;

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
	}

	// Update is called once per frame
	void LateUpdate()
	{
		if (noPivot)
			return;

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

	enum ObservationAxis
	{
		X, Y, Z, XY, XZ, YZ
	}
}
