using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoidsController : MonoBehaviour
{
	[SerializeField] private HashSet<BoidBehaviourBase> activeBoidBehaviours = new HashSet<BoidBehaviourBase>();

	private Vector3 preVelocity;
	private Vector3 currentVelocity;
	private BoidsManager manager;
	private float maxVelocity;

	void Update()
	{
		//Assign proper rotation (A -> B = B - A) -> look rotation
		Vector3 lookRotation = (transform.position + currentVelocity - transform.position).normalized;
		transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(lookRotation), 0.5f);
		manager.GetGridManager().UpdateGrid(transform.position, this);
	}

	// Update is called once per frame
	void FixedUpdate()
	{
		if (activeBoidBehaviours.Count == 0)
		{
			return;
		}

		//currentVelocity = Vector3.zero;

		//Calculate acceleration/deacceleration of all active behaviours
		foreach (BoidBehaviourBase boidBehaviour in activeBoidBehaviours)
		{
			Vector3 newVelocity = boidBehaviour.GetMovement();
			currentVelocity += newVelocity - preVelocity * Time.fixedDeltaTime;
		}

		//Limit velocity
		if (currentVelocity.sqrMagnitude > maxVelocity * maxVelocity)
			currentVelocity = Vector3.ClampMagnitude(currentVelocity, maxVelocity);

		//Apply new velocity to position
		transform.position += currentVelocity * Time.fixedDeltaTime;
		//Save current velocity to old velocity for next calc-cycle
		preVelocity = currentVelocity;
	}

	public Vector3 GetVelocity() => currentVelocity;

	public BoidsManager GetManager() => manager;

	public GridManager<BoidsController> GetGridManager() => manager.GetGridManager();

	public void AddBoidBehaviour(BoidBehaviourBase behaviour)
	{
		activeBoidBehaviours.Add(behaviour);
	}

	public void InitializeController(BoidsManager manager, float maxVelocity)
	{
		this.manager = manager;
		this.maxVelocity = maxVelocity;
	}
}
