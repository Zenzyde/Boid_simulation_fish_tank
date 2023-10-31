using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoidWander : BoidBehaviourBase
{
	Vector3 wanderPoint;

	public BoidWander(BoidsController controller, BoidBehaviourSettings settings)
	{
		this.controller = controller;
		this.settings = settings;
		timeToTarget = 1.3f;
	}

	public override Vector3 GetMovement()
	{
		Vector3 movement = Vector3.zero;

		if (controller.GetManager().IsOutsideTank(controller))
		{
			elapsedTargetTime = timeToTarget * 2;
			return controller.transform.forward;
		}

		if (elapsedTargetTime > 0)
			elapsedTargetTime -= Time.deltaTime;
		else if (elapsedTargetTime <= 0)
		{
			wanderPoint = controller.transform.position + controller.transform.forward * settings.behaviourRadius + Random.onUnitSphere * (settings.behaviourRadius / 2.0f);
			elapsedTargetTime = timeToTarget;
		}

		Vector3 wanderDirection = (wanderPoint - controller.transform.position).normalized;

		movement = wanderDirection * settings.behaviourWeight;

		return movement;
	}

	public override void OnDrawGizmos() { }
}