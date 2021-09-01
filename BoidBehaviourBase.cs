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
	protected float timeToTarget = 3f, elapsedTargetTime = 0;

	public abstract Vector3 GetMovement();
}

[System.Serializable]
public struct BoidBehaviourSettings
{
	public float behaviourWeight;
	public float behaviourRadius;
	public float behaviourFOVRadius;
}