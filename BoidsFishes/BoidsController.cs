using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoidsController : MonoBehaviour
{
	[SerializeField] private HashSet<BoidBehaviourBase> activeBoidBehaviours = new HashSet<BoidBehaviourBase>();

	private BoidsManager manager;
	private float maxVelocity;

	// velocity verlet integration ref: https://github.com/joaen/verlet-cloth-simulation/blob/main/Assets/Scripts/Verlet.cs
	private Vector3 oldPos;
	private Vector3 velocity;

	private float physicsOverlapRadius;

	void Update()
	{
		transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(velocity.normalized), 0.85f);
	}

	// Update is called once per frame
	void FixedUpdate()
	{
		if (activeBoidBehaviours.Count == 0)
			return;

		velocity = transform.position - oldPos;

		Collider[] neighbours = Physics.OverlapSphere(transform.position, physicsOverlapRadius);

		//Calculate acceleration/deacceleration of all active behaviours
		foreach (BoidBehaviourBase boidBehaviour in activeBoidBehaviours)
		{
			boidBehaviour.SetNeighbourPayload(neighbours);
			velocity += boidBehaviour.GetMovement();
		}

		oldPos = transform.position;

		//Limit velocity
		velocity = Vector3.ClampMagnitude(velocity, maxVelocity);

		//Apply new velocity to position
		transform.position += velocity * Time.fixedDeltaTime;
	}

	public void DrawGizmos()
	{
		foreach (BoidBehaviourBase boidBehaviour in activeBoidBehaviours)
			boidBehaviour.OnDrawGizmos();
	}

	public Vector3 GetVelocity() => velocity;

	public BoidsManager GetManager() => manager;

	public void AddBoidBehaviour(BoidBehaviourBase behaviour)
	{
		activeBoidBehaviours.Add(behaviour);

		foreach (BoidBehaviourBase boidBehaviour in activeBoidBehaviours)
		{
			if (boidBehaviour.GetSettings().behaviourRadius > physicsOverlapRadius)
				physicsOverlapRadius = boidBehaviour.GetSettings().behaviourRadius;
		}
	}

	public void InitializeController(BoidsManager manager, float maxVelocity)
	{
		this.manager = manager;
		this.maxVelocity = maxVelocity;
	}
}