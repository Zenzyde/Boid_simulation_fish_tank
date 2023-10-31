using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class BoidBehaviorSettings : ScriptableObject
{
	public EBoidsBehaviour behavior;
	public float behaviourWeight;
	public float behaviourRadius;
	public float behaviourGridRadius;
	public float behaviourFOVRadius;
}

public enum EBoidsBehaviour
{
	cohesion = 0,
	separation = 1,
	alignment = 2,
	avoidTank = 3,
	wander = 4,
	avoidObstacle = 5
}