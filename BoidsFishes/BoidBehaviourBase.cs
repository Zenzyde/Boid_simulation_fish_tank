using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BoidBehaviourBase
{
	protected BoidsController controller;
	protected BoidBehaviourSettings settings;
	protected Vector3 target;
	protected Vector3 unassignedTarget = new Vector3(-10000, -10000, -10000);
	protected Vector3 lastMovement;
	protected Collider[] neighbours = null;
	protected float timeToTarget = 3f, elapsedTargetTime = 0;
	protected int targetAssignAttempts = 50;

	public abstract Vector3 GetMovement();
	public virtual void SetNeighbourPayload(Collider[] neighbours)
	{
		List<Collider> acceptedNeighbours = new List<Collider>();
		for (int i = neighbours.Length - 1; i >= 0; i--)
		{
			if (neighbours[i].transform == null)
				continue;
			if (Vector3.Distance(neighbours[i].transform.position, controller.transform.position) <= settings.behaviourRadius)
				if (neighbours[i].transform != controller.transform.GetChild(0))
					acceptedNeighbours.Add(neighbours[i]);
		}
		this.neighbours = acceptedNeighbours.ToArray();
	}
	public virtual BoidBehaviourSettings GetSettings() => settings;
	public abstract void OnDrawGizmos();
}

[System.Serializable]
public struct BoidBehaviourSettings
{
	public float behaviourWeight;
	public float behaviourRadius;
	public float behaviourGridRadius;
	public float behaviourFOVRadius;
}