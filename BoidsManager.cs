using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;

public class BoidsManager : MonoBehaviour
{
	[SerializeField] private Vector3 gridSize;
	[SerializeField] private GameObject[] boidModels;
	[SerializeField] private float maxVelocity;
	[SerializeField] private float gridManagerSizeX, gridManagerSizeY, gridManagerSizeZ;
	[SerializeField] private float gridManagerUnitRadius;
	[SerializeField] private Text numBoidsText;
	[SerializeField] private bool drawBoxGizmo, drawGridGizmo, drawInhabitedGridsGizmos;

	private List<BoidsController> activeBoids = new List<BoidsController>();

	private BoidBehaviourSettings settings = new BoidBehaviourSettings();

	private GridManager<BoidsController> grid;

#if UNITY_EDITOR
	void OnDrawGizmos()
	{
		Gizmos.color = Color.blue;
		if (drawBoxGizmo)
			HelperMethods.DrawBoxBounds(transform.position, transform.position + gridSize);
		if (grid != null)
		{
			if (drawGridGizmo)
				grid.OnDrawGizmos();
			if (drawInhabitedGridsGizmos)
				grid.DrawInhabitedGridGizmos();
		}
	}
#endif

	void Start()
	{
		UnityEngine.Random.InitState(DateTime.Now.Second);
		grid = new GridManager<BoidsController>((transform.position + (transform.position + gridSize)) / 2.0f, gridManagerSizeX, gridManagerSizeY, gridManagerSizeZ, gridManagerUnitRadius);
		numBoidsText.text = string.Format("Num active boids: {0}", activeBoids.Count);
	}

	void Update()
	{
		if (Input.GetMouseButtonDown(0))
		{
			int numBoids = UnityEngine.Random.Range(2, 6);
			SpawnNewBoid(numBoids);
			numBoidsText.text = string.Format("Num active boids: {0}", activeBoids.Count);
		}
		else if (Input.GetMouseButtonDown(1) && activeBoids.Count > 0)
		{
			RemoveBoid();
			numBoidsText.text = string.Format("Num active boids: {0}", activeBoids.Count);
		}

		if (Input.GetKeyDown(KeyCode.Alpha1))
			drawBoxGizmo = !drawBoxGizmo;
		if (Input.GetKeyDown(KeyCode.Alpha2))
			drawGridGizmo = !drawGridGizmo;
		if (Input.GetKeyDown(KeyCode.Alpha3))
			drawInhabitedGridsGizmos = !drawInhabitedGridsGizmos;
	}

	public void SpawnNewBoid(int numBoids)
	{
		for (int i = 0; i < numBoids; i++)
		{
			GameObject boidObj = Instantiate(boidModels[UnityEngine.Random.Range(0, boidModels.Length)]);
			boidObj.transform.position = new Vector3()
			{
				x = UnityEngine.Random.Range(transform.position.x, transform.position.x + gridSize.x),
				y = UnityEngine.Random.Range(transform.position.y, transform.position.y + gridSize.y),
				z = UnityEngine.Random.Range(transform.position.z, transform.position.z + gridSize.z),
			};
			Vector3 dir = ((boidObj.transform.position + UnityEngine.Random.insideUnitSphere * 2) - boidObj.transform.position).normalized;
			boidObj.transform.rotation = Quaternion.LookRotation(dir);

			BoidsController boid = boidObj.AddComponent<BoidsController>();
			boid.InitializeController(this, maxVelocity);

			settings.behaviourRadius = UnityEngine.Random.Range(5.5f, 7f);
			settings.behaviourWeight = 4.5f;
			boid.AddBoidBehaviour(new BoidAlign(boid, settings));

			settings.behaviourWeight = 3;
			boid.AddBoidBehaviour(new BoidCohesion(boid, settings));

			settings.behaviourRadius = settings.behaviourRadius - UnityEngine.Random.Range(.5f, 2f);
			settings.behaviourWeight = 7f;
			boid.AddBoidBehaviour(new BoidSeparate(boid, settings));

			settings.behaviourWeight = .5f;
			boid.AddBoidBehaviour(new BoidWander(boid, settings));

			settings.behaviourWeight = 5;
			boid.AddBoidBehaviour(new BoidAvoidTank(boid, settings));

			activeBoids.Add(boid);

			grid.AddObject(boidObj.transform.position, boid);
		}
	}

	public void RemoveBoid()
	{
		int random = UnityEngine.Random.Range(0, activeBoids.Count);
		grid.RemoveObject(activeBoids[random]);
		Destroy(activeBoids[random].gameObject);
		activeBoids.RemoveAt(random);
	}

	public bool IsOutsideTank(BoidsController boid)
	{
		Vector3 pos = boid.transform.position;
		return pos.x < transform.position.x || pos.x > transform.position.x + gridSize.x ||
			pos.y < transform.position.y || pos.y > transform.position.y + gridSize.y ||
			pos.z < transform.position.z || pos.z > transform.position.z + gridSize.z;
	}

	public bool IsOutsideTank(Vector3 pos)
	{
		return pos.x < transform.position.x || pos.x > transform.position.x + gridSize.x ||
			pos.y < transform.position.y || pos.y > transform.position.y + gridSize.y ||
			pos.z < transform.position.z || pos.z > transform.position.z + gridSize.z;
	}

	public Vector3 GetRandomTankPosition(BoidsController boid)
	{
		Vector3 rand = (transform.position + gridSize / 2f) + UnityEngine.Random.insideUnitSphere * (Mathf.Min(Mathf.Min(gridSize.x, gridSize.y), gridSize.z) * .7f);

		int randomizationAttempts = 5;
		while (Vector3.Distance(rand, boid.transform.position) < 3f && randomizationAttempts > 0)
		{
			rand = (transform.position + gridSize / 2f) + UnityEngine.Random.insideUnitSphere * (Mathf.Min(Mathf.Min(gridSize.x, gridSize.y), gridSize.z) * .7f);
			randomizationAttempts--;
		}

		return rand;
	}

	public GridManager<BoidsController> GetGridManager() => grid;

}

public enum EBoidsBehaviour
{
	cohesion = 1, separation = 2, alignment = 0
}