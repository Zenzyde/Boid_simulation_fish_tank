using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoidCohesion : BoidBehaviourBase
{
	public BoidCohesion(BoidsController controller, BoidBehaviourSettings settings)
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

		foreach (Collider collider in neighbours)
		{
			if (collider.transform.parent.TryGetComponent(out BoidsController boid))
			{
				if (Vector3.Dot(controller.transform.forward, (boid.transform.position - controller.transform.position).normalized) <= settings.behaviourFOVRadius)
					continue;
				movement += (boid.transform.position - controller.transform.position);
				addedToMovement = true;
			}
		}

		if (!addedToMovement)
		{
			return controller.transform.forward;
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
			if (neighbours[i].gameObject.layer == LayerMask.NameToLayer("Boid"))
			{
				if (Vector3.Distance(neighbours[i].transform.position, controller.transform.position) <= settings.behaviourRadius)
				{
					acceptedNeighbours.Add(neighbours[i]);
				}
			}
		}
		this.neighbours = acceptedNeighbours.ToArray();
	}

	public override void OnDrawGizmos()
	{
		Gizmos.color = Color.green;
		Gizmos.DrawWireSphere(controller.transform.position, settings.behaviourRadius);
	}
}