using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoidAvoidObstacle : BoidBehaviourBase
{
	public BoidAvoidObstacle(BoidsController controller, BoidBehaviourSettings settings)
	{
		this.controller = controller;
		this.settings = settings;
	}

	public override Vector3 GetMovement()
	{
		if (neighbours == null || neighbours.Length == 0)
		{
			return controller.transform.forward;
		}

		Vector3 movement = Vector3.zero;
		bool addedToMovement = false;

		foreach (Collider neighbour in neighbours)
		{
			Vector3 closest = neighbour.ClosestPoint(controller.transform.position);
			Vector3 boidToClosest = (closest - controller.transform.position);
			movement -= boidToClosest;
			addedToMovement = true;
		}

		if (!addedToMovement)
		{
			return movement;
		}

		movement /= neighbours.Length;
		movement *= settings.behaviourWeight;

		return movement;
	}

	public override void SetNeighbourPayload(Collider[] neighbours)
	{
		List<Collider> acceptedNeighbours = new List<Collider>();
		for (int i = neighbours.Length - 1; i >= 0; i--)
		{
			if (neighbours[i].transform == null)
				continue;
			if (neighbours[i].gameObject.layer == LayerMask.NameToLayer("Obstacle"))
			{
				Vector3 closest = neighbours[i].ClosestPoint(controller.transform.position);
				Vector3 boidToClosest = (closest - controller.transform.position).normalized;
				if (Vector3.Distance(closest, controller.transform.position) <= settings.behaviourRadius && Vector3.Dot(controller.transform.forward, boidToClosest) > settings.behaviourFOVRadius)
				{
					acceptedNeighbours.Add(neighbours[i]);
				}
			}
		}
		this.neighbours = acceptedNeighbours.ToArray();
	}

	public override void OnDrawGizmos() { }
}