using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoidAvoidTank : BoidBehaviourBase
{
	public BoidAvoidTank(BoidsController controller, BoidBehaviourSettings settings)
	{
		this.controller = controller;
		this.settings = settings;
		timeToTarget = 2f;
	}

	public override Vector3 GetMovement()
	{
		if (elapsedTargetTime < timeToTarget)
		{
			elapsedTargetTime += Time.deltaTime;
			return lastMovement;
		}

		Vector3 wanderPoint;

		Vector3 movement = Vector3.zero;

		int repositionAttempts = 5;
		while (controller.GetManager().IsOutsideTank(controller.transform.position + movement) && repositionAttempts > 0)
		{
			if (!controller.GetManager().IsOutsideTank(controller.transform.position))
			{
				lastMovement = movement;
				return movement;
			}
			wanderPoint = controller.GetManager().GetRandomTankPosition(controller);
			movement = (wanderPoint - controller.transform.position).normalized * settings.behaviourWeight;
			repositionAttempts--;
		}

		lastMovement = movement;

		if (elapsedTargetTime > timeToTarget)
			elapsedTargetTime = 0;

		return movement;
	}
}
