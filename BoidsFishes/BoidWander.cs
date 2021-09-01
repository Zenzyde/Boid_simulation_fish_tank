using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoidWander : BoidBehaviourBase
{
	public BoidWander(BoidsController controller, BoidBehaviourSettings settings)
	{
		this.controller = controller;
		this.settings = settings;
	}

	public override Vector3 GetMovement()
	{
		Vector3 movement = Vector3.zero;

		if (controller.GetManager().IsOutsideTank(controller))
		{
			return movement;
		}

		Vector3 wanderPoint = controller.transform.position + UnityEngine.Random.insideUnitSphere * settings.behaviourWeight;
		Vector3 wanderDirection = (wanderPoint - controller.transform.position).normalized;

		int assignAttempts = 5;
		while (Vector3.Dot(controller.transform.forward, wanderDirection) < 0 && assignAttempts > 0)
		{
			wanderPoint = controller.transform.position + UnityEngine.Random.insideUnitSphere * settings.behaviourWeight;
			wanderDirection = (wanderPoint - controller.transform.position).normalized;

			assignAttempts--;
		}

		movement = wanderDirection * settings.behaviourWeight;

		return movement;
	}
}