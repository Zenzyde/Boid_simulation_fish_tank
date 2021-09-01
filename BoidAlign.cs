using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoidAlign : BoidBehaviourBase
{
	public BoidAlign(BoidsController controller, BoidBehaviourSettings settings)
	{
		this.controller = controller;
		this.settings = settings;
	}

	public override Vector3 GetMovement()
	{
		List<BoidsController> neighbours = controller.GetGridManager().GetObjectsWithinRangeOfGridUnit(controller.transform.position, settings.behaviourRadius);
		if (neighbours == null || neighbours.Count <= 1)
		{
			return Vector3.zero;
		}

		Vector3 movement = Vector3.zero;
		bool addedToMovement = false;
		foreach (BoidsController boid in neighbours)
		{
			if (boid == controller || Vector3.Distance(boid.transform.position, controller.transform.position) > settings.behaviourRadius)
				continue;
			movement += boid.GetVelocity().normalized;
			addedToMovement = true;
		}

		if (!addedToMovement)
		{
			return Vector3.zero;
		}

		movement /= neighbours.Count;
		movement *= settings.behaviourWeight;
		return movement;
	}
}
